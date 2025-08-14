// FileName: GameManager.cs
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
        CalculateTotalExtraMultiplier();
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

    private void CalculateTotalExtraMultiplier()
    {
        totalExtraMultiplier = 0;
        if (spawner.TetrominoPrefabs == null || spawner.TetrominoPrefabs.Length == 0)
        {
            Debug.LogWarning("Spawner上没有配置任何Tetromino Prefabs，额外倍率将为0。");
            return;
        }

        foreach (var prefab in spawner.TetrominoPrefabs)
        {
            var tetromino = prefab.GetComponent<Tetromino>();
            if (tetromino != null)
            {
                totalExtraMultiplier += tetromino.extraMultiplier;
            }
        }
    }

    public void StartNewGame()
    {
        Time.timeScale = 1f;
        currentFallSpeed = settings.initialFallSpeed;

        blockPool.ResetFullDeck();
        tetrisGrid.ClearAllBlocks();
        huPaiArea.ClearAll();
        scoreManager.ResetScore();
        spawner.StartSpawning(settings);

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

        // BUG修复：消行后不掉落，所以不调用CompactAllColumns

        // 消行处理完毕（且未胡牌），生成下一个方块
        spawner.SpawnBlock();
        isProcessingRows = false;
    }

    private void HandleHuDeclared(List<List<int>> huHand)
    {
        isProcessingRows = true;
        Time.timeScale = 0f;

        // 新的计分逻辑
        int totalFan = mahjongCore.CalculateHandFan(huHand, settings);
        double scorePart = settings.baseFanScore * Mathf.Pow(2, totalFan);
        long finalScore = (long)(scorePart * totalExtraMultiplier);

        scoreManager.AddScore((int)Mathf.Min(finalScore, int.MaxValue));

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

        spawner.StartSpawning(settings);

        isProcessingRows = false;
    }

    private void HandleGameOver()
    {
        Time.timeScale = 0f;
        gameUI.ShowGameOverPanel();
    }
}