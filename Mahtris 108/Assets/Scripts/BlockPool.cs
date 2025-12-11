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

    public void ResetFullDeck(List<int> excludedIds = null)
    {
        availableBlocks.Clear();
        int totalTiles = GameManager.Instance.GetSettings().TotalTileCount;

        // 1. 生成完整的 0~107 ID 列表
        var fullDeck = new List<int>();
        for (int i = 0; i < totalTiles; i++)
        {
            fullDeck.Add(i);
        }

        // 2. 如果有需要排除的牌 (复活石逻辑：胡牌区的牌不应回到牌库)
        if (excludedIds != null)
        {
            foreach (int id in excludedIds)
            {
                fullDeck.Remove(id);
            }
        }

        availableBlocks = fullDeck;

        // 3. 应用条约过滤器 (断幺九、缺一门)
        // 这些过滤器只针对"剩余牌库"生效，不会影响已经胡掉的牌
        if (GameManager.Instance != null)
        {
            // 断幺九：移除所有 1 和 9
            if (GameManager.Instance.useDuanYaoJiuFilter)
            {
                availableBlocks.RemoveAll(id =>
                    (id % 27) % 9 == 0 || // 1万/1条/1筒
                    (id % 27) % 9 == 8    // 9万/9条/9筒
                );
            }

            // 缺一门：移除指定花色
            if (GameManager.Instance.useQueYiMenFilter && GameManager.Instance.queYiMenSuitToRemove >= 0)
            {
                int suitToRemove = GameManager.Instance.queYiMenSuitToRemove;
                availableBlocks.RemoveAll(id => (id % 27) / 9 == suitToRemove);
            }
        }

        // 4. 洗牌
        var rng = new System.Random();
        availableBlocks = availableBlocks.OrderBy(a => rng.Next()).ToList();

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
    // 【新增】偷看牌库 (不消耗)。如果数量不足，不足的部分用 -1 填充
    // 用于 Spawner 生成带黑块的预览
    public List<int> PeekBlockIDs(int count)
    {
        List<int> result = new List<int>();

        // 1. 先把有的都装进去 (模拟取牌)
        for (int i = 0; i < availableBlocks.Count && i < count; i++)
        {
            result.Add(availableBlocks[i]);
        }

        // 2. 不够的用 -1 补齐 (黑块标记)
        while (result.Count < count)
        {
            result.Add(-1);
        }

        return result;
    }

    // 【新增】硬性检查：是否真的足够
    // 用于 Spawner 在生成实体方块前做最后一次生死判定
    public bool HasEnoughBlocks(int count)
    {
        return availableBlocks.Count >= count;
    }
}