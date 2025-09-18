// FileName: ScoreManager.cs
using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static event Action<int> OnScoreChanged;

    private int score;
    private int huCount;
    private int huCountInCycle;
    private int _highScore; // ���������ڻ�����߷�

    void Start()
    {
        // ��Ϸ��ʼʱ����һ����߷�
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

        // ����������鲢������߷�
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

        // ���¼�����߷֣��Ա���ͬһ�Ự�п�ʼ����Ϸ
        _highScore = SaveManager.LoadHighScore();

        OnScoreChanged?.Invoke(score);
    }
}