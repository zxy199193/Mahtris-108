// FileName: AmplifierProItem.cs
using UnityEngine;

[CreateAssetMenu(fileName = "AmplifierPro", menuName = "Items/Advanced/AmplifierPro")]
public class AmplifierProItem : ItemData
{
    public int multiplier = 2;

    public override bool Use(GameManager gameManager)
    {
        gameManager.ModifyBaseFanScore(multiplier, true); // true��ʾ���Ա���
        return true;
    }
}