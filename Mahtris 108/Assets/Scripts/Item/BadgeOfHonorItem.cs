// FileName: BadgeOfHonorItem.cs
using UnityEngine;

[CreateAssetMenu(fileName = "BadgeOfHonor", menuName = "Items/Advanced/BadgeOfHonor")]
public class BadgeOfHonorItem : ItemData
{
    [Header("勋章配置")]
    [Tooltip("选中的方块倍率增加多少")]
    public float bonusMultiplier = 5f;

    [Tooltip("随机选择几种方块进行强化")]
    public int targetCount = 2;

    public override bool Use(GameManager gameManager)
    {
        // 调用 GM 的方法激活效果
        return gameManager.ActivateBadgeOfHonor(targetCount, bonusMultiplier);
    }
}