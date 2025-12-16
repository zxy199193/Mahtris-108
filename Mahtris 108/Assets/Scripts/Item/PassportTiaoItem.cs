using UnityEngine;

[CreateAssetMenu(fileName = "Passport_Tiao", menuName = "Items/Advanced/Passport Tiao")]
public class PassportTiaoItem : ItemData
{
    public override bool Use(GameManager gameManager)
    {
        // 1 代表条子 (Bamboo)
        gameManager.ActivatePassport(0);
        return true;
    }
}