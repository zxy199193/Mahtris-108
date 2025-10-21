// FileName: GameSessionConfig.cs
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ��������ڴ洢һ����Ϸ�ڣ������Ѷȶ�̬����������á�
/// ������ MonoBehaviour��ֻ��һ��������������
/// </summary>
public class GameSessionConfig
{
    public List<GameObject> InitialTetrominoes { get; set; }
    public List<ScoreLevel> DifficultyScoreLevels { get; set; }
    public float InitialFallSpeed { get; set; }

    public GameSessionConfig()
    {
        InitialTetrominoes = new List<GameObject>();
        DifficultyScoreLevels = new List<ScoreLevel>();
        InitialFallSpeed = 1.0f;
    }
}