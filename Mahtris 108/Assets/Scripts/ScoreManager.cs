// FileName: ScoreManager.cs
using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static event Action<int> OnScoreChanged;

    private int score;
    private int huCount;
    private int huCountInCycle;
    private int _highScore; // 新增：用于缓存最高分

    void Start()
    {
        // 游戏开始时加载一次最高分
        _highScore = SaveManager.LoadHighScore();
    }

    public int GetHuCount()
    {
        return huCount;
    }

    public int GetHuCountInCycle()
    {
        return huCountInCycle;
    }
    // 【新增】供“快进按钮”道具调用
    // 【修复2】快进按钮：同时推进循环进度
    public void AddHuCount(int amount)
    {
        huCount += amount;
        // 推进循环进度，保持在 0-3 之间 (4次一循环)
        huCountInCycle = (huCountInCycle + amount) % 4;
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

    // 旧的 AddScore 方法：
    // public void AddScore(int amount) { ... if (score > _highScore) ... }

    // 新的 AddScore 方法：
    public void AddScore(int amount)
    {
        score += amount;
        ScoreManager.OnScoreChanged?.Invoke(score);
    }

    // 新增方法
    public bool CheckForNewHighScore(int finalScore)
    {
        if (finalScore > _highScore)
        {
            _highScore = finalScore;
            SaveManager.SaveHighScore(_highScore);
            return true;
        }
        return false;
    }

    public void ResetScore()
    {
        score = 0;
        huCount = 0;
        huCountInCycle = 0;

        // 重新加载最高分，以备在同一会话中开始新游戏
        _highScore = SaveManager.LoadHighScore();

        OnScoreChanged?.Invoke(score);
    }
    public int GetCurrentScore()
    {
        return score;
    }
}