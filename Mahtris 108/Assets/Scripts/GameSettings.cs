// FileName: GameSettings.cs
using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Game/Game Settings", order = 1)]
public class GameSettings : ScriptableObject
{
    [Header("��Ϸ�������")]
    [Tooltip("��Ϸ����Ŀ�ȣ���������")]
    public int gridWidth = 10;
    [Tooltip("��Ϸ����ĸ߶ȣ���������")]
    public int gridHeight = 20;

    [Header("�齫�ƿ�����")]
    [Tooltip("Ͳ������ÿ���Ƶ�������1-9��")]
    public int tileRanks = 9;
    [Tooltip("ÿ���Ƶ��ظ�����")]
    public int tilesPerRank = 4;

    // �������ԣ�ȷ��������
    public int UniqueTileCount => tileRanks * 3;
    public int TotalTileCount => UniqueTileCount * tilesPerRank;

    [Header("��Ϸ�߼�����")]
    [Tooltip("������Ҫ������������������/˳��/���ӣ�")]
    public int setsForHu = 4;
    [Tooltip("ÿ�κ��ƺ󣬷����½��ٶ����ӵİٷֱȣ�����0.05����5%��")]
    [Range(0f, 0.5f)]
    public float speedIncreasePerHu = 0.05f;
    [Tooltip("��ʼ�����ٶȣ���/��")]
    public float initialFallSpeed = 1.0f;
    [Tooltip("�����������ʱ���ٶȱ���")]
    public float fastFallMultiplier = 20f;

    [Header("�÷�����")]
    [Tooltip("����ʱ�Ļ����÷�")]
    public int baseHuScore = 100;
    [Tooltip("ÿ��һ�����ʱ�����ƵĶ���ӷ�")]
    public int scoreBonusPerKong = 100;
    [Tooltip("ÿ����һ�л�õĻ�����")]
    public int scorePerRow = 10;
}
