// FileName: GameManager.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    // (�ֶ�������Awake/Start/OnEnable/OnDisable�ȷ�������һ����ͬ)
    #region Unchanged Code
    public static GameManager Instance { get; private set; }

    [Header("��������")]
    [SerializeField] private GameSettings settings;

    [Header("ģ������")]
    [SerializeField] private Spawner spawner;
    [SerializeField] private TetrisGrid tetrisGrid;
    [SerializeField] private HuPaiArea huPaiArea;
    [SerializeField] private GameUIController gameUI;
    [SerializeField] private BlockPool blockPool;
    [SerializeField] private ScoreManager scoreManager;

    private MahjongCore mahjongCore;
    [HideInInspector] public float currentFallSpeed;
    private bool isProcessingRows = false;
    private float totalExtraMultiplier;

    void Awake()
    {
        if (Instance == null) { Instance = this; } else { Destroy(gameObject); }

        mahjongCore = new MahjongCore();
        tetrisGrid.Initialize(settings);
        blockPool.Initialize(settings);
    }

    void Start()
    {
        StartNewGame();
    }

    void OnEnable()
    {
        GameEvents.OnRowsCleared += HandleRowsCleared;
        GameEvents.OnHuDeclared += HandleHuDeclared;
        GameEvents.OnGameOver += HandleGameOver;
    }

    void OnDisable()
    {
        GameEvents.OnRowsCleared -= HandleRowsCleared;
        GameEvents.OnHuDeclared -= HandleHuDeclared;
        GameEvents.OnGameOver -= HandleGameOver;
    }
    #endregion

    public void RecalculateTotalMultiplier()
    {
        totalExtraMultiplier = 0;
        if (spawner.GetActivePrefabs() == null) return;
        foreach (var prefab in spawner.GetActivePrefabs())
        {
            totalExtraMultiplier += prefab.GetComponent<Tetromino>().extraMultiplier;
        }
        gameUI.UpdateTetrominoList(spawner.GetActivePrefabs(), totalExtraMultiplier);
    }

    public void StartNewGame()
    {
        Time.timeScale = 1f;
        currentFallSpeed = settings.initialFallSpeed;

        blockPool.ResetFullDeck();
        tetrisGrid.ClearAllBlocks();
        huPaiArea.ClearAll();
        scoreManager.ResetScore();

        spawner.InitializeForNewGame(settings);

        RecalculateTotalMultiplier();
        isProcessingRows = false;
        gameUI.HideGameOverPanel();
        gameUI.HideHuPopup();
    }

    private void HandleRowsCleared(List<int> rowIndices)
    {
        if (isProcessingRows) return;
        isProcessingRows = true;

        rowIndices.Sort();

        List<Transform> allClearedTransforms = new List<Transform>();
        List<int> allRemainingIds = new List<int>();

        foreach (var y in rowIndices)
        {
            var rowData = tetrisGrid.GetRowDataAndClear(y);
            allClearedTransforms.AddRange(rowData.transforms);

            scoreManager.AddScore(settings.scorePerRow);
            var result = mahjongCore.DetectSets(rowData.blockIds);

            var setsToAdd = new List<List<int>>();
            setsToAdd.AddRange(result.Kongs);
            setsToAdd.AddRange(result.Pungs);
            setsToAdd.AddRange(result.Chows);

            int needed = settings.setsForHu - huPaiArea.GetSetCount();
            if (setsToAdd.Count > needed)
            {
                var shuffledSets = setsToAdd.OrderBy(a => Random.value).ToList();
                var chosenSets = shuffledSets.Take(needed).ToList();

                var rejectedSets = shuffledSets.Skip(needed).ToList();
                result.RemainingIds.AddRange(rejectedSets.SelectMany(set => set));
                setsToAdd = chosenSets;
            }

            if (setsToAdd.Count > 0) huPaiArea.AddSets(setsToAdd);

            if (huPaiArea.GetSetCount() >= settings.setsForHu)
            {
                var pair = mahjongCore.FindPair(result.RemainingIds);
                if (pair != null)
                {
                    result.RemainingIds.Remove(pair[0]);
                    result.RemainingIds.Remove(pair[1]);

                    var finalHand = huPaiArea.GetAllSets();
                    finalHand.Add(pair);
                    GameEvents.TriggerHuDeclared(finalHand);

                    allRemainingIds.AddRange(result.RemainingIds);
                    blockPool.ReturnBlockIds(allRemainingIds);
                    tetrisGrid.DestroyTransforms(allClearedTransforms);
                    return;
                }
            }
            allRemainingIds.AddRange(result.RemainingIds);
        }

        blockPool.ReturnBlockIds(allRemainingIds);
        tetrisGrid.DestroyTransforms(allClearedTransforms);

        tetrisGrid.CompactAllColumns(rowIndices);

        spawner.SpawnBlock();
        isProcessingRows = false;
    }

    // --- ���ش�������---
    // ������߼����򻯣�ȷ��ֻ���мƷֺ���ʾ�������������κλ�ˢ�·����б���߼�
    private void HandleHuDeclared(List<List<int>> huHand)
    {
        isProcessingRows = true;
        Time.timeScale = 0f;

        // 1. ���㷬��
        var analysisResult = mahjongCore.CalculateHandFan(huHand, settings);

        // 2. �������յ÷�
        double scorePart = settings.baseFanScore * Mathf.Pow(2, analysisResult.TotalFan);
        long finalScore = (long)(scorePart * totalExtraMultiplier);

        // 3. ���ӷ���
        scoreManager.AddScore((int)Mathf.Min(finalScore, int.MaxValue));

        // 4. ��ʾ������ֻ������Ҫ��ʾ�����ݣ�����������������
        gameUI.ShowHuPopup(huHand, analysisResult, settings.baseFanScore, totalExtraMultiplier, finalScore);
    }

    public void OnLevelButtonClicked(int levelIndex)
    {
        var chosenPrefab = spawner.AddRandomTetrominoOfLevel(levelIndex);
        if (chosenPrefab != null)
        {
            // ע�⣺RecalculateTotalMultiplier ���Զ����� UpdateTetrominoList
            // �����������ȷ�ģ���Ϊ����������ҡ�ѡ��֮�󡿣���Ҫˢ���б�
            // RecalculateTotalMultiplier(); // ������ñ��ƶ����� AddRandomTetrominoOfLevel �ڲ�
            gameUI.DisplayChosenTetrominoAndLockButtons(chosenPrefab);
        }
    }

    public void ContinueAfterHu()
    {
        gameUI.HideHuPopup();
        Time.timeScale = 1f;

        currentFallSpeed *= (1f - settings.speedIncreasePerHu);
        currentFallSpeed = Mathf.Max(0.05f, currentFallSpeed);

        blockPool.ResetFullDeck();
        tetrisGrid.ClearAllBlocks();
        huPaiArea.ClearAll();

        spawner.StartNextRound();

        // ����һ�ֿ�ʼǰ�����ʿ����Ѿ��仯���������������¼��㲢ˢ���б�����ȷ��
        RecalculateTotalMultiplier();

        isProcessingRows = false;
    }

    private void HandleGameOver()
    {
        Time.timeScale = 0f;
        gameUI.ShowGameOverPanel();
    }
}