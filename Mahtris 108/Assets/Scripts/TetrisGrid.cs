using System.Collections.Generic;
using UnityEngine;

public class TetrisGrid : MonoBehaviour
{
    public static int width = 10;
    public static int height = 20;
    public static Transform[,] grid = new Transform[width, height];

    public static bool huPending = false;

    public static Vector2 RoundVector2(Vector2 v) => new Vector2(Mathf.Round(v.x), Mathf.Round(v.y));
    public static bool InsideBorder(Vector2 pos) => (int)pos.x >= 0 && (int)pos.x < width && (int)pos.y >= 0;

    public static bool IsRowFull(int y)
    {
        for (int x = 0; x < width; x++)
            if (grid[x, y] == null) return false;
        return true;
    }

    public static List<int> GetBlockIdsInRow(int y)
    {
        var ids = new List<int>();
        for (int x = 0; x < width; x++)
        {
            if (grid[x, y] != null)
            {
                var bu = grid[x, y].GetComponent<BlockUnit>();
                if (bu != null) ids.Add(bu.blockId);
            }
        }
        return ids;
    }

    private static Transform FindTransformByBlockId(int blockId)
    {
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                if (grid[x, y] != null)
                {
                    var bu = grid[x, y].GetComponent<BlockUnit>();
                    if (bu != null && bu.blockId == blockId) return grid[x, y];
                }
            }
        return null;
    }

    public static List<List<int>> DetectSetsInRow(int y)
    {
        var sets = new List<List<int>>();
        var ids = GetBlockIdsInRow(y);

        var byTile = new Dictionary<int, List<int>>();
        foreach (var id in ids)
        {
            int tile = id % 27;
            if (!byTile.ContainsKey(tile)) byTile[tile] = new List<int>();
            byTile[tile].Add(id);
        }

        foreach (var kv in new List<KeyValuePair<int, List<int>>>(byTile))
        {
            var list = kv.Value;
            while (list.Count >= 3)
            {
                sets.Add(new List<int>() { list[0], list[1], list[2] });
                list.RemoveRange(0, 3);
            }
        }

        var tileToBlockIds = new Dictionary<int, List<int>>();
        foreach (var id in ids)
        {
            int tile = id % 27;
            if (!tileToBlockIds.ContainsKey(tile)) tileToBlockIds[tile] = new List<int>();
            tileToBlockIds[tile].Add(id);
        }

        for (int suit = 0; suit < 3; suit++)
        {
            int startTile = suit * 9;
            int endTile = startTile + 8;
            for (int t = startTile; t <= endTile - 2; t++)
            {
                if (tileToBlockIds.ContainsKey(t) && tileToBlockIds.ContainsKey(t + 1) && tileToBlockIds.ContainsKey(t + 2))
                {
                    int id1 = tileToBlockIds[t].Find(x => true);
                    int id2 = tileToBlockIds[t + 1].Find(x => true);
                    int id3 = tileToBlockIds[t + 2].Find(x => true);
                    if (id1 >= 0 && id2 >= 0 && id3 >= 0)
                    {
                        sets.Add(new List<int>() { id1, id2, id3 });
                        tileToBlockIds[t].Remove(id1);
                        tileToBlockIds[t + 1].Remove(id2);
                        tileToBlockIds[t + 2].Remove(id3);
                        if (tileToBlockIds[t].Count == 0) tileToBlockIds.Remove(t);
                        if (tileToBlockIds.ContainsKey(t + 1) && tileToBlockIds[t + 1].Count == 0) tileToBlockIds.Remove(t + 1);
                        if (tileToBlockIds.ContainsKey(t + 2) && tileToBlockIds[t + 2].Count == 0) tileToBlockIds.Remove(t + 2);
                    }
                }
            }
        }

        return sets;
    }

    private static void MoveTransformToGridPos(Transform t, int x, int y)
    {
        if (t == null) return;
        float z = t.position.z;
        t.position = new Vector3(x, y, z);
    }

    public static void CompactColumns()
    {
        for (int x = 0; x < width; x++)
        {
            int targetY = 0;
            for (int y = 0; y < height; y++)
            {
                var t = grid[x, y];
                if (t != null)
                {
                    if (y != targetY)
                    {
                        grid[x, targetY] = t;
                        grid[x, y] = null;
                        MoveTransformToGridPos(t, x, targetY);

                        var bu = t.GetComponent<BlockUnit>();
                        if (bu != null)
                            Debug.Log($"[TetrisGrid] Compact move id={bu.blockId} ({x},{y}) -> ({x},{targetY})");
                    }
                    targetY++;
                }
            }
            for (int y = targetY; y < height; y++) grid[x, y] = null;
        }
    }

    private static void ClearAllBlocksOnBoardNoReturn()
    {
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

    // PerformHuFinalize: 真正执行清盘、重置牌库并生成下一块（由玩家点击弹窗确认后调用）
    public static void PerformHuFinalize()
    {
        Debug.Log("[TetrisGrid] PerformHuFinalize: finalizing hu - clear board & reset deck");

        ClearAllBlocksOnBoardNoReturn();

        if (HuPaiArea.Instance != null) HuPaiArea.Instance.ClearAll();
        if (BlockPool.Instance != null) BlockPool.Instance.ResetFullDeck();

        huPending = false;

        var spawner = GameObject.FindObjectOfType<Spawner>();
        if (spawner != null)
        {
            spawner.SpawnBlock();
            Debug.Log("[TetrisGrid] Spawned new tetromino after hu finalize");
        }
        else Debug.LogWarning("[TetrisGrid] 找不到 Spawner，无法生成下一个方块");
    }

    public static int DeleteFullRows()
    {
        int rowsCleared = 0;

        var fullRows = new List<int>();
        for (int y = 0; y < height; y++) if (IsRowFull(y)) fullRows.Add(y);
        if (fullRows.Count == 0) return 0;

        var rowsBlockIds = new Dictionary<int, List<int>>();
        var allSets = new List<List<int>>();

        foreach (var y in fullRows)
        {
            var rowIds = GetBlockIdsInRow(y);
            rowsBlockIds[y] = new List<int>(rowIds);
            var sets = DetectSetsInRow(y);
            foreach (var s in sets) allSets.Add(s);
        }

        Debug.Log($"[TetrisGrid] Found fullRows={string.Join(",", fullRows)} totalSetsFound={allSets.Count}");

        var hu = HuPaiArea.Instance;
        int currentHu = hu != null ? hu.GetHuPaiSetCount() : 0;
        int need = Mathf.Max(0, 4 - currentHu);

        var takenSets = new List<List<int>>();
        var rejectedSets = new List<List<int>>();

        var indices = new List<int>();
        for (int i = 0; i < allSets.Count; i++) indices.Add(i);
        for (int i = 0; i < indices.Count; i++)
        {
            int j = Random.Range(i, indices.Count);
            int tmp = indices[i]; indices[i] = indices[j]; indices[j] = tmp;
        }

        for (int k = 0; k < indices.Count; k++)
        {
            var s = allSets[indices[k]];
            if (takenSets.Count < need) takenSets.Add(s); else rejectedSets.Add(s);
        }

        Debug.Log($"[TetrisGrid] takenSets={takenSets.Count} rejectedSets={rejectedSets.Count}");

        var movedIds = new HashSet<int>();
        foreach (var s in takenSets) foreach (var id in s) movedIds.Add(id);

        var toRecycleIds = new HashSet<int>();
        foreach (var kv in rowsBlockIds)
        {
            foreach (var id in kv.Value)
            {
                if (!movedIds.Contains(id)) toRecycleIds.Add(id);
            }
        }
        foreach (var s in rejectedSets) foreach (var id in s) toRecycleIds.Add(id);

        if (hu != null)
        {
            foreach (var set in takenSets)
            {
                var transforms = new List<Transform>();
                foreach (var id in set)
                {
                    var t = FindTransformByBlockId(id);
                    if (t != null) transforms.Add(t);
                }

                if (transforms.Count == 3)
                {
                    foreach (var tr in transforms)
                    {
                        bool cleared = false;
                        for (int yy = 0; yy < height && !cleared; yy++)
                        {
                            for (int xx = 0; xx < width; xx++)
                            {
                                if (grid[xx, yy] == tr)
                                {
                                    grid[xx, yy] = null;
                                    cleared = true;
                                    break;
                                }
                            }
                        }
                    }

                    hu.AddHuPaiSetFromBoardTransforms(transforms);
                }
                else
                {
                    foreach (var id in set) toRecycleIds.Add(id);
                }
            }
        }

        foreach (var id in new List<int>(toRecycleIds))
        {
            var t = FindTransformByBlockId(id);
            if (t != null)
            {
                var bu = t.GetComponent<BlockUnit>();
                if (bu != null)
                {
                    bu.ReturnToPoolAndDestroy();
                    Debug.Log($"[TetrisGrid] Recycled id={id}");
                }

                for (int yy = 0; yy < height; yy++)
                    for (int xx = 0; xx < width; xx++)
                        if (grid[xx, yy] == t) grid[xx, yy] = null;
            }
            else
            {
                Debug.LogWarning($"[TetrisGrid] 回收未找到 transform for id={id}");
            }
        }

        CompactColumns();

        rowsCleared = fullRows.Count;

        if (hu != null)
        {
            int totalSets = hu.GetHuPaiSetCount();
            Debug.Log($"[TetrisGrid] 胡牌区组数 after processing = {totalSets}");

            if (totalSets >= 4)
            {
                var tileCounts = new Dictionary<int, int>();
                foreach (var kv in rowsBlockIds)
                {
                    foreach (var bid in kv.Value)
                    {
                        int tile = bid % 27;
                        tileCounts[tile] = tileCounts.ContainsKey(tile) ? tileCounts[tile] + 1 : 1;
                    }
                }
                bool hasPair = false;
                foreach (var kv in tileCounts) if (kv.Value >= 2) { hasPair = true; break; }

                Debug.Log($"[TetrisGrid] 本次消行是否包含对子: {hasPair}");
                if (hasPair)
                {
                    // 先加分（即时）
                    if (ScoreManager.Instance != null)
                    {
                        ScoreManager.Instance.AddScore(100);
                        Debug.Log("[TetrisGrid] 已加 10000 分（即时反馈）");
                    }

                    // 将要显示的 sets：优先显示本次加入的 takenSets（如果为空则显示 HuPaiArea 全部）
                    var setsToShow = takenSets.Count > 0 ? takenSets : hu.GetAllSets();

                    if (HuPopup.Instance != null)
                    {
                        HuPopup.Instance.ShowHu(setsToShow);
                    }
                    else
                    {
                        Debug.LogWarning("[TetrisGrid] HuPopup.Instance 为 null，直接 finalize");
                        PerformHuFinalize();
                    }
                }
            }
        }

        return rowsCleared;
    }
}
