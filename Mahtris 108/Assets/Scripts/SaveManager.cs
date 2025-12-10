// FileName: SaveManager.cs
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class SaveManager
{
    // 定义用于存储数据的键 (Key)
    private const string GoldKey = "PlayerGold";
    private const string HighScoreKey = "PlayerHighScore";
    private const string BgmKey = "Setting_BgmOn";
    private const string SfxKey = "Setting_SfxOn";
    private const string FullscreenKey = "Setting_IsFullscreen";
    private const string SelectedDiffKey = "Meta_SelectedDifficulty";
    private const string UnlockedDiffKey = "Meta_UnlockedDifficultyLevel";

    // --- 金币存档 ---
    public static void SaveGold(int goldAmount)
    {
        PlayerPrefs.SetInt(GoldKey, goldAmount);
        PlayerPrefs.Save(); // 确保数据立即写入磁盘
    }

    public static int LoadGold()
    {
        // 如果没有存档，则默认为0
        return PlayerPrefs.GetInt(GoldKey, 0);
    }

    // --- 最高分存档 ---
    public static void SaveHighScore(int score)
    {
        PlayerPrefs.SetInt(HighScoreKey, score);
        PlayerPrefs.Save();
    }

    public static int LoadHighScore()
    {
        return PlayerPrefs.GetInt(HighScoreKey, 0);
    }
    public static bool LoadBgmState()
    {
        return PlayerPrefs.GetInt(BgmKey, 1) == 1;
    }

    public static void SaveBgmState(bool isOn)
    {
        PlayerPrefs.SetInt(BgmKey, isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static bool LoadSfxState()
    {
        return PlayerPrefs.GetInt(SfxKey, 1) == 1;
    }

    public static void SaveSfxState(bool isOn)
    {
        PlayerPrefs.SetInt(SfxKey, isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static bool LoadFullscreenState()
    {
        return PlayerPrefs.GetInt(FullscreenKey, 0) == 1;
    }

    public static void SaveFullscreenState(bool isFullscreen)
    {
        PlayerPrefs.SetInt(FullscreenKey, isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static Difficulty LoadSelectedDifficulty()
    {
        // 默认为 Easy (0)
        return (Difficulty)PlayerPrefs.GetInt(SelectedDiffKey, 0);
    }
    public static void SaveSelectedDifficulty(Difficulty difficulty)
    {
        PlayerPrefs.SetInt(SelectedDiffKey, (int)difficulty);
        PlayerPrefs.Save();
    }
    public static int LoadUnlockedLevel()
    {
        // 默认只解锁到 0 (Easy)
        return PlayerPrefs.GetInt(UnlockedDiffKey, 0);
    }

    public static void SaveUnlockedLevel(int level)
    {
        PlayerPrefs.SetInt(UnlockedDiffKey, level);
        PlayerPrefs.Save();
    }

    // --- 商店解锁存档 ---

    // 检查道具是否解锁
    public static bool IsItemUnlocked(string itemName, bool isInitial)
    {
        // 如果是初始道具，或者存档里标记为 1，则视为解锁
        if (isInitial) return true;
        return PlayerPrefs.GetInt($"Unlock_Item_{itemName}", 0) == 1;
    }

    // 解锁道具
    public static void UnlockItem(string itemName)
    {
        PlayerPrefs.SetInt($"Unlock_Item_{itemName}", 1);
        PlayerPrefs.Save();
    }

    // 检查条约是否解锁
    public static bool IsProtocolUnlocked(string protocolName, bool isInitial)
    {
        if (isInitial) return true;
        return PlayerPrefs.GetInt($"Unlock_Protocol_{protocolName}", 0) == 1;
    }

    // 解锁条约
    public static void UnlockProtocol(string protocolName)
    {
        PlayerPrefs.SetInt($"Unlock_Protocol_{protocolName}", 1);
        PlayerPrefs.Save();
    }

    // --- 编辑器功能 ---
#if UNITY_EDITOR
    [MenuItem("游戏/清除玩家存档")]
    public static void ClearSaveData()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("玩家存档已被清除！");
    }
#endif
}