// FileName: AmplifierItem.cs
using UnityEngine;

[CreateAssetMenu(fileName = "Amplifier", menuName = "Items/Common/Amplifier")]
public class AmplifierItem : ItemData
{
    public int scoreBonus = 5;

    public override bool Use(GameManager gameManager)
    {
        gameManager.AddBaseScoreBonus(scoreBonus);
        return true;
    }
}