// FileName: VoucherItem.cs
using UnityEngine;

[CreateAssetMenu(fileName = "Voucher", menuName = "Items/Common/Voucher")]
public class VoucherItem : ItemData
{
    public int goldAmount = 800;

    public override bool Use(GameManager gameManager)
    {
        gameManager.AddExtraGoldReward(goldAmount);
        return true;
    }
}