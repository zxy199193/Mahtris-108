// FileName: GameSettings.cs
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class TetrominoLevel
{
    public string levelName = "Lv.1";
    [Tooltip("该等级包含的最少方块数")]
    public int minBlocks = 3;
    [Tooltip("该等级包含的最多方块数")]
    public int maxBlocks = 3;
}

[System.Serializable]
public class ScoreLevel
{
    [Tooltip("该等级需要达到的目标分数")]
    public int targetScore;
    [Tooltip("达成目标后奖励的金币数量")]
    public int goldReward;
}

[System.Serializable]
public class BlockRewardWeights
{
    [Range(0f, 1f)] public float level1Weight = 0.35f;
    [Range(0f, 1f)] public float level2Weight = 0.50f;
    [Range(0f, 1f)] public float level3Weight = 0.15f;
}

[CreateAssetMenu(fileName = "GameSettings", menuName = "Game/Game Settings", order = 1)]
public class GameSettings : ScriptableObject
{
    [Header("游戏面板与逻辑")]
    public int gridWidth = 10;
    public int gridHeight = 20;
    public int deadlineHeight = 18;
    public int setsForHu = 4;
    [Tooltip("每次胡牌后速度增加初始速度的百分比")]
    [Range(0f, 1f)]
    public float speedIncreasePerHu = 0.2f;
    public float initialFallSpeed = 1.0f; // (此字段将被新逻辑忽略)
    public float fastFallSpeed = 0.05f; // (此字段保持不变, 用于快速下落)

    [Header("新速度系统 (V4.1)")]
    [Tooltip("简单难度下的基础速度等级 (例如: 10)")]
    public int baseDisplayedSpeed = 10;
    [Tooltip("每次胡牌后, 速度等级增加的整数值 (例如: 2)")]
    public int speedIncreasePerHu_Int = 2;

    [Header("麻将牌库")]
    public int tileRanks = 9;
    public int tilesPerRank = 4;
    public int UniqueTileCount => tileRanks * 3;
    public int TotalTileCount => UniqueTileCount * tilesPerRank;

    [Header("得分系统")]
    public int baseFanScore = 10;
    public int fanBonusPerKong = 1;
    public int scorePerRow = 10;

    [Header("游戏循环配置")]
    [Tooltip("完成一圈所需的胡牌次数 (默认为4)")]
    public int husPerLoop = 4;

    [Header("传奇系统")]
    [Tooltip("常规道具/条约的权重 (例如 10)")]
    public float normalWeight = 10f;
    [Tooltip("传奇道具/条约的基础权重 (例如 5)")]
    public float legendaryWeightBase = 5f;
    [Tooltip("每完成一圈，传奇权重增加的值 (例如 2.5)")]
    public float legendaryWeightIncreasePerLoop = 2.5f;
    [Tooltip("传奇图标 (用于UI显示)")]
    public Sprite legendaryIcon;
    
    [Header("UI 样式配置")]
    public Sprite tooltipBgCommon;    // 普通道具背板
    public Sprite tooltipBgAdvanced;  // 高级道具背板
    public Sprite tooltipBgProtocol;  // 条约背板
    public Sprite tooltipBgLegendary; // 传奇背板

    [Header("浮窗标签颜色配置")] // 【新增】
    public Color labelColorCommon = new Color(0.2f, 0.6f, 1f);
    public Color labelColorAdvanced = new Color(1f, 0.5f, 0f);
    public Color labelColorProtocol = new Color(0.5f, 0.5f, 0.5f);

    [Header("目标分数与时间")]
    public float initialTimeLimit = 180f;
    public float huTimeBonus = 60f;
    public List<ScoreLevel> scoreLevels;

    [Header("Tetromino 等级")]
    public List<TetrominoLevel> tetrominoLevels;

    [Header("胡牌奖励配置")]
    public BlockRewardWeights commonBlockRewardWeights;
    public BlockRewardWeights advancedBlockRewardWeights;

    [Header("道具系统")]
    public int itemSlotCount = 3;
    public List<ItemData> commonItemPool;
    public List<ItemData> advancedItemPool;

    [Header("条约系统")]
    public int maxProtocolCount = 5;
    public List<ProtocolData> protocolPool;
}