// FileName: ScoreManager.cs
using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static event Action<int> OnScoreChanged;

    private int score;
    private int huCount;
    private int _highScore;

    // 循环系统变量
    private int currentLoop = 1;
    private int huCountInCurrentLoop = 0; // 当前圈已完成的胡牌数

    void Start()
    {
        _highScore = SaveManager.LoadHighScore();
    }

    // --- 核心修复区域 ---

    // 【修复】重置逻辑
    public void ResetScore()
    {
        score = 0;
        huCount = 0;
        currentLoop = 1;
        huCountInCurrentLoop = 0; // 重置为0，UI显示会+1变成 "1/4"

        if (OnScoreChanged != null) OnScoreChanged(score);
    }

    // 【修复】增加胡牌计数并检查循环
    // 返回 true = 触发高级奖励 (本圈结束)
    // 返回 false = 普通奖励 (本圈继续)
    public bool IncrementHuCountAndCheckCycle()
    {
        huCount++;
        huCountInCurrentLoop++; // 完成了一次胡牌

        int target = 4; // 默认值
        if (GameManager.Instance != null && GameManager.Instance.GetSettings() != null)
        {
            target = GameManager.Instance.GetSettings().husPerLoop;
        }

        // 检查是否达到目标
        if (huCountInCurrentLoop >= target)
        {
            // 完成了一圈
            currentLoop++;
            huCountInCurrentLoop = 0; // 归零，准备开始下一圈的第1次
            return true; // 发放高级奖励
        }

        return false; // 发放普通奖励
    }

    // 【修复】UI显示逻辑
    // 逻辑：huCountInCurrentLoop 是“已完成”的数量。
    // 玩家正在进行的是第 (已完成 + 1) 轮。
    // 比如刚开始：已完成0，显示 "1/4" (正在打第1把)
    // 胡了3次：已完成3，显示 "4/4" (正在打第4把，这把胡了就发奖)
    public string GetLoopProgressString()
    {
        int target = 4;
        if (GameManager.Instance != null && GameManager.Instance.GetSettings() != null)
        {
            target = GameManager.Instance.GetSettings().husPerLoop;
        }

        // 显示正在进行的轮数 (当前完成数 + 1)
        return $"第{currentLoop}圈 {huCountInCurrentLoop + 1}/{target}";
    }

    // 【修复】快进按钮逻辑
    public void AddHuCount(int amount)
    {
        huCount += amount;
        huCountInCurrentLoop += amount;

        int target = GameManager.Instance.GetSettings().husPerLoop;

        // 处理跨圈
        while (huCountInCurrentLoop >= target)
        {
            currentLoop++;
            huCountInCurrentLoop -= target;
        }
    }

    // --- 基础 Getter/Setter ---

    public int GetHuCount() => huCount;
    public int GetHuCountInCycle() => huCountInCurrentLoop;
    public int GetCurrentScore() => score;
    public int GetCurrentLoop() => currentLoop;

    public void AddScore(int amount)
    {
        score += amount;
        OnScoreChanged?.Invoke(score);
    }

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
}
