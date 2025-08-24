// FileName: ScoreManager.cs
using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    // --- 【修正点】---
    // 确保这个公开的静态事件存在，以便其他脚本可以订阅
    public static event Action<int> OnScoreChanged;

    private int score;

    public void AddScore(int amount)
    {
        score += amount;
        GameEvents.TriggerScoreChanged(score); // 触发全局事件，用于UI等
        OnScoreChanged?.Invoke(score);         // 触发自身静态事件，用于GameManager等核心逻辑
    }

    public void ResetScore()
    {
        score = 0;
        GameEvents.TriggerScoreChanged(score);
        OnScoreChanged?.Invoke(score);
    }
}