using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TetrisGrid : MonoBehaviour
{
    private Transform[,] grid;
    private int width;
    private int height;

    public void Initialize(GameSettings settings)
    {
        this.width = settings.gridWidth;
        this.height = settings.gridHeight;
        grid = new Transform[width, height];
    }

    // --- 【修正点】---
    // 确保这些方法是 public，以便 Tetromino 可以调用
    public Vector2 RoundVector2(Vector2 v) => new Vector2(Mathf.Round(v.x), Mathf.Round(v.y));

    public bool IsInsideBorder(Vector2 pos) => (int)pos.x >= 0 && (int)pos.x < width && (int)pos.y >= 0;

    public bool IsValidGridPos(Transform parent)
    {
        foreach (Transform child in parent)
        {
            Vector2 v = RoundVector2(child.position);
            if (!IsInsideBorder(v)) return false;

            if ((int)v.y < height && grid[(int)v.x, (int)v.y] != null && grid[(int)v.x, (int)v.y].parent != parent)
            {
                return false;
            }
        }
        return true;
    }

    public void UpdateGrid(Transform parent)
    {
        // 清理旧位置
        for (int y = 0; y < height; ++y)
            for (int x = 0; x < width; ++x)
                if (grid[x, y] != null && grid[x, y].parent == parent)
                    grid[x, y] = null;

        // 写入新位置
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
            if (IsRowFull(y))
            {
                fullRows.Add(y);
            }
        }

        if (fullRows.Count > 0)
        {
            GameEvents.TriggerRowsCleared(fullRows);
        }
        else
        {
            // 如果没有消行，则直接生成下一个方块
            FindObjectOfType<Spawner>().SpawnBlock();
        }
    }

    private bool IsRowFull(int y)
    {
        for (int x = 0; x < width; x++)
        {
            if (grid[x, y] == null) return false;
        }
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
        foreach (var t in transforms)
        {
            if (t != null) Destroy(t.gameObject);
        }
    }

    public void CompactAllColumns()
    {
        for (int x = 0; x < width; x++)
        {
            int emptyCellY = 0;
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] != null)
                {
                    if (emptyCellY != y)
                    {
                        grid[x, emptyCellY] = grid[x, y];
                        grid[x, y] = null;
                        grid[x, emptyCellY].position = new Vector2(x, emptyCellY);
                    }
                    emptyCellY++;
                }
            }
        }
    }

    public void ClearAllBlocks()
    {
        if (grid == null) return;
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                if (grid[x, y] != null)
                {
                    Destroy(grid[x, y].gameObject);
                    grid[x, y] = null;
                }
            }
    }
}
