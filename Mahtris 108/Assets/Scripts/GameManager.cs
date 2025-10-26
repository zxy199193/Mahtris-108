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

    // �������������� V4.1 ϵͳ
    private int permanentBaseScoreBonus = 0; // "��֭" (+3)
    private int roundBaseScoreBonus = 0; // "��������" (+8) �� "��̴�" (+16 / -16)
    private float permanentBaseScoreMultiplier = 1f; // ��������"�ɾ�" (x2)
    private bool isSteroidActive = false; // ���� "��̴�" (+16) Ч���Ƿ񼤻�
    private bool isSteroidReversalActive = false; // ���� "��̴�" (-16) Ч���Ƿ񼤻�

    // �����������̿��Ƶ��ڲ�����
    private bool _isBombOrSpecialClear = false;

    private int permanentSpeedBonus = 0; // ���üӳ� (����, ��֮�ȼ�, ��Լ)
    private int roundSpeedBonus = 0; // ���ּӳ� (����ɡ)
    private int countedSpeedBonus = 0; // �����ӳ� (��������)
    private int countedBonusBlocksRemaining = 0; // ��������ʣ�෽����
    public Spawner Spawner => spawner;
    public HuPaiArea HuPaiArea => huPaiArea;

    [Header("��ͣ����")]
    private bool isPaused = false;
    private int remainingPauses;
    [SerializeField] private int maxPauses = 2;

    private bool isStopwatchActive = false; // ��������
    private bool isBountyActive = false;

    private GameSessionConfig currentSessionConfig; // �����������е�ǰ��Ϸ�Ự������
    private float difficultySpeedMultiplier = 1.0f; // ���¡��Ѷȴ������ٶȳ���
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
            // 1. ʹ��Spawner��Ĭ���б�
            currentSessionConfig.InitialTetrominoes = new List<GameObject>(spawner.GetInitialTetrominoPrefabs());

            // 2. ʹ�á���ͨ���Ѷȵĳ���
            scoreMultiplier = 2f;
            speedMultiplier = 1.5f;
        }
        else // �������Ѷ��߼���
        {
            // ���޸���ʹ���ֶ��� foreach ѭ���滻 LINQ .Where()
            var masterList = spawner.GetMasterList();
            var L1_Blocks = new List<GameObject>();
            var L2_Blocks = new List<GameObject>();
            var L3_Blocks = new List<GameObject>();

            foreach (var prefab in masterList)
            {
                if (IsInLevel(prefab, 0)) L1_Blocks.Add(prefab);
                if (IsInLevel(prefab, 1)) L2_Blocks.Add(prefab);
                if (IsInLevel(prefab, 2)) L3_Blocks.Add(prefab);
            }

            // 1. �����ѶȾ�����ʼ����
            switch (difficulty)
            {
                case Difficulty.Easy:
                    currentSessionConfig.InitialTetrominoes = L1_Blocks; // ʹ��ɸѡ�õ��б�
                    scoreMultiplier = 1f;
                    speedMultiplier = 1.0f;
                    break;
                case Difficulty.Hard:
                    var hardInitial = new List<GameObject>(L2_Blocks); // ʹ��ɸѡ�õ��б�
                    var level3Random = L3_Blocks.OrderBy(x => Random.value).Take(2).ToList();
                    hardInitial.AddRange(level3Random);
                    currentSessionConfig.InitialTetrominoes = hardInitial;
                    scoreMultiplier = 4f;
                    speedMultiplier = 1.5f;
                    break;
                case Difficulty.Normal:
                default:
                    currentSessionConfig.InitialTetrominoes = L2_Blocks; // ʹ��ɸѡ�õ��б�
                    scoreMultiplier = 2f;
                    speedMultiplier = 1.2f;
                    break;
            }
        }

        // 2. Ӧ���ٶ�����
        this.difficultySpeedMultiplier = speedMultiplier;

        // 3. ����Ŀ���������ʱ����
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

        foreach (var protocol in activeProtocols) { if (protocol != null) protocol.RemoveEffect(this); }
        activeProtocols.Clear();

        permanentSpeedBonus = 0;
        roundSpeedBonus = 0;
        countedSpeedBonus = 0;
        countedBonusBlocksRemaining = 0;

        baseFanScore = settings.baseFanScore;
        extraMultiplier = 1f;

        // ���������������л����ּӳ�
        permanentBaseScoreBonus = 0;
        roundBaseScoreBonus = 0;
        permanentBaseScoreMultiplier = 1f; // �����������ó���
        isSteroidActive = false;
        isSteroidReversalActive = false;

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

        // ���޸��������ȼ����ٶȣ������ɷ���
        UpdateFallSpeed();

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
        // ������������ʱ�����㡰���֡��͡��������ӳɣ������������á��ӳ�
        roundSpeedBonus = 0;
        countedSpeedBonus = 0;
        countedBonusBlocksRemaining = 0;

        // --- �������������������߼� ---
        if (isSteroidReversalActive)
        {
            // ����̴����� (-16) Ч���ڱ��ֽ������Ƴ�
            roundBaseScoreBonus = 0;
            isSteroidReversalActive = false;
        }
        else if (isSteroidActive)
        {
            // ����̴����� (+16) Ч���ڱ��ֽ�����ʩ�� (-16) �ķ�תЧ��
            roundBaseScoreBonus = -16;
            isSteroidActive = false;
            isSteroidReversalActive = true;
        }
        else
        {
            // ���������ϡ��� (+8) Ч���ڱ��ֽ������Ƴ�
            roundBaseScoreBonus = 0;
        }
        // �����ú���������һ�λ����ֺ�UI
        UpdateCurrentBaseScore();
        // --- �������߼����� ---

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

    // ��������1. ������֭������ (���üӳ�)
    public void ApplyPermanentBaseScoreBonus(int amount)
    {
        permanentBaseScoreBonus += amount;
        UpdateCurrentBaseScore();
    }
    // ��������2. �����������ϡ����� (���ּӳ�)
    public void ApplyRoundBaseScoreBonus(int amount)
    {
        roundBaseScoreBonus += amount;
        UpdateCurrentBaseScore();
    }
    // ��������3. ������̴������� (���Ȿ�ּӳ�)
    public void ApplySteroidBaseScoreBonus(int amount)
    {
        roundBaseScoreBonus += amount;
        isSteroidActive = true; // ��Ǵ˼ӳ����ԡ���̴���
        UpdateCurrentBaseScore();
    }
    // ��������4. �����ɾơ����� (���ó˷�)
    public void ApplyPermanentBaseScoreMultiplier(float multiplier)
    {
        permanentBaseScoreMultiplier *= multiplier;
        UpdateCurrentBaseScore();
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
    // ��������1. �������򡱺͡���֮�ȼá�����
    public void ApplyPermanentSpeedBonus(int amount)
    {
        permanentSpeedBonus += amount;
        UpdateFallSpeedAndApplyToCurrentBlock();
    }
    // ��������2. ��������ɡ������
    public void ApplyRoundSpeedBonus(int amount)
    {
        roundSpeedBonus += amount;
        UpdateFallSpeedAndApplyToCurrentBlock();
    }
    // ��������3. ������������������
    public void ApplyCountedSpeedBonus(int amount, int blockCount)
    {
        countedSpeedBonus = amount; // Ч���ɵ��ӻ򸲸ǣ��ݶ�Ϊ����
        countedBonusBlocksRemaining = blockCount;
        UpdateFallSpeedAndApplyToCurrentBlock();
    }
    // ��������4. ������������������ˢ���ٶ�
    private void UpdateFallSpeedAndApplyToCurrentBlock()
    {
        UpdateFallSpeed(); // ���¼����ٶ�
        var currentTetromino = FindObjectOfType<Tetromino>();
        if (currentTetromino != null)
        {
            // ���������ٶ�Ӧ�õ���ǰ����ķ���
            currentTetromino.UpdateFallSpeedNow(this.currentFallSpeed);
        }
    }
    // ���������� Spawner ���ã����ڡ���������������
    public void NotifyBlockSpawned()
    {
        if (countedBonusBlocksRemaining > 0)
        {
            countedBonusBlocksRemaining--;
            // ��������һ�����飬���������ɺ����üӳɲ������ٶ�
            if (countedBonusBlocksRemaining == 0)
            {
                countedSpeedBonus = 0; // Ч������
                UpdateFallSpeed(); // ����ȫ���ٶȣ���һ�����齫�ָ�����
            }
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
        if (prefab == null)
        {
            Debug.LogError("[IsInLevel] ����: ����� prefab Ϊ null!");
            return false;
        }
        string prefabName = prefab.name;
        bool result = false;

        switch (levelIndex)
        {
            case 0: // Lv.1
                result = prefabName.StartsWith("T1-") || prefabName.StartsWith("T2-") || prefabName.StartsWith("T3-");
                break;
            case 1: // Lv.2
                result = prefabName.StartsWith("T4-");
                break;
            case 2: // Lv.3
                result = prefabName.StartsWith("T5-");
                break;
        }

        // �������־��
        // ����ֻ��ӡʧ�ܵļ�飬�Լ�����־ spam
        if (!result && (levelIndex == 1 || levelIndex == 2))
        {
            Debug.Log($"[IsInLevel] ���ʧ��: Ԥ�Ƽ� '{prefabName}' (���� MasterList) �� Level {levelIndex} (T{levelIndex + 3}-) ����������ƥ�䡣");
        }
        else if (result)
        {
            Debug.Log($"<color=green>[IsInLevel] ƥ��ɹ�: '{prefabName}' ���� Level {levelIndex}</color>");
        }

        return result;
    }

    private void UpdateFallSpeed()
    {
        // 1. ��ȡ�����ٶ� (���� 10)
        int baseSpeed = settings.baseDisplayedSpeed;

        // 2. ���޸���ֻ�Ի����ٶ�Ӧ���Ѷȳ���
        int baseSpeedWithDifficulty = (int)(baseSpeed * this.difficultySpeedMultiplier);

        // 3. ���޸���������ȡ�����Ƽӳɡ� (�����Ѷ�Ӱ��)
        int huBonus = scoreManager.GetHuCount() * settings.speedIncreasePerHu_Int;

        // 4. ���޸���������ȡ������/��Լ�ӳɡ� (�����Ѷ�Ӱ��)
        int currentCountedBonus = (countedBonusBlocksRemaining > 0) ? countedSpeedBonus : 0;
        int totalBonus = permanentSpeedBonus + roundSpeedBonus + currentCountedBonus;

        // 5. ���޸�������������ӣ�(����*�Ѷ�) + (����) + (����)
        int totalDisplayedSpeed = baseSpeedWithDifficulty + huBonus + totalBonus;

        // 6. �����������ݡ����������������ٶ����Ϊ 1
        if (totalDisplayedSpeed < 1) totalDisplayedSpeed = 1;

        // 7. Ӧ���¹�ʽ������ʱ�� = 20 / ��ʾ�ٶ�
        currentFallSpeed = 20.0f / totalDisplayedSpeed;

        // 8. ����UI
        gameUI.UpdateSpeedText(totalDisplayedSpeed);
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
    // �����������ڼ���͸������л����ּӳ�
    private void UpdateCurrentBaseScore()
    {
        // 1. �������л�ȡĬ�ϻ�����
        int defaultScore = settings.baseFanScore;

        // 2. �ۼ����мӷ��ӳ�
        int addedScore = defaultScore + permanentBaseScoreBonus + roundBaseScoreBonus;

        // 3. ���޸ġ�Ӧ�ó˷��ӳ�
        int calculatedScore = (int)(addedScore * permanentBaseScoreMultiplier);

        // 4. Ӧ�á���̴���������������Ϊ1��
        if (isSteroidReversalActive && calculatedScore < 1)
        {
            calculatedScore = 1;
        }

        // 5. �������յĻ�����
        baseFanScore = calculatedScore;

        // 6. ����UI
        gameUI.UpdateBaseScoreText(baseFanScore);
    }
    }