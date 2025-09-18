// FileName: ParachuteItem.cs
using UnityEngine;

[CreateAssetMenu(fileName = "Parachute", menuName = "Items/Common/Parachute")]
public class ParachuteItem : ItemData
{
    [Tooltip("�ٶȽ��͵İٷֱ���ֵ����������30������30%")]
    public float speedReductionPercent = 30f;

    public override bool Use(GameManager gameManager)
    {
        gameManager.ApplySpeedToCurrentTetromino(-speedReductionPercent);
        return true;
    }
}