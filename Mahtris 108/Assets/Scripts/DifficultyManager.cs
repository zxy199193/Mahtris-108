// FileName: DifficultyManager.cs
using UnityEngine;

// 确保枚举顺序对应：0=Easy, 1=Normal, 2=Hard
public enum Difficulty { Easy = 0, Normal = 1, Hard = 2 }

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    public Difficulty CurrentDifficulty { get; private set; } = Difficulty.Easy;

    // 【新增】当前已解锁的最高等级 (0, 1, or 2)
    public int MaxUnlockedLevel { get; private set; } = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 【新增】初始化时读取存档
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
        // 安全检查：如果试图选择未解锁的难度，强制切回 Easy
        if ((int)newDifficulty > MaxUnlockedLevel)
        {
            Debug.LogWarning("试图选择未解锁的难度，操作被拦截。");
            newDifficulty = Difficulty.Easy;
        }

        CurrentDifficulty = newDifficulty;
        // 【新增】保存选择
        SaveManager.SaveSelectedDifficulty(newDifficulty);
    }

    // 【新增】供 GameManager 在通关时调用
    public void CompleteDifficulty(Difficulty difficultyJustBeaten)
    {
        int levelBeaten = (int)difficultyJustBeaten;

        // 如果通关的难度 等于 当前解锁的最高难度，且不是最后一级(Hard=2)
        // 那么解锁下一级
        if (levelBeaten == MaxUnlockedLevel && MaxUnlockedLevel < 2)
        {
            MaxUnlockedLevel++;
            SaveManager.SaveUnlockedLevel(MaxUnlockedLevel);
            Debug.Log($"新难度解锁！当前最高解锁等级: {MaxUnlockedLevel}");
        }
    }
}