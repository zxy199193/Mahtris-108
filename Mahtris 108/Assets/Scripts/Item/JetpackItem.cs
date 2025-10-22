using UnityEngine;
[CreateAssetMenu(fileName = "Jetpack", menuName = "Items/Common/Jetpack")]
public class JetpackItem : ItemData
{
    public int speedBonus = -15;
    public int blockCount = 10;
    public override bool Use(GameManager gameManager)
    {
        gameManager.ApplyCountedSpeedBonus(speedBonus, blockCount);
        return true;
    }
}