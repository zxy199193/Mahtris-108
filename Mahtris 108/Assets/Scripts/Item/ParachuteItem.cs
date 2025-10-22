using UnityEngine;
[CreateAssetMenu(fileName = "Parachute", menuName = "Items/Common/Parachute")]
public class ParachuteItem : ItemData
{
    [Tooltip("������Ϸ�ٶȽ��� 8")]
    public int speedBonus = -8;
    public override bool Use(GameManager gameManager)
    {
        gameManager.ApplyRoundSpeedBonus(speedBonus);
        return true;
    }
}