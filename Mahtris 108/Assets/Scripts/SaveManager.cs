// FileName: SaveManager.cs
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class SaveManager
{
    // ========================================================================
    // 1. 键名定义 (Keys)
    // ========================================================================
    private const string GoldKey = "PlayerGold";

    // 最高分 (兼容旧版 Int 和新版 Long)
    private const string HighScoreKey_Int = "HighScore";
    private const string HighScoreKey_Long = "HighScore_Long";

    // 设置相关
    private const string BgmKey = "Setting_BgmOn";
    private const string SfxKey = "Setting_SfxOn";
    private const string FullscreenKey = "Setting_IsFullscreen";
    private const string LanguageKey = "Setting_Language";

    // 游戏进度相关
    private const string SelectedDiffKey = "Meta_SelectedDifficulty";
    private const string UnlockedDiffKey = "Meta_UnlockedDifficultyLevel";


    // ========================================================================
    // 2. 金币存取
    // ========================================================================
    public static void SaveGold(int goldAmount)
    {
        PlayerPrefs.SetInt(GoldKey, goldAmount);
        PlayerPrefs.Save();
    }

    public static int LoadGold()
    {
        return PlayerPrefs.GetInt(GoldKey, 0);
    }

    // ========================================================================
    // 3. 最高分存取 (支持 long)
    // ========================================================================
    public static void SaveHighScore(long score)
    {
        // 1. 存新的 long (字符串形式)
        PlayerPrefs.SetString(HighScoreKey_Long, score.ToString());

        // 2. 存旧的 int (为了防止旧代码报错，截断存储)
        int scoreInt = (int)Mathf.Clamp(score, 0, int.MaxValue);
        PlayerPrefs.SetInt(HighScoreKey_Int, scoreInt);

        PlayerPrefs.Save();
    }

    public static long LoadHighScore()
    {
        // 1. 优先尝试读取 long
        string strVal = PlayerPrefs.GetString(HighScoreKey_Long, "");
        if (!string.IsNullOrEmpty(strVal) && long.TryParse(strVal, out long result))
        {
            return result;
        }

        // 2. 只有 int 存档时，读取 int (兼容老存档)
        return PlayerPrefs.GetInt(HighScoreKey_Int, 0);
    }

    // ========================================================================
    // 4. 设置存取 (BGM, SFX, 全屏, 语言) - 【修复报错的关键部分】
    // ========================================================================

    // BGM
    public static bool LoadBgmState()
    {
        return PlayerPrefs.GetInt(BgmKey, 1) == 1;
    }
    public static void SaveBgmState(bool isOn)
    {
        PlayerPrefs.SetInt(BgmKey, isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    // SFX
    public static bool LoadSfxState()
    {
        return PlayerPrefs.GetInt(SfxKey, 1) == 1;
    }
    public static void SaveSfxState(bool isOn)
    {
        PlayerPrefs.SetInt(SfxKey, isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    // 全屏
    public static bool LoadFullscreenState()
    {
        return PlayerPrefs.GetInt(FullscreenKey, 0) == 1;
    }
    public static void SaveFullscreenState(bool isFullscreen)
    {
        PlayerPrefs.SetInt(FullscreenKey, isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    // 语言
    public static string LoadLanguage()
    {
        return PlayerPrefs.GetString(LanguageKey, "");
    }
    public static void SaveLanguage(string langCode)
    {
        PlayerPrefs.SetString(LanguageKey, langCode);
        PlayerPrefs.Save();
    }

    // ========================================================================
    // 5. 难度与解锁进度
    // ========================================================================
    public static Difficulty LoadSelectedDifficulty()
    {
        return (Difficulty)PlayerPrefs.GetInt(SelectedDiffKey, 0);
    }
    public static void SaveSelectedDifficulty(Difficulty difficulty)
    {
        PlayerPrefs.SetInt(SelectedDiffKey, (int)difficulty);
        PlayerPrefs.Save();
    }

    public static int LoadUnlockedLevel()
    {
        return PlayerPrefs.GetInt(UnlockedDiffKey, 0);
    }
    public static void SaveUnlockedLevel(int level)
    {
        PlayerPrefs.SetInt(UnlockedDiffKey, level);
        PlayerPrefs.Save();
    }

    // ========================================================================
    // 6. 商店解锁 (道具与条约)
    // ========================================================================
    public static bool IsItemUnlocked(string itemName, bool isInitial)
    {
        if (isInitial) return true;
        return PlayerPrefs.GetInt($"Unlock_Item_{itemName}", 0) == 1;
    }

    public static void UnlockItem(string itemName)
    {
        PlayerPrefs.SetInt($"Unlock_Item_{itemName}", 1);
        PlayerPrefs.Save();
    }

    public static bool IsProtocolUnlocked(string protocolName, bool isInitial)
    {
        if (isInitial) return true;
        return PlayerPrefs.GetInt($"Unlock_Protocol_{protocolName}", 0) == 1;
    }

    public static void UnlockProtocol(string protocolName)
    {
        PlayerPrefs.SetInt($"Unlock_Protocol_{protocolName}", 1);
        PlayerPrefs.Save();
    }

    // ========================================================================
    // 7. 调试与清除
    // ========================================================================

    // 运行时清除 (供测试按钮调用)
    public static void DeleteAllSaveData()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("【系统】所有存档数据已清除！");
    }

#if UNITY_EDITOR
    // 编辑器菜单清除
    [MenuItem("游戏/清除玩家存档")]
    public static void ClearSaveData()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("玩家存档已被清除！");
    }
#endif
}