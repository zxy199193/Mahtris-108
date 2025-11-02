using UnityEngine;
[CreateAssetMenu(fileName = "Scoreboard", menuName = "Items/Advanced/Scoreboard")]
public class ScoreboardItem : ItemData
{
    [Tooltip("效果持续时间（秒）")]
    public float duration = 20f;
    public override bool Use(GameManager gameManager)
    {
        gameManager.ActivateScoreboard(duration);
        return true;
    }
}