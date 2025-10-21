using UnityEngine;

[CreateAssetMenu(fileName = "DivineSalvation", menuName = "Items/Advanced/DivineSalvation")]
public class DivineSalvationItem : ItemData
{
    public int baseScoreBonus = 16;
    public float speedReduction = -60f; // ¼õËÙ60%
    public float timeBonus = 60f;
    public int rowsToClear = 2;

    public override bool Use(GameManager gameManager)
    {
        gameManager.ModifyBaseFanScore(baseScoreBonus, false); // false = ¼Ó·¨
        gameManager.ModifySpeedOfCurrentTetrominoByPercent(speedReduction);
        gameManager.AddTime(timeBonus);
        gameManager.ForceClearRowsFromBottom(rowsToClear);
        return true;
    }
}