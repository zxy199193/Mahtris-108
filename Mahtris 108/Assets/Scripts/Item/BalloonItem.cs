using UnityEngine;
[CreateAssetMenu(fileName = "Balloon", menuName = "Items/Common/Balloon")]
public class BalloonItem : ItemData
{
    public int speedBonus = -3;
    public override bool Use(GameManager gameManager)
    {
        gameManager.ApplyPermanentSpeedBonus(speedBonus);
        return true;
    }
}