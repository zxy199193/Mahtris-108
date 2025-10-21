// FileName: GameManager.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
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

    [Header("Test Mode")]
    [Tooltip("������, �������Ѷ�ѡ��, ��ʹ��Spawner�е�'Initial Tetromino Prefabs'�б�ʼ��Ϸ��")]
    [SerializeField] private bool isTestMode = false;

    // ��Ϸ״̬����
    private float remainingTime;
    private int currentScoreLevelIndex;
    private bool isEndlessMode = false;
    private List<ProtocolData> activeProtocols = new List<ProtocolData>();

    // �ᱻ��Լ�͵���Ӱ��Ķ�̬����
    private float blockMultiplier;
    private float extraMultiplier;
    private int baseFanScore;

    // �����������̿��Ƶ��ڲ�����
    private bool _isBombOrSpecialClear = false;
    private float protocolSpeedModifier = 1.0f; // ����������Լ�����������ٶȱ���
    public Spawner Spawner => spawner;
    public HuPaiArea HuPaiArea => huPaiArea;

    [Header("��ͣ����")]
    private bool isPaused = false;
    private int remainingPauses;
    [SerializeField] private int maxPauses = 2;

    private bool isStopwatchActive = false; // ��������
    private bool isBountyActive = false;

    private GameSessionConfig currentSessionConfig; // �����������е�ǰ��Ϸ�Ự������
    void Awake()
    {
        if (Instance == null) { Instance = this; } else { Destroy(gameObject); }

        mahjongCore = new MahjongCore();
        tetrisGrid.Initialize(settings);
        blockPool.Initialize(settings);
        inventoryManager.Initialize(settings, this);
        if(tetrisGrid != null && spawner != null)
        {
            tetrisGrid.RegisterSpawner(spawner);
        }
    }

    void Start() { StartNewGame(); }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
        // ԭ�е���ͣ�ж��߼�
        if (isPaused || isProcessingRows || Time.timeScale == 0f) return;
        if (isProcessingRows || Time.timeScale == 0f) return;
        remainingTime -= Time.deltaTime;
        gameUI.UpdateTimerText(remainingTime);
        if (remainingTime <= 0) GameEvents.TriggerGameOver();
    }

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

    public void StartNewGame()
    {
        // === ��1����: ��������䵱ǰ��Ϸ�Ự������ ===
        currentSessionConfig = new GameSessionConfig();
        Difficulty difficulty = DifficultyManager.Instance.CurrentDifficulty;

        // �����Ѷȳ���
        float scoreMultiplier = 1f;
        float speedMultiplier = 1f;

        // ������������ģʽ��顿
        if (isTestMode)
        {
            Debug.LogWarning("--- ����ģʽ�ѿ��� ---");
            // 1. ʹ��Spawner��Ĭ���б�
            currentSessionConfig.InitialTetrominoes = new List<GameObject>(spawner.GetInitialTetrominoPrefabs());

            // 2. ʹ�á���ͨ���Ѷȵĳ���
            scoreMultiplier = 2f;
            speedMultiplier = 1.5f;
        }
        else // �������Ѷ��߼���
        {
            // 1. �����ѶȾ�����ʼ����
            switch (difficulty)
            {
                case Difficulty.Easy:
                    currentSessionConfig.InitialTetrominoes = spawner.GetMasterList().Where(p => IsInLevel(p, 0)).ToList();
                    scoreMultiplier = 1f;
                    speedMultiplier = 1f;
                    break;
                case Difficulty.Hard:
                    var hardInitial = spawner.GetMasterList().Where(p => IsInLevel(p, 1)).ToList();
                    var level3Blocks = spawner.GetMasterList().Where(p => IsInLevel(p, 2)).OrderBy(x => Random.value).Take(2).ToList();
                    hardInitial.AddRange(level3Blocks);
                    currentSessionConfig.InitialTetrominoes = hardInitial;
                    scoreMultiplier = 4f;
                    speedMultiplier = 2f;
                    break;
                case Difficulty.Normal:
                default:
                    currentSessionConfig.InitialTetrominoes = spawner.GetMasterList().Where(p => IsInLevel(p, 1)).ToList();
                    scoreMultiplier = 2f;
                    speedMultiplier = 1.5f;
                    break;
            }
        }

        // 2. Ӧ���ٶ����� (�˲��ֲ���)
        currentSessionConfig.InitialFallSpeed = settings.initialFallSpeed / speedMultiplier;

        // 3. ����Ŀ���������ʱ���� (�˲��ֲ���)
        foreach (var levelTemplate in settings.scoreLevels)
        {
            currentSessionConfig.DifficultyScoreLevels.Add(new ScoreLevel
            {
                targetScore = (int)(levelTemplate.targetScore * scoreMultiplier),
                goldReward = (int)(levelTemplate.goldReward * scoreMultiplier)
            });
        }

        // === ��2����: ʹ��������������Ϸ״̬ ===
        Time.timeScale = 1f;
        isPaused = false;
        // ... (ʡ����������״̬�����ô���, ���Ǳ��ֲ���)
        foreach (var protocol in activeProtocols) { if (protocol != null) protocol.RemoveEffect(this); }
        activeProtocols.Clear();
        protocolSpeedModifier = 1.0f;
        baseFanScore = settings.baseFanScore;
        extraMultiplier = 1f;
        remainingPauses = maxPauses;
        if (gameUI != null) gameUI.UpdatePauseUI(isPaused, remainingPauses);

        remainingTime = settings.initialTimeLimit;
        currentScoreLevelIndex = 0;
        isEndlessMode = false;

        blockPool.ResetFullDeck();
        tetrisGrid.ClearAllBlocks();
        huPaiArea.ClearAll();
        scoreManager.ResetScore();
        inventoryManager.ClearInventory();
        UpdateFallSpeed(); // UpdateFallSpeed���ڻ�ʹ��currentSessionConfig.InitialFallSpeed
        // ʹ�ûỰ�����е���������ʼ��
        spawner.InitializeForNewGame(settings, currentSessionConfig.InitialTetrominoes);

        UpdateActiveBlockListUI();
        isProcessingRows = false;
        gameUI.HideAllPanels();
        UpdateTargetScoreUI();
        gameUI.UpdateBaseScoreText(baseFanScore);
        gameUI.UpdateExtraMultiplierText(extraMultiplier);
    }

    private void HandleHuDeclared(List<List<int>> huHand)
    {
        isProcessingRows = true;
        Time.timeScale = 0f;

        bool isAdvancedReward = scoreManager.IncrementHuCountAndCheckCycle();

        var analysisResult = mahjongCore.CalculateHandFan(huHand, settings);
        double scorePart = baseFanScore * analysisResult.FanMultiplier;
        long finalScore = (long)(scorePart * blockMultiplier * extraMultiplier);
        scoreManager.AddScore((int)Mathf.Min(finalScore, int.MaxValue));
        remainingTime += settings.huTimeBonus;

        var rewards = GenerateHuRewards(isAdvancedReward);
        gameUI.ShowHuPopup(huHand, analysisResult, baseFanScore, blockMultiplier, extraMultiplier, finalScore, rewards, isAdvancedReward);
    }

    public void ContinueAfterHu()
    {
        // �����������ͣ��Ч�������������ͣ����
        if (isStopwatchActive)
        {
            remainingPauses = maxPauses;
            isStopwatchActive = false;
        }
        else // ���ͣ��û�������������
        {
            remainingPauses = maxPauses;
        }

        if (gameUI != null) gameUI.UpdatePauseUI(isPaused, remainingPauses);
        remainingPauses = maxPauses;
        if (gameUI != null) gameUI.UpdatePauseUI(isPaused, remainingPauses);
        gameUI.HideHuPopup();
        Time.timeScale = 1f;
        UpdateFallSpeed();
        blockPool.ResetFullDeck();
        tetrisGrid.ClearAllBlocks();
        huPaiArea.ClearAll();
        spawner.StartNextRound();
        isProcessingRows = false;
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

        if (!_isBombOrSpecialClear)
        {
            spawner.SpawnBlock();
        }

        _isBombOrSpecialClear = false;
        isProcessingRows = false;
    }

    // --- ���������ߺ���Լ���õĽӿ� ---
    public void AddTime(float time) => remainingTime += time;

    public void ModifyBaseFanScore(int amount, bool isMultiplier)
    {
        if (isMultiplier) baseFanScore *= amount;
        else baseFanScore += amount;
        gameUI.UpdateBaseScoreText(baseFanScore);
    }

    public void ModifyTargetScore(float multiplier)
    {
        if (isEndlessMode) return;
        var level = settings.scoreLevels[currentScoreLevelIndex];
        level.targetScore = (int)(level.targetScore * multiplier);
        UpdateTargetScoreUI();
    }

    public void AddProtocol(ProtocolData protocol)
    {
        if (activeProtocols.Count < settings.maxProtocolCount && !activeProtocols.Contains(protocol))
        {
            activeProtocols.Add(protocol);
            protocol.ApplyEffect(this);
            // ���������ﴥ��һ�� OnProtocolsChanged �¼�����UI����
            gameUI.UpdateProtocolUI(activeProtocols);
        }
    }

    public void RecalculateBlockMultiplier()
    {
        blockMultiplier = 0;
        if (spawner.GetActivePrefabs() == null) return;
        foreach (var prefab in spawner.GetActivePrefabs())
            blockMultiplier += prefab.GetComponent<Tetromino>().extraMultiplier;

        if (blockMultiplier < 1f) blockMultiplier = 1f;

        gameUI.UpdateBlockMultiplierText(blockMultiplier);
    }
    public void UpdateActiveBlockListUI()
    {
        // 1. �������еķ����������ܱ��ʲ����±����ı�
        // RecalculateBlockMultiplier() ����㲢���� this.blockMultiplier �ֶ�
        RecalculateBlockMultiplier();

        // 2. �� Spawner ��ȡ��ǰ�ķ����
        var prefabs = spawner.GetActivePrefabs();

        // 3. ��ȡ�ռ�������ܱ���
        float totalMultiplier = this.blockMultiplier;

        // 4. ���� GameUIController �����·����б��UI��ʾ
        if (gameUI != null)
        {
            gameUI.UpdateTetrominoList(prefabs, totalMultiplier);
        }
    }
    public void ApplyBlockMultiplierModifier(float amount)
    {
        blockMultiplier += amount;
        if (blockMultiplier < 1f) blockMultiplier = 1f;
        gameUI.UpdateBlockMultiplierText(blockMultiplier);
    }

    public void ApplyExtraMultiplier(float factor)
    {
        extraMultiplier *= factor;
        gameUI.UpdateExtraMultiplierText(extraMultiplier);
    }

    public void ForceClearRowsFromBottom(int count)
    {
        _isBombOrSpecialClear = true;
        tetrisGrid.ForceClearBottomRows(count);
    }
    public void ApplyPermanentSpeedModifier(float multiplier)
    {
        protocolSpeedModifier *= multiplier;

        // �������¼���ȫ���ٶ�
        UpdateFallSpeed();

        // ���������ٶ�Ӧ�õ���ǰ���飬ʵ�ּ�ʱЧ��
        var currentTetromino = FindObjectOfType<Tetromino>();
        if (currentTetromino != null)
        {
            currentTetromino.UpdateFallSpeedNow(this.currentFallSpeed);
        }
    }

    public void ModifySpeedOfCurrentTetrominoByPercent(float percentage)
    {
        var currentTetromino = FindObjectOfType<Tetromino>();
        if (currentTetromino != null)
        {
            // 1. ��ȡ��ǰ���ٶȣ�ʵ�����ӳ�ʱ�䣩
            float currentDelay = currentTetromino.GetCurrentFallSpeed();

            // 2. ���ݰٷֱȼ����µ��ӳ�ʱ��
            // ��ʽ: NewDelay = CurrentDelay / (1 + PercentageChange / 100)
            float newDelay = currentDelay / (1f + percentage / 100f);
            if (settings != null)
            {
                float newSpeedPercent = (settings.initialFallSpeed / newDelay) * 100f;

                // 2. ����UI�ı�
                gameUI.UpdateSpeedText(newSpeedPercent);
            }
            // 3. ������������ٶ�Ӧ�õ�������
            currentTetromino.UpdateFallSpeedNow(newDelay);
        }
    }

    // --- ˽�и������� ---
    private HuRewardPackage GenerateHuRewards(bool isAdvanced)
    {
        var package = new HuRewardPackage();
        if (isAdvanced)
        {
            package.BlockChoices = GetWeightedRandomBlocks(5, settings.advancedBlockRewardWeights).ToList();
            package.ItemChoices = settings.advancedItemPool.OrderBy(x => Random.value).Take(2).ToList();
            package.ProtocolChoices = settings.protocolPool.Except(activeProtocols).OrderBy(x => Random.value).Take(2).ToList();
        }
        else
        {
            package.BlockChoices = GetWeightedRandomBlocks(3, settings.commonBlockRewardWeights).ToList();
            package.ItemChoices = settings.commonItemPool.OrderBy(x => Random.value).Take(3).ToList();
        }
        return package;
    }

    private IEnumerable<GameObject> GetWeightedRandomBlocks(int count, BlockRewardWeights weights)
    {
        var source = spawner.GetMasterList();
        var level1 = source.Where(p => IsInLevel(p, 0)).ToList();
        var level2 = source.Where(p => IsInLevel(p, 1)).ToList();
        var level3 = source.Where(p => IsInLevel(p, 2)).ToList();
        var result = new List<GameObject>();
        for (int i = 0; i < count; i++)
        {
            GameObject chosenBlock = null;
            float roll = Random.value;
            if (roll < weights.level1Weight && level1.Count > 0)
                chosenBlock = level1[Random.Range(0, level1.Count)];
            else if (roll < weights.level1Weight + weights.level2Weight && level2.Count > 0)
                chosenBlock = level2[Random.Range(0, level2.Count)];
            else if (level3.Count > 0)
                chosenBlock = level3[Random.Range(0, level3.Count)];
            else if (level2.Count > 0)
                chosenBlock = level2[Random.Range(0, level2.Count)];
            else if (level1.Count > 0)
                chosenBlock = level1[Random.Range(0, level1.Count)];
            if (chosenBlock != null && !result.Contains(chosenBlock))
                result.Add(chosenBlock);
        }
        return result;
    }

    private bool IsInLevel(GameObject prefab, int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= settings.tetrominoLevels.Count) return false;
        var levelDef = settings.tetrominoLevels[levelIndex];
        int count = prefab.GetComponentsInChildren<BlockUnit>().Length;
        return count >= levelDef.minBlocks && count <= levelDef.maxBlocks;
    }

    private void UpdateFallSpeed()
    {
        float speedPercent = 100f + (scoreManager.GetHuCount() * (settings.speedIncreasePerHu * 100f));

        // ���޸ġ�ʹ�� currentSessionConfig �еĳ�ʼ�ٶ�
        float baseFallSpeed = currentSessionConfig.InitialFallSpeed / protocolSpeedModifier;

        currentFallSpeed = baseFallSpeed / (speedPercent / 100f);
        gameUI.UpdateSpeedText(speedPercent * protocolSpeedModifier);
    }

    private void OnScoreUpdated(int newScore)
    {
        // ���޸ġ�ʹ�� currentSessionConfig����������Ƿ�Ϊnull
        if (isEndlessMode || currentSessionConfig == null || currentSessionConfig.DifficultyScoreLevels.Count == 0) return;

        // ���޸ġ�ʹ�� currentSessionConfig
        while (currentScoreLevelIndex < currentSessionConfig.DifficultyScoreLevels.Count && newScore >= currentSessionConfig.DifficultyScoreLevels[currentScoreLevelIndex].targetScore)
        {
            if (GameSession.Instance != null)
            {
                // ���޸ġ�ʹ�� currentSessionConfig
                GameSession.Instance.AddGold(currentSessionConfig.DifficultyScoreLevels[currentScoreLevelIndex].goldReward);
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.SoundLibrary.targetReached);
            }

            currentScoreLevelIndex++;

            // ���޸ġ�ʹ�� currentSessionConfig
            if (currentScoreLevelIndex >= currentSessionConfig.DifficultyScoreLevels.Count)
            {
                HandleGameWon();
            }

            UpdateTargetScoreUI();
        }

        // (�������ԡ������������ܣ����ֲ���)
        gameUI.UpdateScoreProgress(newScore);
    }

    private void UpdateTargetScoreUI()
    {
        if (isEndlessMode)
        {
            gameUI.UpdateTargetScoreDisplay("�޾�ģʽ");
        }
        // ���޸ġ�ʹ�� currentSessionConfig
        else if (currentScoreLevelIndex < currentSessionConfig.DifficultyScoreLevels.Count)
        {
            // ���޸ġ�ʹ�� currentSessionConfig
            var level = currentSessionConfig.DifficultyScoreLevels[currentScoreLevelIndex];
            gameUI.UpdateTargetScoreDisplay(level.targetScore, level.goldReward);

            // (�������ԡ������������ܣ����ֲ���)
            gameUI.UpdateScoreProgress(scoreManager.GetCurrentScore());
        }
    }

    private void HandleGameOver()
    {
        Time.timeScale = 0f;
        int finalScore = scoreManager.GetCurrentScore();
        bool isNewHighScore = scoreManager.CheckForNewHighScore(finalScore); // ��Ҫ��ScoreManager��ʵ�ִ˷���
        gameUI.ShowGameEndPanel(false, finalScore, isNewHighScore);
    }

    public void TogglePause()
    {
        // �����Ϸ�ѽ�������������ͣ
        if (Time.timeScale == 0f && !isPaused) return;

        if (isPaused)
        {
            // --- ȡ����ͣ ---
            isPaused = false;
            Time.timeScale = 1f;
            gameUI.ShowPausePanel(false);
        }
        else
        {
            // --- ������ͣ ---
            if (remainingPauses > 0)
            {
                isPaused = true;
                remainingPauses--;
                Time.timeScale = 0f;
                gameUI.ShowPausePanel(true);
            }
            else
            {
                Debug.Log("��ͣ����������!");
                // (��ѡ) �����ڴ˴���һ��UI��ʾ
            }
        }
        gameUI.UpdatePauseUI(isPaused, remainingPauses);

    }
    public void AddPauseCount(int amount)
    {
        remainingPauses += amount;
        gameUI.UpdatePauseUI(isPaused, remainingPauses);
    }
    private void HandleGameWon()
    {
        Time.timeScale = 0f;
        int finalScore = scoreManager.GetCurrentScore();
        bool isNewHighScore = scoreManager.CheckForNewHighScore(finalScore);
        gameUI.ShowGameEndPanel(true, finalScore, isNewHighScore);
    }

    public void StartEndlessMode()
    {
        isEndlessMode = true;
        Time.timeScale = 1f;
        gameUI.HideAllPanels(); // ȷ��������屻����
        UpdateTargetScoreUI();
    }
    public void SetStopwatchActive(bool isActive)
    {
        isStopwatchActive = isActive;
    }
    // ����±���
    private ItemData lastUsedItem = null;

    // ����µĹ������ԣ��Ա���߽ű����Է��� InventoryManager
    public InventoryManager Inventory => inventoryManager;

    // ����·���
    public void SetLastUsedItem(ItemData item)
    {
        // ����¼����������
        if (!(item is ReplicatorMk2Item))
        {
            lastUsedItem = item;
        }
    }
    public ItemData GetLastUsedItem()
    {
        return lastUsedItem;
    }
    public void ActivateBounty()
    {
        if (!isBountyActive)
        {
            isBountyActive = true;
            // ��������UI������ҿ�������������
            UpdateTargetScoreUI();
        }
    }
}