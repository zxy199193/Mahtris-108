// FileName: GameSessionConfig.cs
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 这个类用于存储一局游戏内，根据难度动态调整后的配置。
/// 它不是 MonoBehaviour，只是一个纯数据容器。
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