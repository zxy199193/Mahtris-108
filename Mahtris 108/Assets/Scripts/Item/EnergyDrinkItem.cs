using UnityEngine;
[CreateAssetMenu(fileName = "EnergyDrink", menuName = "Items/Common/EnergyDrink")]
public class EnergyDrinkItem : ItemData
{
    public int scoreBonus = 8;
    public override bool Use(GameManager gameManager)
    {
        gameManager.ApplyRoundBaseScoreBonus(scoreBonus);
        return true;
    }
}