using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    private int score;

    public void AddScore(int amount)
    {
        score += amount;
        GameEvents.TriggerScoreChanged(score);
    }

    public void ResetScore()
    {
        score = 0;
        GameEvents.TriggerScoreChanged(score);
    }
}
