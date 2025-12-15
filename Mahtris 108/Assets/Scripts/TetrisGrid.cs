// FileName: TetrisGrid.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TetrisGrid : MonoBehaviour
{
    private Transform[,] grid;
    private int width;
    private int height;
    private Spawner spawner;

    public void Initialize(GameSettings settings)
    {
        this.width = settings.gridWidth;
        this.height = settings.gridHeight;
        grid = new Transform[width, height];
    }

    public void RegisterSpawner(Spawner sp)
    {
        this.spawner = sp;
    }

    // 【新增方法】供炸弹道具调用
    public void ForceClearBottomRows(int count)
    {
        List<int> rowsToClear = new List<int>();
        for (int i = 0; i < count && i < height; i++)
        {
            // 检查这一行是否真的有方块，空的行不算
            bool hasBlocks = false;
            for (int x = 0; x < width; x++)
            {
                if (grid[x, i] != null)
                {
                    hasBlocks = true;
                    break;
                }
            }
            if (hasBlocks)
            {
                rowsToClear.Add(i);
            }
        }

        if (rowsToClear.Count > 0)
        {
            // 触发和普通消行完全一样的事件，让GameManager处理后续的计分和胡牌判定
            GameEvents.TriggerRowsCleared(rowsToClear);
        }
    }

    public Vector2 RoundVector2(Vector2 v) => new Vector2(Mathf.Round(v.x), Mathf.Round(v.y));

    public bool IsInsideBorder(Vector2 pos) => (int)pos.x >= 0 && (int)pos.x < width && (int)pos.y >= 0;

    public bool IsValidGridPos(Transform parent)
    {
        foreach (Transform child in parent)
        {
            Vector2 v = RoundVector2(child.position);
            if (!IsInsideBorder(v)) return false;
            if ((int)v.y >= height) continue; // 允许方块在游戏区域上方生成

            if (grid[(int)v.x, (int)v.y] != null && grid[(int)v.x, (int)v.y].parent != parent)
                return false;
        }
        return true;
    }

    public void UpdateGrid(Transform parent)
    {
        for (int y = 0; y < height; ++y)
            for (int x = 0; x < width; ++x)
                if (grid[x, y] != null && grid[x, y].parent == parent)
                    grid[x, y] = null;

        foreach (Transform child in parent)
        {
            Vector2 v = RoundVector2(child.position);
            if (IsInsideBorder(v) && (int)v.y < height)
            {
                grid[(int)v.x, (int)v.y] = child;
            }
        }
    }

    public void CheckForFullRows()
    {
        var fullRows = new List<int>();
        for (int y = 0; y < height; y++)
        {
            if (IsRowFull(y)) fullRows.Add(y);
        }

        if (fullRows.Count > 0)
        {
            GameEvents.TriggerRowsCleared(fullRows);
        }
        else
        {
            // 如果没有消行，则直接生成下一个方块
            if (spawner != null) spawner.SpawnBlock();
        }
    }

    private bool IsRowFull(int y)
    {
        for (int x = 0; x < width; x++)
            if (grid[x, y] == null) return false;
        return true;
    }

    public (List<int> blockIds, List<Transform> transforms) GetRowDataAndClear(int y)
    {
        var rowData = (blockIds: new List<int>(), transforms: new List<Transform>());
        for (int x = 0; x < width; x++)
        {
            if (grid[x, y] != null)
            {
                rowData.blockIds.Add(grid[x, y].GetComponent<BlockUnit>().blockId);
                rowData.transforms.Add(grid[x, y]);
                grid[x, y] = null;
            }
        }
        return rowData;
    }

    public void DestroyTransforms(List<Transform> transforms)
    {
        foreach (var t in transforms.Where(t => t != null))
            Destroy(t.gameObject);
    }

    public void CompactAllColumns(List<int> clearedRows)
    {
        clearedRows.Sort();
        for (int y = 0; y < height; y++)
        {
            int clearedRowsBelow = clearedRows.Count(clearedY => clearedY < y);
            if (clearedRowsBelow > 0)
            {
                for (int x = 0; x < width; x++)
                {
                    if (grid[x, y] != null)
                    {
                        grid[x, y - clearedRowsBelow] = grid[x, y];
                        grid[x, y] = null;
                        grid[x, y - clearedRowsBelow].position += new Vector3(0, -clearedRowsBelow, 0);
                    }
                }
            }
        }
    }

    public void ClearAllBlocks()
    {
        // 【新增】在清除数组前，主动查找并销毁所有带Tag的方块
        // 这可以清除上一局残留的，或者编辑器内误操作的方块
        GameObject[] blocks = GameObject.FindGameObjectsWithTag("PlayerBlock");
        foreach (GameObject block in blocks)
        {
            Destroy(block);
        }

        // 【保留】清除网格数组的逻辑
        if (grid == null) return;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (grid[x, y] != null)
                {
                    // 检查是否已被上一步销毁，避免二次销毁报错
                    if (grid[x, y].gameObject != null)
                    {
                        Destroy(grid[x, y].gameObject);
                    }
                    grid[x, y] = null;
                }
            }
        }
    }
    // 【新增】获取当前场上最高的方块高度 (用于子弹时间)
    public int GetMaxColumnHeight()
    {
        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                if (grid[x, y] != null) return y + 1;
            }
        }
        return 0;
    }
    // 【TetrisGrid.cs 新增方法】
    public void RemoveSpecificBlock(Transform blockTransform)
    {
        if (blockTransform == null) return;

        // 获取方块坐标
        Vector2 pos = RoundVector2(blockTransform.position);
        int x = (int)pos.x;
        int y = (int)pos.y;

        // 确认坐标在网格内，且该位置确实是我们要移除的方块
        if (IsInsideBorder(pos) && grid[x, y] == blockTransform)
        {
            Destroy(blockTransform.gameObject);
            grid[x, y] = null;
        }
    }
    public Transform GetBlockTransformByValue(int value0to26)
    {
        // 遍历所有格子
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (grid[x, y] != null)
                {
                    var unit = grid[x, y].GetComponent<BlockUnit>();
                    // 【关键修复】使用 % 27 进行严格数值比对
                    // 确保 5万 (id%27=4) 绝不会匹配到 6万 (id%27=5)
                    if (unit != null && (unit.blockId % 27) == value0to26)
                    {
                        return grid[x, y];
                    }
                }
            }
        }
        return null;
    }
    public void ForceClearTopRows(int count, Transform ignoreSource = null)
    {
        // 1. 从上往下扫，找到当前最高的【已锁定】方块在哪一行
        int maxHeightIndex = -1;
        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                if (grid[x, y] != null)
                {
                    // 【关键修复】如果这个格子属于正在下落的方块，则忽略，继续找
                    if (ignoreSource != null && grid[x, y].parent == ignoreSource)
                    {
                        continue;
                    }

                    // 找到了非下落方块，标记为最高点
                    maxHeightIndex = y;
                    goto HeightFound; // 跳出双层循环
                }
            }
        }

    HeightFound:
        if (maxHeightIndex == -1)
        {
            Debug.Log("空投炸弹：场上没有已锁定的方块，无效。");
            return;
        }

        // 2. 计算要消除哪些行 (从 maxHeightIndex 向下数 count 行)
        List<int> rowsToClear = new List<int>();
        for (int i = 0; i < count; i++)
        {
            int targetY = maxHeightIndex - i;
            if (targetY >= 0)
            {
                rowsToClear.Add(targetY);
            }
        }

        // 3. 执行消除
        if (rowsToClear.Count > 0)
        {
            GameEvents.TriggerRowsCleared(rowsToClear);
            Debug.Log($"空投炸弹生效：消除了 {rowsToClear.Count} 行 (最高堆叠: {maxHeightIndex + 1})");
        }
    }
    public void ShuffleAllBoardTiles()
    {
        // 1. 收集场上所有的 BlockUnit
        List<BlockUnit> allUnits = new List<BlockUnit>();
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (grid[x, y] != null)
                {
                    var unit = grid[x, y].GetComponent<BlockUnit>();
                    if (unit != null) allUnits.Add(unit);
                }
            }
        }

        if (allUnits.Count == 0) return;

        // 2. 提取所有 ID 并洗牌 (只交换场上的牌，不引入新牌)
        List<int> shuffledIds = allUnits.Select(u => u.blockId).ToList();

        // Fisher-Yates 洗牌算法
        for (int i = 0; i < shuffledIds.Count; i++)
        {
            int temp = shuffledIds[i];
            int randIndex = Random.Range(i, shuffledIds.Count);
            shuffledIds[i] = shuffledIds[randIndex];
            shuffledIds[randIndex] = temp;
        }

        // 3. 重新赋值
        var pool = GameManager.Instance.BlockPool;
        for (int i = 0; i < allUnits.Count; i++)
        {
            allUnits[i].Initialize(shuffledIds[i], pool);
        }
    }
}