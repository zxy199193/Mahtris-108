// FileName: ParachuteItem.cs
using UnityEngine;

[CreateAssetMenu(fileName = "Parachute", menuName = "Items/Common/Parachute")]
public class ParachuteItem : ItemData
{
    [Tooltip("速度降低的百分比数值，例如输入30代表降低30%")]
    public float speedReductionPercent = 30f;

    public override bool Use(GameManager gameManager)
    {
        gameManager.ApplySpeedToCurrentTetromino(-speedReductionPercent);
        return true;
    }
}