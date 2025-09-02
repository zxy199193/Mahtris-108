// FileName: ScoreManager.cs
using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static event Action<int> OnScoreChanged;

    private int score;
    private int huCount;
    private int huCountInCycle;

    public int GetHuCount()
    {
        return huCount;
    }

    public int GetHuCountInCycle()
    {
        return huCountInCycle;
    }

    // ����ֵ: bool, �����Ƿ������һȦ�ġ��߼�����������
    public bool IncrementHuCountAndCheckCycle()
    {
        huCount++;
        huCountInCycle++;
        if (huCountInCycle >= 4)
        {
            huCountInCycle = 0; // ����Ȧ�ڼ���
            return true; // �Ǹ߼�����
        }
        return false; // ����ͨ����
    }

    public void AddScore(int amount)
    {
        score += amount;
        OnScoreChanged?.Invoke(score);
    }

    public void ResetScore()
    {
        score = 0;
        huCount = 0;
        huCountInCycle = 0;
        OnScoreChanged?.Invoke(score);
    }
}