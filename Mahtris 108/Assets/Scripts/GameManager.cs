// FileName: GameManager.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    // ... (�����ֶκʹ󲿷ַ�������һ����ͬ)
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
    [SerializeField] private InventoryManager inventoryManager;
    private MahjongCore mahjongCore;
    [HideInInspector] public float currentFallSpeed;
    private bool isProcessingRows = false;
    private float totalExtraMultiplier;
    private float remainingTime;
    private int currentScoreLevelIndex;
    private bool isEndlessMode = false;
    private int baseFanScoreBonus = 0;
    private int huCount = 0;
    private float speedPercentageModifier = 0f;
    public Spawner Spawner => spawner;
    public HuPaiArea HuPaiArea => huPaiArea;
    void Awake()
    {
        if (Instance == null) { Instance = this; } else { Destroy(gameObject); }
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
        huCount = 0;
        baseFanScoreBonus = 0;
        speedPercentageModifier = 0f;
        UpdateFallSpeed();
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
        isProcessingRows = true;
        Time.timeScale = 0f;
        scoreManager.IncrementHuCount();
        var analysisResult = mahjongCore.CalculateHandFan(huHand, settings);
        int currentBaseScore = settings.baseFanScore + baseFanScoreBonus;
        double scorePart = currentBaseScore * Mathf.Pow(2, analysisResult.TotalFan);
        long finalScore = (long)(scorePart * totalExtraMultiplier);
        scoreManager.AddScore((int)Mathf.Min(finalScore, int.MaxValue));
        remainingTime += settings.huTimeBonus;
        gameUI.ShowHuPopup(huHand, analysisResult, currentBaseScore, totalExtraMultiplier, finalScore);
    }
    private void OnScoreUpdated(int newScore)
    {
        if (isEndlessMode || settings.scoreLevels == null || settings.scoreLevels.Count == 0) return;
        if (currentScoreLevelIndex < settings.scoreLevels.Count && newScore >= settings.scoreLevels[currentScoreLevelIndex].targetScore)
        {
            if (GameSession.Instance != null)
                GameSession.Instance.AddGold(settings.scoreLevels[currentScoreLevelIndex].goldReward);
            if (AudioManager.Instance != null && AudioManager.Instance.SoundLibraryProperty != null)
                AudioManager.Instance.PlaySFX(AudioManager.Instance.SoundLibraryProperty.targetReached);
            currentScoreLevelIndex++;
            if (currentScoreLevelIndex >= settings.scoreLevels.Count) isEndlessMode = true;
            UpdateTargetScoreUI();
        }
    }
    public void ContinueAfterHu()
    {
        gameUI.HideHuPopup();
        Time.timeScale = 1f;
        huCount = scoreManager.GetHuCount();
        UpdateFallSpeed();
        baseFanScoreBonus = 0;
        blockPool.ResetFullDeck();
        tetrisGrid.ClearAllBlocks();
        huPaiArea.ClearAll();
        spawner.StartNextRound();
        RecalculateTotalMultiplier();
        isProcessingRows = false;
    }
    #endregion

    // --- ���ش�������---
    // �����Ƿ��ҵ��·��飬������UI������������ť
    public void OnLevelButtonClicked(int levelIndex)
    {
        var chosenPrefab = spawner.AddRandomTetrominoOfLevel(levelIndex);

        if (chosenPrefab != null)
        {
            // �ɹ�������·��飬�����������¼����ܱ��ʲ�ˢ��UI�б�
            RecalculateTotalMultiplier();
        }
        else
        {
            // ��ʹû�ҵ��·��飬ҲҪ��ȷ��֪���
            Debug.LogWarning("����� Tetromino ʧ�ܣ����Խ�����������ť��");
        }

        // ���۳ɹ���񣬶����ô˷�������ʾ�����null��prefab����������ť
        gameUI.DisplayChosenTetrominoAndLockButtons(chosenPrefab);
    }

    // (���෽������һ����ȫ��ͬ)
    #region Unchanged Code
    private void UpdateFallSpeed()
    {
        float currentSpeedPercent = 100f + (huCount * (settings.speedIncreasePerHu * 100f)) + speedPercentageModifier;
        currentSpeedPercent = Mathf.Max(1f, currentSpeedPercent);
        currentFallSpeed = settings.initialFallSpeed / (currentSpeedPercent / 100f);
        gameUI.UpdateSpeedText(currentSpeedPercent);
    }
    public void GrantRandomItem()
    {
        if (settings.masterItemList != null && settings.masterItemList.Count > 0)
        {
            var item = settings.masterItemList[Random.Range(0, settings.masterItemList.Count)];
            bool added = inventoryManager.AddItem(item);
            gameUI.SetGrantItemButtonInteractable(false);
        }
    }
    private void UpdateTargetScoreUI()
    {
        if (isEndlessMode) gameUI.UpdateTargetScoreText("�޾�ģʽ");
        else if (settings.scoreLevels != null && currentScoreLevelIndex < settings.scoreLevels.Count)
        {
            var level = settings.scoreLevels[currentScoreLevelIndex];
            gameUI.UpdateTargetScoreText($"{level.targetScore} (����: {level.goldReward}��)");
        }
    }
    private void HandleGameOver() { Time.timeScale = 0f; gameUI.ShowGameOverPanel(); }
    public void AddTime(float time) => remainingTime += time;
    public void ModifySpeedByPercentage(float percentageChange)
    {
        speedPercentageModifier += percentageChange;
        UpdateFallSpeed();
    }
    public void AddBaseScoreBonus(int bonus) => baseFanScoreBonus += bonus;
    #endregion
}

