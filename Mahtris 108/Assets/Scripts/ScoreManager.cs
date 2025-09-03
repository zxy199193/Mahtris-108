// FileName: ScoreManager.cs
using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    // 这个脚本内部的事件不再需要，我们统一使用全局事件
    // public static event Action<int> OnScoreChanged;
    public static event System.Action<int> OnScoreChanged;
    private int score;
    private int huCount;
    private int huCountInCycle;

    public int GetHuCount()
    {
        return huCount;
    }

    public int GetHuCountInCycle()
    {
        return huCountInCycle;
    }

    public bool IncrementHuCountAndCheckCycle()
    {
        huCount++;
        huCountInCycle++;
        if (huCountInCycle >= 4)
        {
            huCountInCycle = 0;
            return true;
        }
        return false;
    }

    public void AddScore(int amount)
    {
        score += amount;
        OnScoreChanged?.Invoke(score); // 触发事件，通知GameManager
        // ---【重大修正点】---
        // 调用全局的 GameEvents 来触发分数变化事件
        GameEvents.TriggerScoreChanged(score);
    }

    public void ResetScore()
    {
        score = 0;
        huCount = 0;
        huCountInCycle = 0;
        OnScoreChanged?.Invoke(score);
        // ---【重大修正点】---
        // 调用全局的 GameEvents 来触发分数变化事件
        GameEvents.TriggerScoreChanged(score);
    }
}