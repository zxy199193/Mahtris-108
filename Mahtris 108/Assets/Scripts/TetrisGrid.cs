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
}