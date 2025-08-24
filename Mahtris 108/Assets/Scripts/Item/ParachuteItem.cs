using UnityEngine;
[CreateAssetMenu(fileName = "Parachute", menuName = "Items/Parachute")]
public class ParachuteItem : ItemData
{
    [Range(0.1f, 0.9f)]
    public float speedReductionPercent = 0.3f;
    public override bool Use(GameManager gameManager)
    {
        gameManager.ModifyFallSpeed(1f + speedReductionPercent);
        return true;
    }
}