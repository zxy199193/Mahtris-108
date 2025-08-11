using System;
using System.Collections.Generic;
using UnityEngine;

public class BlockPool : MonoBehaviour
{
    public static BlockPool Instance { get; private set; }

    [Tooltip("总小方块数量（默认108）")]
    public int totalBlocks = 108;

    // 可用的 blockId 列表
    private List<int> availableBlocks = new List<int>();

    // 麻将图片（放在 Resources/MahjongTiles）
    private Sprite[] mahjongSprites;

    // 牌库数量变化事件
    public event Action<int> OnPoolCountChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        // 尝试加载 Sprites（可选）
        mahjongSprites = Resources.LoadAll<Sprite>("MahjongTiles");
        if (mahjongSprites != null && mahjongSprites.Length > 0)
            Array.Sort(mahjongSprites, (a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));
        else
            Debug.LogWarning("BlockPool: 未加载到 MahjongTiles Sprite（Resources/MahjongTiles）");

        ResetFullDeck();
    }

    public void ResetFullDeck()
    {
        availableBlocks.Clear();
        for (int i = 0; i < totalBlocks; i++) availableBlocks.Add(i);
        Shuffle(availableBlocks);

        Debug.Log($"[BlockPool] ResetFullDeck -> {availableBlocks.Count}");
        OnPoolCountChanged?.Invoke(availableBlocks.Count);
    }

    private void Shuffle(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = UnityEngine.Random.Range(i, list.Count);
            int tmp = list[i]; list[i] = list[j]; list[j] = tmp;
        }
    }

    public int GetBlockId()
    {
        if (availableBlocks.Count == 0) return -1;
        int idx = UnityEngine.Random.Range(0, availableBlocks.Count);
        int id = availableBlocks[idx];
        availableBlocks.RemoveAt(idx);
        OnPoolCountChanged?.Invoke(availableBlocks.Count);
        Debug.Log($"[BlockPool] GetBlockId -> {id}  left={availableBlocks.Count}");
        return id;
    }

    // 批量取牌（返回 null 表示不足）
    public List<int> GetBlockIds(int count)
    {
        if (count <= 0) return new List<int>();
        if (count > availableBlocks.Count) return null;

        List<int> ids = new List<int>();
        for (int i = 0; i < count; i++)
        {
            int id = GetBlockId();
            if (id == -1)
            {
                // 回滚（理论上不会走到）
                foreach (var r in ids) ReturnBlock(r);
                return null;
            }
            ids.Add(id);
        }
        return ids;
    }

    public void ReturnBlock(int id)
    {
        if (id < 0 || id >= totalBlocks) { Debug.LogWarning($"[BlockPool] ReturnBlock invalid {id}"); return; }
        if (availableBlocks.Contains(id))
        {
            Debug.LogWarning($"[BlockPool] ReturnBlock duplicate {id}");
            return;
        }
        availableBlocks.Add(id);
        OnPoolCountChanged?.Invoke(availableBlocks.Count);
        Debug.Log($"[BlockPool] ReturnBlock {id} -> left={availableBlocks.Count}");
    }

    public void ReturnBlockIds(List<int> ids)
    {
        if (ids == null || ids.Count == 0) return;
        foreach (var id in ids) ReturnBlock(id);
    }

    public int GetRemainingBlocks() => availableBlocks.Count;

    // 根据 blockId 映射到 Sprite（用 mahjongSprites）
    public Sprite GetSpriteForBlock(int blockId)
    {
        if (mahjongSprites == null || mahjongSprites.Length == 0) return null;
        if (blockId < 0) return null;
        int idx = blockId % mahjongSprites.Length;
        return mahjongSprites[idx];
    }
}
