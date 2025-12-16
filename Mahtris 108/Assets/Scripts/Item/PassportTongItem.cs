using UnityEngine;

[CreateAssetMenu(fileName = "Passport_Tong", menuName = "Items/Advanced/Passport Tong")]
public class PassportTongItem : ItemData
{
    public override bool Use(GameManager gameManager)
    {
        // 0 ´ú±íÍ²×Ó (Dots)
        gameManager.ActivatePassport(1);
        return true;
    }
}