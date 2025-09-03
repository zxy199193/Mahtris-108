// FileName: ScoreManager.cs
using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    // ����ű��ڲ����¼�������Ҫ������ͳһʹ��ȫ���¼�
    // public static event Action<int> OnScoreChanged;
    public static event System.Action<int> OnScoreChanged;
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
        OnScoreChanged?.Invoke(score); // �����¼���֪ͨGameManager
        // ---���ش������㡿---
        // ����ȫ�ֵ� GameEvents �����������仯�¼�
        GameEvents.TriggerScoreChanged(score);
    }

    public void ResetScore()
    {
        score = 0;
        huCount = 0;
        huCountInCycle = 0;
        OnScoreChanged?.Invoke(score);
        // ---���ش������㡿---
        // ����ȫ�ֵ� GameEvents �����������仯�¼�
        GameEvents.TriggerScoreChanged(score);
    }
}