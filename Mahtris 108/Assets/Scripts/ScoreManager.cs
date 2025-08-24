// FileName: ScoreManager.cs
using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    // --- �������㡿---
    // ȷ����������ľ�̬�¼����ڣ��Ա������ű����Զ���
    public static event Action<int> OnScoreChanged;

    private int score;

    public void AddScore(int amount)
    {
        score += amount;
        GameEvents.TriggerScoreChanged(score); // ����ȫ���¼�������UI��
        OnScoreChanged?.Invoke(score);         // ��������̬�¼�������GameManager�Ⱥ����߼�
    }

    public void ResetScore()
    {
        score = 0;
        GameEvents.TriggerScoreChanged(score);
        OnScoreChanged?.Invoke(score);
    }
}