// FileName: CouponItem.cs
using UnityEngine;

[CreateAssetMenu(fileName = "Coupon", menuName = "Items/Advanced/Coupon")]
public class CouponItem : ItemData
{
    [Range(0.1f, 0.9f)]
    public float reductionPercentage = 0.5f; // ºı…Ÿ50%

    public override bool Use(GameManager gameManager)
    {
        gameManager.ModifyTargetScore(1f - reductionPercentage);
        return true;
    }
}