using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    // ===在此处定义所有需要保存的数据===
    public int playerGold = 0;
    public long highScore = 0;

    // 设置
    public bool bgmOn = true;
    public bool sfxOn = true;
    public bool isFullscreen = false;
    public string language = "";

    // 游戏进度
    public int selectedDifficulty = 0;
    public int unlockedLevel = 0;

    // 商店解锁状态 (使用 List 存储键名，方便 JSON 序列化)
    public List<string> unlockedItems = new List<string>();
    public List<string> unlockedProtocols = new List<string>();
}

public static class SaveManager
{
    // 存档文件名
    private const string SAVE_FILE_NAME = "save.json";

    // 内存中的缓存数据
    private static SaveData _cachedData = null;

    // 获取存档文件的完整路径
    // 路径就是您刚才找到的: AppData\LocalLow\Dabbido Studio\Mahtris 108\save.json
    private static string SavePath => Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);

    // ========================================================================
    // 核心：读取数据 (优先读文件，没有文件则尝试从注册表迁移)
    // ========================================================================
    private static SaveData Data
    {
        get
        {
            if (_cachedData == null)
            {
                LoadData();
            }
            return _cachedData;
        }
    }

    private static void LoadData()
    {
        if (File.Exists(SavePath))
        {
            // === 情况A：有文件，直接读取 ===
            try
            {
                string json = File.ReadAllText(SavePath);
                _cachedData = JsonUtility.FromJson<SaveData>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"存档读取失败: {e.Message}, 重置为默认。");
                _cachedData = new SaveData();
            }
        }
        else
        {
            // === 情况B：没文件，尝试从旧版 PlayerPrefs 迁移 ===
            Debug.Log("未找到存档文件，尝试从注册表迁移旧数据...");
            _cachedData = new SaveData();

            // 1. 迁移金币
            if (PlayerPrefs.HasKey("PlayerGold"))
                _cachedData.playerGold = PlayerPrefs.GetInt("PlayerGold");

            // 2. 迁移最高分
            string scoreStr = PlayerPrefs.GetString("HighScore_Long", "");
            if (long.TryParse(scoreStr, out long score)) _cachedData.highScore = score;
            else _cachedData.highScore = PlayerPrefs.GetInt("HighScore", 0);

            // 3. 迁移设置
            _cachedData.bgmOn = PlayerPrefs.GetInt("Setting_BgmOn", 1) == 1;
            _cachedData.sfxOn = PlayerPrefs.GetInt("Setting_SfxOn", 1) == 1;
            _cachedData.isFullscreen = PlayerPrefs.GetInt("Setting_IsFullscreen", 0) == 1;
            _cachedData.language = PlayerPrefs.GetString("Setting_Language", "");

            // 4. 迁移进度
            _cachedData.selectedDifficulty = PlayerPrefs.GetInt("Meta_SelectedDifficulty", 0);
            _cachedData.unlockedLevel = PlayerPrefs.GetInt("Meta_UnlockedDifficultyLevel", 0);

            // 注意：解锁列表较难从 PlayerPrefs 反向迁移，除非遍历所有可能的 ID。
            // 如果您的物品 ID 是固定的，可以在这里手动检查一下。
            // 否则新版本会让玩家重新解锁（或者您可以写个逻辑把所有常见ID都查一遍）

            // 迁移完成后立即保存成文件
            Save();
        }
    }

    // ========================================================================
    // 核心：保存数据 (写入硬盘)
    // ========================================================================
    public static void Save()
    {
        if (_cachedData == null) return;

        try
        {
            string json = JsonUtility.ToJson(_cachedData, true); // true = 格式化输出，方便人类阅读
            File.WriteAllText(SavePath, json);
            // Debug.Log("存档已保存至: " + SavePath);
        }
        catch (Exception e)
        {
            Debug.LogError($"存档保存失败: {e.Message}");
        }
    }

    // ========================================================================
    // 外部接口 (保持与旧代码一致，不需要改动其他脚本)
    // ========================================================================

    // 金币
    public static void SaveGold(int amount) { Data.playerGold = amount; Save(); }
    public static int LoadGold() { return Data.playerGold; }

    // 最高分
    public static void SaveHighScore(long score) { Data.highScore = score; Save(); }
    public static long LoadHighScore() { return Data.highScore; }

    // 设置
    public static bool LoadBgmState() { return Data.bgmOn; }
    public static void SaveBgmState(bool isOn) { Data.bgmOn = isOn; Save(); }

    public static bool LoadSfxState() { return Data.sfxOn; }
    public static void SaveSfxState(bool isOn) { Data.sfxOn = isOn; Save(); }

    public static bool LoadFullscreenState() { return Data.isFullscreen; }
    public static void SaveFullscreenState(bool isFull) { Data.isFullscreen = isFull; Save(); }

    public static string LoadLanguage() { return Data.language; }
    public static void SaveLanguage(string lang) { Data.language = lang; Save(); }

    // 难度
    public static Difficulty LoadSelectedDifficulty() { return (Difficulty)Data.selectedDifficulty; }
    public static void SaveSelectedDifficulty(Difficulty diff) { Data.selectedDifficulty = (int)diff; Save(); }

    public static int LoadUnlockedLevel() { return Data.unlockedLevel; }
    public static void SaveUnlockedLevel(int level) { Data.unlockedLevel = level; Save(); }

    // 解锁系统 (适配 List)
    public static bool IsItemUnlocked(string itemName, bool isInitial)
    {
        if (isInitial) return true;
        return Data.unlockedItems.Contains(itemName);
    }
    public static void UnlockItem(string itemName)
    {
        if (!Data.unlockedItems.Contains(itemName))
        {
            Data.unlockedItems.Add(itemName);
            Save();
        }
    }

    public static bool IsProtocolUnlocked(string pName, bool isInitial)
    {
        if (isInitial) return true;
        return Data.unlockedProtocols.Contains(pName);
    }
    public static void UnlockProtocol(string pName)
    {
        if (!Data.unlockedProtocols.Contains(pName))
        {
            Data.unlockedProtocols.Add(pName);
            Save();
        }
    }

    // ========================================================================
    // 7. 调试与清除 (修正版)
    // ========================================================================

    // 运行时清除 (供游戏内的重置按钮调用)
    public static void DeleteAllSaveData()
    {
        // 1. 删除物理存档文件
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
        }

        // 2. 清除内存缓存
        _cachedData = new SaveData(); // 重置为空数据

        // 3. 清除注册表 (为了兼容性，把旧的 PlayerPrefs 也删了)
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        Debug.Log("【系统】所有存档数据（文件+注册表）已彻底清除！");
    }

#if UNITY_EDITOR
    // 【新增】恢复 Unity 编辑器顶部菜单按钮
    // 点击菜单栏的 "游戏" -> "清除玩家存档" 即可触发
    [UnityEditor.MenuItem("游戏/清除玩家存档")]
    public static void ClearSaveDataMenu()
    {
        // 编辑器模式下可能没有运行 Application.persistentDataPath，
        // 但 SavePath 属性依然能获取到正确的路径。

        string path = SavePath;
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"[Editor] 已删除存档文件: {path}");
        }

        PlayerPrefs.DeleteAll();
        Debug.Log("[Editor] 已清除 PlayerPrefs 注册表数据。");

        // 清除缓存防止编辑器不重启直接运行读到旧数据
        _cachedData = null;
    }
#endif
}