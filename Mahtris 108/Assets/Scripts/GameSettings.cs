// FileName: GameSettings.cs
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class TetrominoLevel
{
    public string levelName = "Lv.1";
    [Tooltip("�õȼ����������ٷ�����")]
    public int minBlocks = 1;
    [Tooltip("�õȼ���������෽����")]
    public int maxBlocks = 4;
}

[CreateAssetMenu(fileName = "GameSettings", menuName = "Game/Game Settings", order = 1)]
public class GameSettings : ScriptableObject
{
    [Header("��Ϸ�������")]
    public int gridWidth = 10;
    public int gridHeight = 20;
    // ---�����µ㡿---
    // �Ƴ��� [Range(15, 20)] ���ƣ����ڿ������ⶨ��
    [Tooltip("��Ϸʧ�ܵ������߸߶ȣ�����Ӵ���Խ���˸߶�����Ϸ����")]
    public int deadlineHeight = 18;

    [Header("�齫�ƿ�����")]
    public int tileRanks = 9;
    public int tilesPerRank = 4;
    public int UniqueTileCount => tileRanks * 3;
    public int TotalTileCount => UniqueTileCount * tilesPerRank;

    [Header("��Ϸ�߼�����")]
    public int setsForHu = 4;
    [Range(0f, 0.5f)]
    public float speedIncreasePerHu = 0.05f;
    public float initialFallSpeed = 1.0f;
    public float fastFallMultiplier = 20f;

    [Header("�÷�����")]
    [Tooltip("�Ĵ��齫�Ʒ��㷨�ġ������֡�")]
    public int baseFanScore = 50;
    [Tooltip("ÿ��һ�����ʱ���������ӵġ�������")]
    public int fanBonusPerKong = 1;
    public int scorePerRow = 10;

    [Header("Tetromino �ȼ�����")]
    public List<TetrominoLevel> tetrominoLevels;
}