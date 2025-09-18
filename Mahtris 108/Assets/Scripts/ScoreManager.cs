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

        // 【新增】检查并保存最高分
        if (score > _highScore)
        {
            _highScore = score;
            SaveManager.SaveHighScore(_highScore);
        }

        OnScoreChanged?.Invoke(score);
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
}