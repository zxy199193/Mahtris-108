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