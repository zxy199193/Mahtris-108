// FileName: DivineSalvationItem.cs
using UnityEngine;

[CreateAssetMenu(fileName = "DivineSalvation", menuName = "Items/Advanced/DivineSalvation")]
public class DivineSalvationItem : ItemData
{
    [Header("�°�Ч�� (V4.1)")]
    [Tooltip("������Ϸ���������� 18")]
    public int baseScoreBonus = 18;

    [Tooltip("������Ϸ�ٶȵȼ����� 8")]
    public int speedBonus = -8;

    [Tooltip("���� 80 ����Ϸʱ��")]
    public float timeBonus = 80f;

    [Tooltip("ǿ��������ײ��� 3 ��")]
    public int rowsToClear = 3;

    public override bool Use(GameManager gameManager)
    {
        // 1. �����µġ����û����֡�ϵͳ
        gameManager.ApplyPermanentBaseScoreBonus(baseScoreBonus);

        // 2. �����µġ������ٶȡ�ϵͳ
        gameManager.ApplyPermanentSpeedBonus(speedBonus);

        // 3. ���á�����ʱ�䡱
        gameManager.AddTime(timeBonus);

        // 4. ���á�ǿ�����С�
        gameManager.ForceClearRowsFromBottom(rowsToClear);

        return true;
    }
}