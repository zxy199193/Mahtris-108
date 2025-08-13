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

        GameEvents.TriggerPoolCountChanged(availableBlocks.Count);
    }

    public List<int> GetBlockIds(int count)
    {
        if (count > availableBlocks.Count) return null;

        List<int> ids = availableBlocks.Take(count).ToList();
        availableBlocks.RemoveRange(0, count);

        GameEvents.TriggerPoolCountChanged(availableBlocks.Count);
        return ids;
    }

    public void ReturnBlockIds(List<int> ids)
    {
        availableBlocks.AddRange(ids);
        GameEvents.TriggerPoolCountChanged(availableBlocks.Count);
    }

    public Sprite GetSpriteForBlock(int blockId)
    {
        if (mahjongSprites == null || mahjongSprites.Length == 0) return null;
        return mahjongSprites[blockId % mahjongSprites.Length];
    }
}
