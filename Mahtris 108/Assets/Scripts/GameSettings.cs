// FileName: GameSettings.cs
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class TetrominoLevel
{
    public string levelName = "Lv.1";
    [Tooltip("�õȼ����������ٷ�����")]
    public int minBlocks = 3;
    [Tooltip("�õȼ���������෽����")]
    public int maxBlocks = 3;
}

[System.Serializable]
public class ScoreLevel
{
    [Tooltip("�õȼ���Ҫ�ﵽ��Ŀ�����")]
    public int targetScore;
    [Tooltip("���Ŀ������Ľ������")]
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
    [Header("��Ϸ������߼�")]
    public int gridWidth = 10;
    public int gridHeight = 20;
    public int deadlineHeight = 18;
    public int setsForHu = 4;
    [Tooltip("ÿ�κ��ƺ��ٶ����ӳ�ʼ�ٶȵİٷֱ�")]
    [Range(0f, 1f)]
    public float speedIncreasePerHu = 0.2f;
    public float initialFallSpeed = 1.0f;
    public float fastFallSpeed = 3f;

    [Header("�齫�ƿ�")]
    public int tileRanks = 9;
    public int tilesPerRank = 4;
    public int UniqueTileCount => tileRanks * 3;
    public int TotalTileCount => UniqueTileCount * tilesPerRank;

    [Header("�÷�ϵͳ")]
    public int baseFanScore = 10;
    public int fanBonusPerKong = 1;
    public int scorePerRow = 10;

    [Header("Ŀ�������ʱ��")]
    public float initialTimeLimit = 180f;
    public float huTimeBonus = 60f;
    public List<ScoreLevel> scoreLevels;

    [Header("Tetromino �ȼ�")]
    public List<TetrominoLevel> tetrominoLevels;

    [Header("���ƽ�������")]
    public BlockRewardWeights commonBlockRewardWeights;
    public BlockRewardWeights advancedBlockRewardWeights;

    [Header("����ϵͳ")]
    public int itemSlotCount = 3;
    public List<ItemData> commonItemPool;
    public List<ItemData> advancedItemPool;

    [Header("��Լϵͳ")]
    public int maxProtocolCount = 5;
    public List<ProtocolData> protocolPool;
}