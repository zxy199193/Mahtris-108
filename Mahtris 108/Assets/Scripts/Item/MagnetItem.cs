using UnityEngine;

[CreateAssetMenu(fileName = "Magnet", menuName = "Items/Common/Magnet")]
public class MagnetItem : ItemData
{
    public override bool Use(GameManager gameManager)
    {
        return gameManager.ActivateMagnetV2();
    }
}
