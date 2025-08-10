using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetrisGrid : MonoBehaviour
{
    public static int width = 10;
    public static int height = 20;
    public static Transform[,] grid = new Transform[width, height];

    public static Vector2 RoundVector2(Vector2 v)
    {
        return new Vector2(Mathf.Round(v.x), Mathf.Round(v.y));
    }

    public static bool InsideBorder(Vector2 pos)
    {
        return ((int)pos.x >= 0 && (int)pos.x < width && (int)pos.y >= 0);
    }

    public static void DeleteRowWithBlocks(int y, List<int> blockIdsToDelete)
    {
        for (int x = 0; x < width; ++x)
        {
            if (grid[x, y] != null)
            {
                BlockUnit block = grid[x, y].GetComponent<BlockUnit>();
                if (block != null && blockIdsToDelete.Contains(block.blockId))
                {
                    GameObject.Destroy(grid[x, y].gameObject);
                    grid[x, y] = null;
                }
            }
        }
    }

    public static void DecreaseRow(int y)
    {
        for (int x = 0; x < width; ++x)
        {
            if (grid[x, y] != null)
            {
                grid[x, y - 1] = grid[x, y];
                grid[x, y] = null;
                grid[x, y - 1].position += new Vector3(0, -1, 0);
            }
        }
    }

    public static void DecreaseRowsAbove(int y)
    {
        for (int i = y; i < height; ++i)
        {
            DecreaseRow(i);
        }
    }

    public static bool IsRowFull(int y)
    {
        for (int x = 0; x < width; ++x)
        {
            if (grid[x, y] == null)
                return false;
        }
        return true;
    }

    // 获取某行所有小方块的 blockId 列表
    public static List<int> GetBlockIdsInRow(int y)
    {
        List<int> ids = new List<int>();
        for (int x = 0; x < width; ++x)
        {
            if (grid[x, y] != null)
            {
                BlockUnit block = grid[x, y].GetComponent<BlockUnit>();
                if (block != null)
                    ids.Add(block.blockId);
            }
        }
        return ids;
    }

    // 检测并提取刻子和顺子
    // 返回已被移除(提取到胡牌区)的blockId列表
    public static List<int> DetectAndExtractSets(List<int> blockIdsInRow, HuPaiArea huPaiArea)
    {
        List<int> removedIds = new List<int>();
        if (huPaiArea == null) return removedIds;

        // 先统计麻将牌对应的类型和数字
        // Mahjong类型映射按27张牌循环，分3个花色，每个花色9张牌
        // blockId % 27 表示麻将牌类型：0~26
        // type: 0-8  筒子 (Tong), 9-17 万子 (Wan), 18-26 条子 (Tiao)
        // num: 1~9 （类型序号%9 +1）

        Dictionary<int, int> countMap = new Dictionary<int, int>();
        foreach (var id in blockIdsInRow)
        {
            int tileType = id % 27;
            if (!countMap.ContainsKey(tileType))
                countMap[tileType] = 0;
            countMap[tileType]++;
        }

        // 1. 检测刻子：3个相同tileType
        foreach (var kvp in countMap)
        {
            int tileType = kvp.Key;
            int count = kvp.Value;
            while (count >= 3)
            {
                // 找3个对应的blockId加入刻子牌型
                List<int> set = new List<int>();
                int found = 0;
                for (int i = 0; i < blockIdsInRow.Count && found < 3; i++)
                {
                    if (!removedIds.Contains(blockIdsInRow[i]) && blockIdsInRow[i] % 27 == tileType)
                    {
                        set.Add(blockIdsInRow[i]);
                        found++;
                    }
                }
                if (set.Count == 3)
                {
                    huPaiArea.AddHuPaiSet(set);
                    removedIds.AddRange(set);
                    count -= 3;
                }
                else
                {
                    break;
                }
            }
        }

        // 2. 检测顺子：同一花色连续的三张牌
        // 花色：tileType / 9，数字：tileType % 9 + 1
        // 按花色分组找顺子

        Dictionary<int, List<int>> tilesBySuit = new Dictionary<int, List<int>>();
        foreach (var id in blockIdsInRow)
        {
            if (removedIds.Contains(id)) continue; // 已提取的跳过
            int tileType = id % 27;
            int suit = tileType / 9; // 0,1,2
            if (!tilesBySuit.ContainsKey(suit))
                tilesBySuit[suit] = new List<int>();
            tilesBySuit[suit].Add(tileType);
        }

        // 对每个花色排序检测顺子
        foreach (var kvp in tilesBySuit)
        {
            var tileTypes = kvp.Value;
            tileTypes.Sort();

            for (int i = 0; i < tileTypes.Count - 2; i++)
            {
                int t1 = tileTypes[i];
                int t2 = tileTypes[i + 1];
                int t3 = tileTypes[i + 2];

                // 连续数字
                if (t2 == t1 + 1 && t3 == t2 + 1)
                {
                    // 从blockIdsInRow找对应的blockId，且未被提取
                    List<int> set = new List<int>();
                    for (int j = 0; j < blockIdsInRow.Count && set.Count < 3; j++)
                    {
                        int id = blockIdsInRow[j];
                        if (!removedIds.Contains(id))
                        {
                            int tt = id % 27;
                            if (tt == t1 || tt == t2 || tt == t3)
                            {
                                if (!set.Contains(id))
                                    set.Add(id);
                            }
                        }
                    }
                    if (set.Count == 3)
                    {
                        huPaiArea.AddHuPaiSet(set);
                        removedIds.AddRange(set);
                        // 跳过已检测的顺子，避免重复识别
                        i += 2;
                    }
                }
            }
        }

        return removedIds;
    }

    public static int DeleteFullRows()
    {
        int rowsCleared = 0;
        for (int y = 0; y < height; ++y)
        {
            if (IsRowFull(y))
            {
                List<int> blockIdsInRow = GetBlockIdsInRow(y);

                // 获取胡牌区单例
                HuPaiArea huPaiArea = HuPaiArea.Instance;

                List<int> removedFromLine = DetectAndExtractSets(blockIdsInRow, huPaiArea);

                // 剩余的牌回收
                List<int> toRecycle = new List<int>();
                foreach (var id in blockIdsInRow)
                {
                    if (!removedFromLine.Contains(id))
                        toRecycle.Add(id);
                }

                DeleteRowWithBlocks(y, toRecycle);

                // 回收
                BlockPool.Instance.ReturnBlockIds(toRecycle);

                DecreaseRowsAbove(y + 1);
                --y;
                rowsCleared++;
            }
        }
        return rowsCleared;
    }
}
