using UnityEngine;

[CreateAssetMenu(fileName = "Passport_Wan", menuName = "Items/Advanced/Passport Wan")]
public class PassportWanItem : ItemData
{
    public override bool Use(GameManager gameManager)
    {
        // 2 代表万子 (Characters)
        gameManager.ActivatePassport(2);
        return true;
    }
}