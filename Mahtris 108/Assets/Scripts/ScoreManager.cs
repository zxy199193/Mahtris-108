// FileName: ScoreManager.cs
using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static event Action<int> OnScoreChanged;

    private int score;
    private int huCount; // 【新增】用于记录胡牌次数的变量

    // 【新增】供外部获取胡牌次数的方法
    public int GetHuCount()
    {
        return huCount;
    }

    public void AddScore(int amount)
    {
        score += amount;
        GameEvents.TriggerScoreChanged(score);
        OnScoreChanged?.Invoke(score);
    }

    public void IncrementHuCount()
    {
        huCount++;
    }

    public void ResetScore()
    {
        score = 0;
        huCount = 0; // 重置游戏时，胡牌次数也一并重置
        GameEvents.TriggerScoreChanged(score);
        OnScoreChanged?.Invoke(score);
    }
}