// FileName: GameSettings.cs
using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Game/Game Settings", order = 1)]
public class GameSettings : ScriptableObject
{
    [Header("游戏面板设置")]
    [Tooltip("游戏区域的宽度（格子数）")]
    public int gridWidth = 10;
    [Tooltip("游戏区域的高度（格子数）")]
    public int gridHeight = 20;

    [Header("麻将牌库设置")]
    [Tooltip("筒、万、条每种牌的数量（1-9）")]
    public int tileRanks = 9;
    [Tooltip("每种牌的重复数量")]
    public int tilesPerRank = 4;

    // 计算属性，确保它存在
    public int UniqueTileCount => tileRanks * 3;
    public int TotalTileCount => UniqueTileCount * tilesPerRank;

    [Header("游戏逻辑设置")]
    [Tooltip("胡牌需要的最少面子数（刻子/顺子/杠子）")]
    public int setsForHu = 4;
    [Tooltip("每次胡牌后，方块下降速度增加的百分比（例如0.05代表5%）")]
    [Range(0f, 0.5f)]
    public float speedIncreasePerHu = 0.05f;
    [Tooltip("初始下落速度（秒/格）")]
    public float initialFallSpeed = 1.0f;
    [Tooltip("方块加速下落时的速度倍率")]
    public float fastFallMultiplier = 20f;

    [Header("得分设置")]
    [Tooltip("胡牌时的基础得分")]
    public int baseHuScore = 100;
    [Tooltip("每有一组杠牌时，胡牌的额外加分")]
    public int scoreBonusPerKong = 100;
    [Tooltip("每消除一行获得的基础分")]
    public int scorePerRow = 10;
}
