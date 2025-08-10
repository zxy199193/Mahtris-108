// BlockPool.cs
using System;
using System.Collections.Generic;
using UnityEngine;

public class BlockPool : MonoBehaviour
{
    public static BlockPool Instance;

    [Tooltip("总小方块数量（默认108）")]
    public int totalBlocks = 108;

    // 可用的 blockId 列表（0..totalBlocks-1）
    private List<int> availableBlocks = new List<int>();

    // 27 种麻将图片（应放在 Resources/MahjongTiles 下）
    private Sprite[] mahjongSprites;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        // 加载麻将图片（放在 Assets/Resources/MahjongTiles）
        mahjongSprites = Resources.LoadAll<Sprite>("MahjongTiles");
        if (mahjongSprites == null || mahjongSprites.Length == 0)
        {
            Debug.LogError("BlockPool: 没有加载到 MahjongTiles 下的 Sprite。请把 27 张图片放到 Assets/Resources/MahjongTiles/ 中。");
        }
        else
        {
            // 为了确保映射顺序稳定，按文件名排序（Name 应命名有序，例如 Tong1, Tong2 ... Wan1 ... Tiao1 ...）
            Array.Sort(mahjongSprites, (a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));
            Debug.Log($"BlockPool: 加载到 {mahjongSprites.Length} 张麻将图片（已按名字排序）。");
        }

        // 初始化 ID 池
        availableBlocks.Clear();
        for (int i = 0; i < totalBlocks; i++)
            availableBlocks.Add(i);

        Debug.Log($"BlockPool: 初始化完成，可用小方块数 = {availableBlocks.Count}");
    }

    // 单个取出（随机或FIFO都可以，这里用随机）
    public int GetBlockId()
    {
        if (availableBlocks.Count == 0) return -1;
        int idx = UnityEngine.Random.Range(0, availableBlocks.Count);
        int id = availableBlocks[idx];
        availableBlocks.RemoveAt(idx);
        return id;
    }

    // 单个回收
    public void ReturnBlock(int blockId)
    {
        if (blockId < 0 || blockId >= totalBlocks)
        {
            Debug.LogWarning($"BlockPool.ReturnBlock: 无效 blockId {blockId}");
            return;
        }
        availableBlocks.Add(blockId);
    }

    // 批量获取（返回 null 表示不足）
    public List<int> GetBlockIds(int count)
    {
        List<int> ids = new List<int>();
        if (count <= 0) return ids;

        for (int i = 0; i < count; i++)
        {
            int id = GetBlockId();
            if (id == -1)
            {
                // 不够，回滚已经取出的 id 并返回 null
                foreach (var r in ids) ReturnBlock(r);
                return null;
            }
            ids.Add(id);
        }
        return ids;
    }

    // 批量回收
    public void ReturnBlockIds(List<int> ids)
    {
        if (ids == null) return;
        foreach (var id in ids)
            ReturnBlock(id);
    }

    // 剩余数量（外部调用）
    public int GetRemainingBlocks()
    {
        return availableBlocks.Count;
    }

    // 根据 blockId 返回对应的麻将 Sprite（27 张循环）
    public Sprite GetSpriteForBlock(int blockId)
    {
        if (mahjongSprites == null || mahjongSprites.Length == 0)
        {
            Debug.LogWarning("BlockPool.GetSpriteForBlock: mahjongSprites 未加载或为空。");
            return null;
        }

        if (blockId < 0 || blockId >= totalBlocks)
        {
            Debug.LogWarning($"BlockPool.GetSpriteForBlock: 无效的 blockId {blockId}");
            return null;
        }

        int tileType = blockId % mahjongSprites.Length; // 0..26
        return mahjongSprites[tileType];
    }
}
