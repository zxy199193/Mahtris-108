using UnityEngine;
[CreateAssetMenu(fileName = "Steroids", menuName = "Items/Common/Steroids")]
public class SteroidsItem : ItemData
{
    public int scoreBonus = 24;
    public override bool Use(GameManager gameManager)
    {
        gameManager.ApplySteroidBaseScoreBonus(scoreBonus);
        return true;
    }
}