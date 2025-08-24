// FileName: GameSettings.cs
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class TetrominoLevel { public string levelName = "Lv.1"; public int minBlocks = 1; public int maxBlocks = 4; }
[System.Serializable]
public class ScoreLevel { public int targetScore; public int goldReward; }

[CreateAssetMenu(fileName = "GameSettings", menuName = "Game/Game Settings", order = 1)]
public class GameSettings : ScriptableObject
{
    [Header("��Ϸ������߼�")]
    public int gridWidth = 10; public int gridHeight = 20; public int deadlineHeight = 18; public int setsForHu = 4;
    [Range(0f, 0.5f)] public float speedIncreasePerHu = 0.05f;
    public float initialFallSpeed = 1.0f; public float fastFallMultiplier = 20f;
    [Header("�齫�ƿ�")]
    public int tileRanks = 9; public int tilesPerRank = 4;
    public int UniqueTileCount => tileRanks * 3; public int TotalTileCount => UniqueTileCount * tilesPerRank;
    [Header("�÷��뷬��")]
    public int baseFanScore = 50; public int fanBonusPerKong = 1; public int scorePerRow = 10;
    [Header("Ŀ�������ʱ��")]
    public float initialTimeLimit = 180f; public float huTimeBonus = 60f;
    public List<ScoreLevel> scoreLevels;
    [Header("Tetromino �ȼ�")]
    public List<TetrominoLevel> tetrominoLevels;
    [Header("����ϵͳ")]
    public int itemSlotCount = 3; public List<ItemData> masterItemList;
}