// FileName: ParachuteItem.cs
using UnityEngine;

[CreateAssetMenu(fileName = "Parachute", menuName = "Items/Parachute")]
public class ParachuteItem : ItemData
{
    [Tooltip("�ٶȽ��͵İٷֱ���ֵ����������30�����͵�ǰ�ٶȵ�30%")]
    public float speedReductionPercent = 30f;

    public override bool Use(GameManager gameManager)
    {
        Debug.Log($"ʹ���ˡ�{itemName}�����ٶȽ����� {speedReductionPercent}%��");
        // ---���ش������㡿---
        // ���� GameManager ����ȷ���·����� ModifySpeedByPercentage
        gameManager.ModifySpeedByPercentage(-speedReductionPercent); // ���븺ֵ��ʾ����
        return true;
    }
}