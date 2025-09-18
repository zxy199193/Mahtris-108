// FileName: SaveManager.cs
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class SaveManager
{
    // �������ڴ洢���ݵļ� (Key)
    private const string GoldKey = "PlayerGold";
    private const string HighScoreKey = "PlayerHighScore";

    // --- ��Ҵ浵 ---
    public static void SaveGold(int goldAmount)
    {
        PlayerPrefs.SetInt(GoldKey, goldAmount);
        PlayerPrefs.Save(); // ȷ����������д�����
    }

    public static int LoadGold()
    {
        // ���û�д浵����Ĭ��Ϊ0
        return PlayerPrefs.GetInt(GoldKey, 0);
    }

    // --- ��߷ִ浵 ---
    public static void SaveHighScore(int score)
    {
        PlayerPrefs.SetInt(HighScoreKey, score);
        PlayerPrefs.Save();
    }

    public static int LoadHighScore()
    {
        return PlayerPrefs.GetInt(HighScoreKey, 0);
    }

    // --- �༭������ ---
#if UNITY_EDITOR
    [MenuItem("��Ϸ/�����Ҵ浵")]
    public static void ClearSaveData()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("��Ҵ浵�ѱ������");
    }
#endif
}