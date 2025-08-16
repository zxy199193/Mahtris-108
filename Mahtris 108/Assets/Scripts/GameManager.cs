// FileName: GameManager.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    // (字段声明和Awake/Start/OnEnable/OnDisable/RecalculateTotalMultiplier等方法与上一版相同)
    #region Unchanged Code
    public static GameManager Instance { get; private set; }
    [Header("核心配置")]
    [SerializeField] private GameSettings settings;
    [Header("模块引用")]
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
    #endregion

    public void StartNewGame()
    {
        Time.timeScale = 1f;
        currentFallSpeed = settings.initialFallSpeed;

        blockPool.ResetFullDeck();
        tetrisGrid.ClearAllBlocks();
        huPaiArea.ClearAll();
        scoreManager.ResetScore();

        // --- 【重大修正】---
        // 调用完全重置方法
        spawner.InitializeForNewGame(settings);

        RecalculateTotalMultiplier();
        isProcessingRows = false;
        gameUI.HideGameOverPanel();
        gameUI.HideHuPopup();
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

        // --- 【重大修正】---
        // 调用开始下一轮的方法，这不会重置方块池
        spawner.StartNextRound();

        isProcessingRows = false;
    }

    // (HandleRowsCleared, HandleHuDeclared, OnLevelButtonClicked, HandleGameOver 等方法与上一版相同)
    #region Unchanged Code
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

    private void HandleHuDeclared(List<List<int>> huHand)
    {
        isProcessingRows = true;
        Time.timeScale = 0f;

        var analysisResult = mahjongCore.CalculateHandFan(huHand, settings);
        double scorePart = settings.baseFanScore * Mathf.Pow(2, analysisResult.TotalFan);
        long finalScore = (long)(scorePart * totalExtraMultiplier);

        scoreManager.AddScore((int)Mathf.Min(finalScore, int.MaxValue));

        gameUI.ShowHuPopup(huHand, analysisResult, settings.baseFanScore, totalExtraMultiplier, finalScore);
    }

    public void OnLevelButtonClicked(int levelIndex)
    {
        var chosenPrefab = spawner.AddRandomTetrominoOfLevel(levelIndex);
        if (chosenPrefab != null)
        {
            gameUI.DisplayChosenTetrominoAndLockButtons(chosenPrefab);
        }
    }

    private void HandleGameOver()
    {
        Time.timeScale = 0f;
        gameUI.ShowGameOverPanel();
    }
    #endregion
}

