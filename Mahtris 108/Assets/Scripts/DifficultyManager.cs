// FileName: DifficultyManager.cs
using UnityEngine;

// 1. 【修改】增加 Unmatched = 3
public enum Difficulty { Easy = 0, Normal = 1, Hard = 2, Unmatched = 3 }

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    public Difficulty CurrentDifficulty { get; private set; } = Difficulty.Easy;

    // 当前已解锁的最高等级 (0, 1, 2, 3)
    public int MaxUnlockedLevel { get; private set; } = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadData()
    {
        CurrentDifficulty = SaveManager.LoadSelectedDifficulty();
        MaxUnlockedLevel = SaveManager.LoadUnlockedLevel();
    }

    public void SetDifficulty(Difficulty newDifficulty)
    {
        // 安全检查
        if ((int)newDifficulty > MaxUnlockedLevel)
        {
            Debug.LogWarning("试图选择未解锁的难度，操作被拦截。");
            newDifficulty = Difficulty.Easy;
        }

        CurrentDifficulty = newDifficulty;
        SaveManager.SaveSelectedDifficulty(newDifficulty);
    }

    public void CompleteDifficulty(Difficulty difficultyJustBeaten)
    {
        int levelBeaten = (int)difficultyJustBeaten;

        // 【修改】现在最高等级是 3 (Unmatched)，所以判断 < 3
        if (levelBeaten == MaxUnlockedLevel && MaxUnlockedLevel < 3)
        {
            MaxUnlockedLevel++;
            SaveManager.SaveUnlockedLevel(MaxUnlockedLevel);
            Debug.Log($"新难度解锁！当前最高解锁等级: {MaxUnlockedLevel}");
        }
    }

    // 【新增】调试用：一键解锁所有难度
    public void UnlockAllForTesting()
    {
        MaxUnlockedLevel = 3;
        SaveManager.SaveUnlockedLevel(3);
    }
}