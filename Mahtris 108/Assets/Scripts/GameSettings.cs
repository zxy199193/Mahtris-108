// FileName: GameSettings.cs
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class TetrominoLevel
{
    public string levelName = "Lv.1";
    [Tooltip("该等级包含的最少方块数")]
    public int minBlocks = 1;
    [Tooltip("该等级包含的最多方块数")]
    public int maxBlocks = 4;
}

[CreateAssetMenu(fileName = "GameSettings", menuName = "Game/Game Settings", order = 1)]
public class GameSettings : ScriptableObject
{
    [Header("游戏面板设置")]
    public int gridWidth = 10;
    public int gridHeight = 20;
    // ---【更新点】---
    // 移除了 [Range(15, 20)] 限制，现在可以任意定制
    [Tooltip("游戏失败的死亡线高度，方块接触或越过此高度则游戏结束")]
    public int deadlineHeight = 18;

    [Header("麻将牌库设置")]
    public int tileRanks = 9;
    public int tilesPerRank = 4;
    public int UniqueTileCount => tileRanks * 3;
    public int TotalTileCount => UniqueTileCount * tilesPerRank;

    [Header("游戏逻辑设置")]
    public int setsForHu = 4;
    [Range(0f, 0.5f)]
    public float speedIncreasePerHu = 0.05f;
    public float initialFallSpeed = 1.0f;
    public float fastFallMultiplier = 20f;

    [Header("得分设置")]
    [Tooltip("四川麻将计番算法的【基本分】")]
    public int baseFanScore = 50;
    [Tooltip("每有一组杠牌时，额外增加的【番数】")]
    public int fanBonusPerKong = 1;
    public int scorePerRow = 10;

    [Header("Tetromino 等级配置")]
    public List<TetrominoLevel> tetrominoLevels;
}