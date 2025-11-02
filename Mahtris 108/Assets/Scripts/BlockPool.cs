// FileName: BlockPool.cs
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BlockPool : MonoBehaviour
{
    [SerializeField] private Sprite[] mahjongSprites;

    private List<int> availableBlocks = new List<int>();
    private int totalBlocks;

    public void Initialize(GameSettings settings)
    {
        this.totalBlocks = settings.TotalTileCount;
        if (mahjongSprites != null && mahjongSprites.Length > 0)
            System.Array.Sort(mahjongSprites, (a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));
    }

    public void ResetFullDeck()
    {
        availableBlocks.Clear();
        for (int i = 0; i < totalBlocks; i++) availableBlocks.Add(i);

        var rng = new System.Random();
        availableBlocks = availableBlocks.OrderBy(a => rng.Next()).ToList();
        // 【新增】应用条约过滤器
        if (GameManager.Instance != null)
        {
            // “断幺九”：移除1和9
            if (GameManager.Instance.useDuanYaoJiuFilter)
            {
                availableBlocks.RemoveAll(id =>
                    (id % 27) % 9 == 0 || // 1 (0, 9, 18)
                    (id % 27) % 9 == 8  // 9 (8, 17, 26)
                );
            }

            // “缺一门”：移除指定花色
            if (GameManager.Instance.useQueYiMenFilter && GameManager.Instance.queYiMenSuitToRemove >= 0)
            {
                int suitToRemove = GameManager.Instance.queYiMenSuitToRemove; // 0, 1, or 2
                availableBlocks.RemoveAll(id => (id % 27) / 9 == suitToRemove);
            }
        }
        // --- 过滤器结束 ---

        GameEvents.TriggerPoolCountChanged(availableBlocks.Count);
    }

    public List<int> GetBlockIds(int count)
    {
        if (count > availableBlocks.Count)
        {
            GameEvents.TriggerGameOver(); // 牌库抽干，游戏结束
            return null;
        }

        List<int> ids = availableBlocks.Take(count).ToList();
        availableBlocks.RemoveRange(0, count);

        GameEvents.TriggerPoolCountChanged(availableBlocks.Count);
        return ids;
    }

    public void ReturnBlockIds(List<int> ids)
    {
        if (ids != null)
        {
            availableBlocks.AddRange(ids);
            GameEvents.TriggerPoolCountChanged(availableBlocks.Count);
        }
    }

    public Sprite GetSpriteForBlock(int blockId)
    {
        if (mahjongSprites == null || mahjongSprites.Length == 0) return null;
        // 使用 blockId % 27 来对应 27 种不同的牌面
        return mahjongSprites[blockId % 27];
    }
    // 【新增】供“圣诞礼物”道具扫描牌库
    public List<int> GetAvailableBlockIDs()
    {
        return new List<int>(availableBlocks);
    }
    // 【新增】供“圣诞礼物”道具从牌库中移除选定的牌
    public bool RemoveSpecificBlockIds(List<int> idsToRemove)
    {
        if (idsToRemove == null) return false;
        // 检查牌库中是否真的有这些牌
        var tempAvailable = new List<int>(availableBlocks);
        foreach (int id in idsToRemove)
        {
            if (!tempAvailable.Remove(id))
            {
                Debug.LogError($"[BlockPool] 圣诞礼物: 试图移除不存在的牌 ID: {id}");
                return false; // 牌库中没有这张牌
            }
        }

        // 确认无误后，正式从牌库移除
        foreach (int id in idsToRemove)
        {
            availableBlocks.Remove(id);
        }

        GameEvents.TriggerPoolCountChanged(availableBlocks.Count);
        return true;
    }
    }