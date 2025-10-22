using UnityEngine;
[CreateAssetMenu(fileName = "Juice", menuName = "Items/Common/Juice")]
public class JuiceItem : ItemData
{
    public int scoreBonus = 3;
    public override bool Use(GameManager gameManager)
    {
        gameManager.ApplyPermanentBaseScoreBonus(scoreBonus);
        return true;
    }
}