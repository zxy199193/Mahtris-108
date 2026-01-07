using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    // 【核心修改】事件参数改为 long
    public static event Action<long> OnScoreChanged;

    // 【核心修改】分数变量改为 long
    private long score;
    private long _highScore;

    private int huCount;

    // 循环系统变量
    private int currentLoop = 1;
    private int huCountInCurrentLoop = 0;

    void Start()
    {
        // 【核心修改】兼容性读取逻辑
        // 1. 先尝试读取新的 long 类型存档 (字符串格式)
        string savedLong = PlayerPrefs.GetString("HighScore_Long", "");

        if (!string.IsNullOrEmpty(savedLong) && long.TryParse(savedLong, out long result))
        {
            _highScore = result;
        }
        else
        {
            // 2. 如果没有，则读取旧的 int 类型存档 (兼容老玩家)
            // 注意：这里暂时不调用 SaveManager，直接用 PlayerPrefs 以确保逻辑闭环
            _highScore = PlayerPrefs.GetInt("HighScore", 0);
        }
    }

    public void ResetScore()
    {
        score = 0;
        huCount = 0;
        currentLoop = 1;
        huCountInCurrentLoop = 0;

        // 【核心修改】
        OnScoreChanged?.Invoke(score);
    }

    public bool IncrementHuCountAndCheckCycle()
    {
        huCount++;
        huCountInCurrentLoop++;

        int target = GetCurrentLoopTarget();

        if (huCountInCurrentLoop >= target)
        {
            currentLoop++;
            huCountInCurrentLoop = 0;
            return true;
        }

        return false;
    }

    public string GetLoopProgressString()
    {
        int target = GetCurrentLoopTarget();
        string format = "第{0}圈 {1}/{2}";

        if (LocalizationManager.Instance)
        {
            format = LocalizationManager.Instance.GetText("GAME_LOOP");
        }

        return string.Format(format, currentLoop, huCountInCurrentLoop + 1, target);
    }

    public void AddHuCount(int amount)
    {
        huCount += amount;
        huCountInCurrentLoop += amount;

        int target = GetCurrentLoopTarget();

        while (huCountInCurrentLoop >= target)
        {
            currentLoop++;
            huCountInCurrentLoop -= target;
        }
    }

    // --- Getter/Setter ---

    public int GetHuCount() => huCount;
    public int GetHuCountInCycle() => huCountInCurrentLoop;

    // 【核心修改】返回值改为 long
    public long GetCurrentScore() => score;
    public int GetCurrentLoop() => currentLoop;

    // 【核心修改】参数改为 long
    public void AddScore(long amount)
    {
        score += amount;
        OnScoreChanged?.Invoke(score);
    }

    // 【核心修改】参数改为 long
    public bool CheckForNewHighScore(long finalScore)
    {
        if (finalScore > _highScore)
        {
            _highScore = finalScore;

            // 【核心修改】存为字符串以支持超大数值
            PlayerPrefs.SetString("HighScore_Long", _highScore.ToString());
            PlayerPrefs.Save();

            // 顺便也更新一下旧的int存档，防止其他未修改的地方报错 (虽然存不下会溢出，但为了兼容性)
            PlayerPrefs.SetInt("HighScore", (int)Mathf.Clamp(finalScore, 0, int.MaxValue));

            return true;
        }
        return false;
    }

    private int GetCurrentLoopTarget()
    {
        int target = 4;
        if (GameManager.Instance != null && GameManager.Instance.GetSettings() != null)
        {
            target = GameManager.Instance.GetSettings().husPerLoop;
        }

        if (GameManager.Instance != null && GameManager.Instance.isSubspaceActive)
        {
            target = Mathf.Max(1, target - 1);
        }

        return target;
    }

    public void SetProgressToLastRound()
    {
        int target = GetCurrentLoopTarget();
        huCountInCurrentLoop = Mathf.Max(0, target - 1);
    }
}