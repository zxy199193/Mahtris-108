// FileName: ChristmasGiftItem.cs
using UnityEngine;
[CreateAssetMenu(fileName = "ChristmasGift", menuName = "Items/Advanced/ChristmasGift")]
public class ChristmasGiftItem : ItemData
{
    public override bool Use(GameManager gameManager)
    {
        // 调用GameManager中的新逻辑，此方法将返回true/false
        return gameManager.TryFindAndAddRandomSetFromPool();
    }
}