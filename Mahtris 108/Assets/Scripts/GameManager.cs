using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
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

    void Awake()
    {
        if (Instance == null) { Instance = this; } else { Destroy(gameObject); }

        mahjongCore = new MahjongCore();
        tetrisGrid.Initialize(settings);
        blockPool.Initialize(settings);
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

    void Start()
    {
        StartNewGame();
    }

    public void StartNewGame()
    {
        Time.timeScale = 1f;
        currentFallSpeed = settings.initialFallSpeed;

        blockPool.ResetFullDeck();
        tetrisGrid.ClearAllBlocks();
        huPaiArea.ClearAll();
        scoreManager.ResetScore();

        // ---【修正点】---
        // 正确调用了 Spawner.StartSpawning
        spawner.StartSpawning(settings);

        isProcessingRows = false;

        // ---【修正点】---
        // 正确调用了 GameUIController 的方法
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

            if (setsToAdd.Count > 0)
            {
                huPaiArea.AddSets(setsToAdd);
            }

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

        tetrisGrid.CompactAllColumns();
        spawner.SpawnBlock();
        isProcessingRows = false;
    }

    private void HandleHuDeclared(List<List<int>> huHand)
    {
        isProcessingRows = true;
        Time.timeScale = 0f;

        int kongsCount = huHand.Count(set => set.Count == 4);
        int totalScore = settings.baseHuScore + (kongsCount * settings.scoreBonusPerKong);
        scoreManager.AddScore(totalScore);

        gameUI.ShowHuPopup(huHand);
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

        // ---【修正点】---
        // 正确调用了 Spawner.StartSpawning
        spawner.StartSpawning(settings);

        isProcessingRows = false;
    }

    private void HandleGameOver()
    {
        Time.timeScale = 0f;
        gameUI.ShowGameOverPanel();
    }
}
