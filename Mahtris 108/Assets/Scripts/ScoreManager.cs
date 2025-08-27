// FileName: ScoreManager.cs
using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static event Action<int> OnScoreChanged;

    private int score;
    private int huCount; // �����������ڼ�¼���ƴ����ı���

    // �����������ⲿ��ȡ���ƴ����ķ���
    public int GetHuCount()
    {
        return huCount;
    }

    public void AddScore(int amount)
    {
        score += amount;
        GameEvents.TriggerScoreChanged(score);
        OnScoreChanged?.Invoke(score);
    }

    public void IncrementHuCount()
    {
        huCount++;
    }

    public void ResetScore()
    {
        score = 0;
        huCount = 0; // ������Ϸʱ�����ƴ���Ҳһ������
        GameEvents.TriggerScoreChanged(score);
        OnScoreChanged?.Invoke(score);
    }
}