// FileName: VoucherItem.cs
using UnityEngine;

[CreateAssetMenu(fileName = "Voucher", menuName = "Items/Common/Voucher")]
public class VoucherItem : ItemData
{
    public int goldAmount = 50;

    public override bool Use(GameManager gameManager)
    {
        if (GameSession.Instance != null)
        {
            GameSession.Instance.AddGold(goldAmount);
            return true;
        }
        return false;
    }
}