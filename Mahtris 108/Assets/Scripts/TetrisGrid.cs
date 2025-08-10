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

    // ��ȡĳ������С����� blockId �б�
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

    // ��Ⲣ��ȡ���Ӻ�˳��
    // �����ѱ��Ƴ�(��ȡ��������)��blockId�б�
    public static List<int> DetectAndExtractSets(List<int> blockIdsInRow, HuPaiArea huPaiArea)
    {
        List<int> removedIds = new List<int>();
        if (huPaiArea == null) return removedIds;

        // ��ͳ���齫�ƶ�Ӧ�����ͺ�����
        // Mahjong����ӳ�䰴27����ѭ������3����ɫ��ÿ����ɫ9����
        // blockId % 27 ��ʾ�齫�����ͣ�0~26
        // type: 0-8  Ͳ�� (Tong), 9-17 ���� (Wan), 18-26 ���� (Tiao)
        // num: 1~9 ���������%9 +1��

        Dictionary<int, int> countMap = new Dictionary<int, int>();
        foreach (var id in blockIdsInRow)
        {
            int tileType = id % 27;
            if (!countMap.ContainsKey(tileType))
                countMap[tileType] = 0;
            countMap[tileType]++;
        }

        // 1. �����ӣ�3����ͬtileType
        foreach (var kvp in countMap)
        {
            int tileType = kvp.Key;
            int count = kvp.Value;
            while (count >= 3)
            {
                // ��3����Ӧ��blockId�����������
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

        // 2. ���˳�ӣ�ͬһ��ɫ������������
        // ��ɫ��tileType / 9�����֣�tileType % 9 + 1
        // ����ɫ������˳��

        Dictionary<int, List<int>> tilesBySuit = new Dictionary<int, List<int>>();
        foreach (var id in blockIdsInRow)
        {
            if (removedIds.Contains(id)) continue; // ����ȡ������
            int tileType = id % 27;
            int suit = tileType / 9; // 0,1,2
            if (!tilesBySuit.ContainsKey(suit))
                tilesBySuit[suit] = new List<int>();
            tilesBySuit[suit].Add(tileType);
        }

        // ��ÿ����ɫ������˳��
        foreach (var kvp in tilesBySuit)
        {
            var tileTypes = kvp.Value;
            tileTypes.Sort();

            for (int i = 0; i < tileTypes.Count - 2; i++)
            {
                int t1 = tileTypes[i];
                int t2 = tileTypes[i + 1];
                int t3 = tileTypes[i + 2];

                // ��������
                if (t2 == t1 + 1 && t3 == t2 + 1)
                {
                    // ��blockIdsInRow�Ҷ�Ӧ��blockId����δ����ȡ
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
                        // �����Ѽ���˳�ӣ������ظ�ʶ��
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

                // ��ȡ����������
                HuPaiArea huPaiArea = HuPaiArea.Instance;

                List<int> removedFromLine = DetectAndExtractSets(blockIdsInRow, huPaiArea);

                // ʣ����ƻ���
                List<int> toRecycle = new List<int>();
                foreach (var id in blockIdsInRow)
                {
                    if (!removedFromLine.Contains(id))
                        toRecycle.Add(id);
                }

                DeleteRowWithBlocks(y, toRecycle);

                // ����
                BlockPool.Instance.ReturnBlockIds(toRecycle);

                DecreaseRowsAbove(y + 1);
                --y;
                rowsCleared++;
            }
        }
        return rowsCleared;
    }
}
