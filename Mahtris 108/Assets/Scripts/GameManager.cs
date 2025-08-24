// FileName: GameManager.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [Header("核心配置")][SerializeField] private GameSettings settings;
    [Header("模块引用")]
    [SerializeField] private Spawner spawner;
    [SerializeField] private TetrisGrid tetrisGrid;
    [SerializeField] private HuPaiArea huPaiArea;
    [SerializeField] private GameUIController gameUI;
    [SerializeField] private BlockPool blockPool;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private InventoryManager inventoryManager;

    private MahjongCore mahjongCore;
    [HideInInspector] public float currentFallSpeed;
    private bool isProcessingRows = false; private float totalExtraMultiplier;
    private float remainingTime; private int currentScoreLevelIndex; private bool isEndlessMode = false;

    // 公开属性，供道具访问
    public Spawner Spawner => spawner;
    public HuPaiArea HuPaiArea => huPaiArea;

    void Awake()
    {
        Instance = this;
        mahjongCore = new MahjongCore();
        tetrisGrid.Initialize(settings);
        blockPool.Initialize(settings);
        inventoryManager.Initialize(settings, this);
    }
    void Start() { StartNewGame(); }
    void OnEnable()
    {
        GameEvents.OnRowsCleared += HandleRowsCleared;
        GameEvents.OnHuDeclared += HandleHuDeclared;
        GameEvents.OnGameOver += HandleGameOver;
        ScoreManager.OnScoreChanged += OnScoreUpdated;
    }
    void OnDisable()
    {
        GameEvents.OnRowsCleared -= HandleRowsCleared;
        GameEvents.OnHuDeclared -= HandleHuDeclared;
        GameEvents.OnGameOver -= HandleGameOver;
        ScoreManager.OnScoreChanged -= OnScoreUpdated;
    }
    void Update()
    {
        if (isProcessingRows || Time.timeScale == 0f) return;
        remainingTime -= Time.deltaTime;
        gameUI.UpdateTimerText(remainingTime);
        if (remainingTime <= 0) GameEvents.TriggerGameOver();
    }
    public void RecalculateTotalMultiplier()
    {
        totalExtraMultiplier = 0;
        if (spawner.GetActivePrefabs() == null) return;
        foreach (var prefab in spawner.GetActivePrefabs())
            totalExtraMultiplier += prefab.GetComponent<Tetromino>().extraMultiplier;
        gameUI.UpdateTetrominoList(spawner.GetActivePrefabs(), totalExtraMultiplier);
    }
    public void StartNewGame()
    {
        Time.timeScale = 1f;
        currentFallSpeed = settings.initialFallSpeed;
        remainingTime = settings.initialTimeLimit;
        currentScoreLevelIndex = 0;
        isEndlessMode = false;
        blockPool.ResetFullDeck();
        tetrisGrid.ClearAllBlocks();
        huPaiArea.ClearAll();
        scoreManager.ResetScore();
        inventoryManager.ClearInventory();
        spawner.InitializeForNewGame(settings);
        RecalculateTotalMultiplier();
        isProcessingRows = false;
        gameUI.HideAllPanels();
        UpdateTargetScoreUI();
        gameUI.UpdateSpeedText(100f);
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
            setsToAdd.AddRange(result.Kongs); setsToAdd.AddRange(result.Pungs); setsToAdd.AddRange(result.Chows);
            int needed = settings.setsForHu - huPaiArea.GetSetCount();
            if (setsToAdd.Count > needed)
            {
                var shuffledSets = setsToAdd.OrderBy(a => Random.value).ToList();
                var chosenSets = shuffledSets.Take(needed).ToList();
                result.RemainingIds.AddRange(shuffledSets.Skip(needed).SelectMany(set => set));
                setsToAdd = chosenSets;
            }
            if (setsToAdd.Count > 0) huPaiArea.AddSets(setsToAdd);
            if (huPaiArea.GetSetCount() >= settings.setsForHu)
            {
                var pair = mahjongCore.FindPair(result.RemainingIds);
                if (pair != null)
                {
                    result.RemainingIds.Remove(pair[0]); result.RemainingIds.Remove(pair[1]);
                    var finalHand = huPaiArea.GetAllSets(); finalHand.Add(pair);
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
        isProcessingRows = true; Time.timeScale = 0f;
        var analysisResult = mahjongCore.CalculateHandFan(huHand, settings);
        double scorePart = settings.baseFanScore * Mathf.Pow(2, analysisResult.TotalFan);
        long finalScore = (long)(scorePart * totalExtraMultiplier);
        scoreManager.AddScore((int)Mathf.Min(finalScore, int.MaxValue));
        remainingTime += settings.huTimeBonus;
        gameUI.ShowHuPopup(huHand, analysisResult, settings.baseFanScore, totalExtraMultiplier, finalScore);
    }
    private void OnScoreUpdated(int newScore)
    {
        if (isEndlessMode || settings.scoreLevels == null || settings.scoreLevels.Count == 0) return;
        if (currentScoreLevelIndex < settings.scoreLevels.Count && newScore >= settings.scoreLevels[currentScoreLevelIndex].targetScore)
        {
            if (GameSession.Instance != null)
                GameSession.Instance.AddGold(settings.scoreLevels[currentScoreLevelIndex].goldReward);
            if (AudioManager.Instance != null && AudioManager.Instance.SoundLib != null)
                AudioManager.Instance.PlaySFX(AudioManager.Instance.SoundLib.targetReached);
            currentScoreLevelIndex++;
            if (currentScoreLevelIndex >= settings.scoreLevels.Count) isEndlessMode = true;
            UpdateTargetScoreUI();
        }
    }
    public void GrantRandomItem()
    {
        if (settings.masterItemList != null && settings.masterItemList.Count > 0)
        {
            var item = settings.masterItemList[Random.Range(0, settings.masterItemList.Count)];
            bool added = inventoryManager.AddItem(item);
            gameUI.SetGrantItemButtonInteractable(added);
        }
    }
    private void UpdateTargetScoreUI()
    {
        if (isEndlessMode) gameUI.UpdateTargetScoreText("无尽模式");
        else if (settings.scoreLevels != null && currentScoreLevelIndex < settings.scoreLevels.Count)
        {
            var level = settings.scoreLevels[currentScoreLevelIndex];
            gameUI.UpdateTargetScoreText($"{level.targetScore} (奖励: {level.goldReward}金)");
        }
    }
    public void OnLevelButtonClicked(int levelIndex)
    {
        var chosenPrefab = spawner.AddRandomTetrominoOfLevel(levelIndex);
        if (chosenPrefab != null) gameUI.DisplayChosenTetrominoAndLockButtons(chosenPrefab);
    }
    public void ContinueAfterHu()
    {
        gameUI.HideHuPopup();
        Time.timeScale = 1f;
        currentFallSpeed *= (1f - settings.speedIncreasePerHu);
        currentFallSpeed = Mathf.Max(0.05f, currentFallSpeed);
        float speedPercent = (settings.initialFallSpeed / currentFallSpeed) * 100f;
        gameUI.UpdateSpeedText(speedPercent);
        blockPool.ResetFullDeck();
        tetrisGrid.ClearAllBlocks();
        huPaiArea.ClearAll();
        spawner.StartNextRound();
        RecalculateTotalMultiplier();
        isProcessingRows = false;
    }
    private void HandleGameOver() { Time.timeScale = 0f; gameUI.ShowGameOverPanel(); }
    public void AddTime(float time) => remainingTime += time;
    public void ModifyFallSpeed(float multiplier) => currentFallSpeed *= multiplier;
    public void AddBaseScoreBonus(int bonus) { Debug.Log($"基础分临时增加了 {bonus}"); }
}
