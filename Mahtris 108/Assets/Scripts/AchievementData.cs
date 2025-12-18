// FileName: AchievementData.cs
using UnityEngine;

public enum AchievementType
{
    // --- 基础类 ---
    FirstHu,            // No.1
    WinGame,            // No.2-4
    HuFanCount,         // No.5-9
    GameEndSpeed,       // No.10-13
    GameEndTime,        // No.14-17

    // --- 局内数值监控 ---
    StatBaseScore,      // No.18-19
    StatBlockMult,      // No.20-21
    StatExtraMult,      // No.22-23
    MaxBlocksOnBoard,   // No.29

    // --- 局内统计 ---
    SingleGameItemUse,        // No.24
    SingleGameActiveProtocol, // No.25
    SingleGameTotalProtocol,  // No.26
    SingleGameLoopCount,      // No.31-34
    SingleGameScore,          // No.38-42 (修复报错: SingleGameScore)

    // --- 特殊胜利 ---
    WinNoItem,          // No.27
    WinNoProtocol,      // No.28
    WinMinBlocks,       // No.30
    GameEndGold,        // No.18-20 (修复报错: GameEndGold)

    // --- 累计与解锁 ---
    UnlockItemCount,    // No.35-37
    UnlockProtocolCount,// No.38-40
    AccumulateHu,       // No.30-32 (修复报错: AccumulateHu)
    HuPattern,          // No.21-29 (修复报错: HuPattern)
    AccumulateItemUse,  // No.41
    AccumulateProtocolGet, // No.42
    AccumulateLegendary,// No.43 (传奇物品统计)
    AccumulateGold,     // No.44-46
    HighScore,          // No.47-50
}

[CreateAssetMenu(fileName = "New Achievement", menuName = "Mahjong/Achievement Data")]
public class AchievementData : ScriptableObject
{
    public string id;
    public string title;
    [TextArea] public string description;
    public Sprite icon;
    public int rewardGold = 2000;

    [Header("达成条件")]
    public AchievementType type;
    public int targetValue;
    public string targetString;
}