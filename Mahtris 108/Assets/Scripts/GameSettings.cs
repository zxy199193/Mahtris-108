// FileName: GameSettings.cs
using UnityEngine;
using System.Collections.Generic;

// ========================================================================
// 辅助数据结构定义
// ========================================================================

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

// ========================================================================
// 核心配置类
// ========================================================================

[CreateAssetMenu(fileName = "GameSettings", menuName = "Game/Game Settings", order = 1)]
public class GameSettings : ScriptableObject
{
    // ========================================================================
    // 0. 调试与开发配置
    // ========================================================================
    [Header("--- 调试与开发 ---")]
    [Tooltip("是否开启测试模式? (开启后将使用 Spawner 中的 Initial Tetromino Prefabs，且无视解锁条件)")]
    public bool isTestMode = false;

    // ========================================================================
    // 1. 核心游戏机制 (网格 / 麻将规则)
    // ========================================================================
    [Header("--- 核心机制配置 ---")]
    [Tooltip("网格宽度")]
    public int gridWidth = 10;
    [Tooltip("网格高度")]
    public int gridHeight = 20;
    [Tooltip("死亡线高度")]
    public int deadlineHeight = 18;

    [Space(10)]
    [Tooltip("胡牌所需的面子数组合数 (默认为4)")]
    public int setsForHu = 4;

    [Header("麻将牌库规则")]
    public int tileRanks = 9;      // 牌的序数 (1-9)
    public int tilesPerRank = 4;   // 每张牌的张数
    public int UniqueTileCount => tileRanks * 3; // 3种花色
    public int TotalTileCount => UniqueTileCount * tilesPerRank;

    // ========================================================================
    // 2. 速度与物理系统
    // ========================================================================
    [Header("--- 速度与物理 (V4.1) ---")]
    [Tooltip("简单难度下的基础速度等级 (例如: 10)")]
    public int baseDisplayedSpeed = 10;

    [Tooltip("每次胡牌后, 速度等级增加的整数值 (例如: 2)")]
    public int speedIncreasePerHu_Int = 2;

    [Tooltip("按住下落键时的快速下落速度 (秒/格)")]
    public float fastFallSpeed = 0.05f;

    [Header("旧版速度参数 (部分已弃用)")]
    [Tooltip("每次胡牌后速度增加初始速度的百分比 (旧逻辑)")]
    [Range(0f, 1f)]
    public float speedIncreasePerHu = 0.2f;
    [Tooltip("初始下落速度 (旧逻辑，可能被新系统忽略)")]
    public float initialFallSpeed = 1.0f;

    // ========================================================================
    // 3. 得分与关卡目标
    // ========================================================================
    [Header("--- 得分与关卡目标 ---")]
    [Tooltip("初始游戏时间 (秒)")]
    public float initialTimeLimit = 180f;
    [Tooltip("每次胡牌奖励的时间 (秒)")]
    public float huTimeBonus = 60f;

    [Space(10)]
    [Tooltip("基础番型的底分")]
    public int baseFanScore = 10;
    [Tooltip("每个杠的额外番数")]
    public int fanBonusPerKong = 1;
    [Tooltip("每消除一行的得分")]
    public int scorePerRow = 10;

    [Space(10)]
    [Tooltip("关卡分数目标列表")]
    public List<ScoreLevel> scoreLevels;

    // ========================================================================
    // 4. 循环与传奇系统
    // ========================================================================
    [Header("--- 循环与传奇系统 ---")]
    [Tooltip("完成一圈所需的胡牌次数 (默认为4)")]
    public int husPerLoop = 4;

    [Space(10)]
    [Tooltip("常规道具/条约的随机权重 (例如 10)")]
    public float normalWeight = 10f;
    [Tooltip("传奇道具/条约的基础随机权重 (例如 5)")]
    public float legendaryWeightBase = 5f;
    [Tooltip("每完成一圈，传奇权重增加的值 (例如 2.5)")]
    public float legendaryWeightIncreasePerLoop = 2.5f;

    // ========================================================================
    // 5. 奖励与生成配置 (方块 / 道具 / 条约)
    // ========================================================================
    [Header("--- 奖励与生成配置 ---")]

    [Header("方块生成")]
    public List<TetrominoLevel> tetrominoLevels;
    public BlockRewardWeights commonBlockRewardWeights;
    public BlockRewardWeights advancedBlockRewardWeights;

    [Header("道具池")]
    public int itemSlotCount = 3;
    public List<ItemData> commonItemPool;
    public List<ItemData> advancedItemPool;

    [Header("条约池")]
    public int maxProtocolCount = 5;
    public List<ProtocolData> protocolPool;

    [Header("奖励刷新功能")]
    [Tooltip("每次胡牌初始的刷新价格")]
    public int refreshBaseCost = 100;
    // ========================================================================
    // 6. UI 视觉配置
    // ========================================================================
    [Header("--- UI 视觉配置 ---")]
    [Tooltip("传奇图标 (用于UI显示)")]
    public Sprite legendaryIcon;

    [Header("浮窗背板")]
    public Sprite tooltipBgCommon;    // 普通道具背板
    public Sprite tooltipBgAdvanced;  // 高级道具背板
    public Sprite tooltipBgProtocol;  // 条约背板
    public Sprite tooltipBgLegendary; // 传奇背板

    [Header("浮窗标签颜色")]
    public Color labelColorCommon = new Color(0.2f, 0.6f, 1f);
    public Color labelColorAdvanced = new Color(1f, 0.5f, 0f);
    public Color labelColorProtocol = new Color(0.5f, 0.5f, 0.5f);

    [Tooltip("消除行时的动画持续时间 (秒)")]
    public float rowClearAnimationDuration = 0.5f;
}