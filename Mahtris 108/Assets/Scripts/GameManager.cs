// FileName: GameManager.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

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
    [SerializeField] private InventoryManager inventoryManager;

    private MahjongCore mahjongCore;
    [HideInInspector] public float currentFallSpeed;
    private bool isProcessingRows = false;

    [Header("Test Mode")]
    [Tooltip("开启后, 将忽略难度选择, 并使用Spawner中的'Initial Tetromino Prefabs'列表开始游戏。")]
    [SerializeField] private bool isTestMode = false;

    // 游戏状态变量
    private float remainingTime;
    private int currentScoreLevelIndex;
    private bool isEndlessMode = false;
    private List<ProtocolData> activeProtocols = new List<ProtocolData>();

    // 会被条约和道具影响的动态变量
    private float blockMultiplier;
    private float extraMultiplier;
    private int baseFanScore;

    // 【新增】基础分 V4.1 系统
    private int permanentBaseScoreBonus = 0; // "果汁" (+3)
    private int roundBaseScoreBonus = 0; // "功能饮料" (+8) 和 "类固醇" (+16 / -16)
    private float permanentBaseScoreMultiplier = 1f; // 【新增】"仙酒" (x2)
    private bool isSteroidActive = false; // 跟踪 "类固醇" (+16) 效果是否激活
    private bool isSteroidReversalActive = false; // 跟踪 "类固醇" (-16) 效果是否激活
                                                  // 【新增】条约 V4.1 状态
    public bool useDuanYaoJiuFilter = false;
    public bool useQueYiMenFilter = false;
    public int queYiMenSuitToRemove = -1;
    public bool isHunYaoShiTingActive = false;
    public bool isChaoSuanLiActive = false;
    public bool isDarkFantasyActive = false;
    public bool isTyphoonActive = false;
    public bool isMeteorShowerActive = false;
    public bool isTrickRoomActive = false;
    public bool isNoGravityActive = false;

    // 用于特殊流程控制的内部变量
    private bool _isBombOrSpecialClear = false;

    private int permanentSpeedBonus = 0; // 永久加成 (气球, 神之救济, 条约)
    private int roundSpeedBonus = 0; // 本轮加成 (降落伞)
    private int countedSpeedBonus = 0; // 计数加成 (喷气背包)
    private int countedBonusBlocksRemaining = 0; // 喷气背包剩余方块数
    public Spawner Spawner => spawner;
    public HuPaiArea HuPaiArea => huPaiArea;

    [Header("暂停功能")]
    private bool isPaused = false;
    private int remainingPauses;
    [SerializeField] private int maxPauses = 2;

    private bool isStopwatchActive = false; // 【新增】
    private bool isBountyActive = false;
    private int ignoreMahjongCheckCount = 0; // 【新增】"垃圾筒"
    private int bonusBlocksOnHuCount = 0;    // 【新增】"试用小样"
    private string bonusBlockPrefabName = "";  // 【新增】"试用小样"
    // 【新增】“点金手”
    private float midasTimer = 0f;
    private int midasGoldValue = 0;
    // 【新增】“计分板”
    private float scoreboardTimer = 0f;


    private GameSessionConfig currentSessionConfig; // 【新增】持有当前游戏会话的配置
    private float difficultySpeedMultiplier = 1.0f; // 【新】难度带来的速度乘数
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
        // 原有的暂停判断逻辑
        if (isPaused || isProcessingRows || Time.timeScale == 0f) return;
        if (isProcessingRows || Time.timeScale == 0f) return;
        remainingTime -= Time.deltaTime;
        gameUI.UpdateTimerText(remainingTime);
        // 【新增】处理“点金手”计时器
        if (midasTimer > 0)
        {
            midasTimer -= Time.deltaTime;
            if (midasTimer <= 0)
            {
                midasTimer = 0;
                midasGoldValue = 0;
            }
        }
        // 【新增】处理“计分板”计时器
        if (scoreboardTimer > 0)
        {
            scoreboardTimer -= Time.deltaTime;
            if (scoreboardTimer <= 0)
            {
                scoreboardTimer = 0;
            }
        }
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
        // === 第1部分: 创建并填充当前游戏会话的配置 ===
        currentSessionConfig = new GameSessionConfig();
        Difficulty difficulty = DifficultyManager.Instance.CurrentDifficulty;

        // 定义难度乘数
        float scoreMultiplier = 1f;
        float speedMultiplier = 1f;

        // 【新增：测试模式检查】
        if (isTestMode)
        {
            // 1. 使用Spawner的默认列表
            currentSessionConfig.InitialTetrominoes = new List<GameObject>(spawner.GetInitialTetrominoPrefabs());

            // 2. 使用“普通”难度的乘数
            scoreMultiplier = 2f;
            speedMultiplier = 1.5f;
        }
        else // 【常规难度逻辑】
        {
            // 【修复】使用手动的 foreach 循环替换 LINQ .Where()
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

            // 1. 根据难度决定初始方块
            switch (difficulty)
            {
                case Difficulty.Easy:
                    currentSessionConfig.InitialTetrominoes = L1_Blocks; // 使用筛选好的列表
                    scoreMultiplier = 1f;
                    speedMultiplier = 1.0f;
                    break;
                case Difficulty.Hard:
                    var hardInitial = new List<GameObject>(L2_Blocks); // 使用筛选好的列表
                    var level3Random = L3_Blocks.OrderBy(x => Random.value).Take(2).ToList();
                    hardInitial.AddRange(level3Random);
                    currentSessionConfig.InitialTetrominoes = hardInitial;
                    scoreMultiplier = 4f;
                    speedMultiplier = 1.5f;
                    break;
                case Difficulty.Normal:
                default:
                    currentSessionConfig.InitialTetrominoes = L2_Blocks; // 使用筛选好的列表
                    scoreMultiplier = 2f;
                    speedMultiplier = 1.2f;
                    break;
            }
        }

        // 2. 应用速度配置
        this.difficultySpeedMultiplier = speedMultiplier;

        // 3. 创建目标分数的临时副本
        foreach (var levelTemplate in settings.scoreLevels)
        {
            currentSessionConfig.DifficultyScoreLevels.Add(new ScoreLevel
            {
                targetScore = (int)(levelTemplate.targetScore * scoreMultiplier),
                goldReward = (int)(levelTemplate.goldReward * scoreMultiplier)
            });
        }

        // === 第2部分: 使用新配置重置游戏状态 ===
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

        // 【新增】重置所有基础分加成
        permanentBaseScoreBonus = 0;
        roundBaseScoreBonus = 0;
        permanentBaseScoreMultiplier = 1f; // 【新增】重置乘数
        isSteroidActive = false;
        isSteroidReversalActive = false;
        ignoreMahjongCheckCount = 0; // 【新增】
        bonusBlocksOnHuCount = 0;    // 【新增】
        useDuanYaoJiuFilter = false;
        useQueYiMenFilter = false;
        queYiMenSuitToRemove = -1;
        isHunYaoShiTingActive = false;
        isChaoSuanLiActive = false;
        isDarkFantasyActive = false;
        isTyphoonActive = false;
        isMeteorShowerActive = false;
        isTrickRoomActive = false;

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

        // 【修复】必须先计算速度，再生成方块
        UpdateFallSpeed();

        // 使用会话配置中的数据来初始化
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
        // 【新增】胡牌时重置“点金手”和“计分板”
        midasTimer = 0f;
        midasGoldValue = 0;
        scoreboardTimer = 0f;
        ignoreMahjongCheckCount = 0; // 【新增】胡牌后重置垃圾筒
        // 【新增】胡牌时，清零“本轮”和“计数”加成，但保留“永久”加成
        roundSpeedBonus = 0;
        countedSpeedBonus = 0;
        countedBonusBlocksRemaining = 0;

        // --- 【新增】基础分重置逻辑 ---
        if (isSteroidReversalActive)
        {
            // “类固醇”的 (-16) 效果在本轮结束，移除
            roundBaseScoreBonus = 0;
            isSteroidReversalActive = false;
        }
        else if (isSteroidActive)
        {
            // “类固醇”的 (+16) 效果在本轮结束，施加 (-16) 的反转效果
            roundBaseScoreBonus = -16;
            isSteroidActive = false;
            isSteroidReversalActive = true;
        }
        else
        {
            // “功能饮料”的 (+8) 效果在本轮结束，移除
            roundBaseScoreBonus = 0;
        }
        // 在重置后，立即更新一次基础分和UI
        UpdateCurrentBaseScore();
        // --- 基础分逻辑结束 ---

        // 【新增】如果停表效果激活，则重置暂停次数
        if (isStopwatchActive)
        {
            remainingPauses = maxPauses;
            isStopwatchActive = false;
        }
        else // 如果停表没激活，才正常重置
        {
            remainingPauses = maxPauses;
        }

        if (gameUI != null) gameUI.UpdatePauseUI(isPaused, remainingPauses);
        remainingPauses = maxPauses;
        if (gameUI != null) gameUI.UpdatePauseUI(isPaused, remainingPauses);
        gameUI.HideHuPopup();
        Time.timeScale = 1f;
        UpdateFallSpeed();
        blockPool.ResetFullDeck(); // 【关键】这一行会自动重新应用过滤器
        blockPool.ResetFullDeck();
        tetrisGrid.ClearAllBlocks();
        huPaiArea.ClearAll();
        spawner.StartNextRound();
        // 【新增】“试用小样”奖励逻辑
        if (bonusBlocksOnHuCount > 0 && !string.IsNullOrEmpty(bonusBlockPrefabName))
        {
            var prefab = spawner.GetMasterList().FirstOrDefault(p => p.name == bonusBlockPrefabName);
            if (prefab != null)
            {
                for (int i = 0; i < bonusBlocksOnHuCount; i++)
                {
                    spawner.AddTetrominoToPool(prefab);
                }
            }
            bonusBlocksOnHuCount = 0;
            bonusBlockPrefabName = "";
        }
        isProcessingRows = false;
    }

    private void HandleRowsCleared(List<int> rowIndices)
    {
        if (isProcessingRows) return;
        isProcessingRows = true;
        rowIndices.Sort();

        // 【修复】将 allClearedTransforms 和 allRemainingIds 提到顶部声明一次
        List<Transform> allClearedTransforms = new List<Transform>();
        List<int> allRemainingIds = new List<int>();

        // 【新增】“垃圾筒”逻辑
        if (ignoreMahjongCheckCount > 0)
        {
            ignoreMahjongCheckCount--;

            // --- 跳过麻将判定的快速清理流程 ---
            foreach (var y in rowIndices)
            {
                var rowData = tetrisGrid.GetRowDataAndClear(y);
                allClearedTransforms.AddRange(rowData.transforms);
                allRemainingIds.AddRange(rowData.blockIds); // 【修复】垃圾筒也应归还牌，而不是销毁
            }
            // --- 流程结束 ---
        }
        // 【新增】“点金手”和“计分板”逻辑
        else if (midasTimer > 0) // 【修复】将此逻辑改为 else if，防止垃圾筒也触发
        {
            GameSession.Instance.AddGold(midasGoldValue);
            midasGoldValue *= 2;
        }

        if (scoreboardTimer > 0) // (计分板可以和垃圾桶并存)
        {
            int currentScore = scoreManager.GetCurrentScore();
            int bonusScore = Mathf.RoundToInt(currentScore * 0.05f);
            scoreManager.AddScore(bonusScore);
        }

        // --- 正常的麻将判定逻辑 ---
        if (ignoreMahjongCheckCount <= 0) // 【修改】仅在垃圾筒未激活时执行
        {
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
        }
        // --- 麻将判定结束 ---

        // 统一在末尾执行清理
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

    // --- 公开给道具和条约调用的接口 ---
    public void AddTime(float time) => remainingTime += time;

    // 【新增】1. 供“果汁”调用 (永久加成)
    public void ApplyPermanentBaseScoreBonus(int amount)
    {
        permanentBaseScoreBonus += amount;
        UpdateCurrentBaseScore();
    }
    // 【新增】2. 供“功能饮料”调用 (本轮加成)
    public void ApplyRoundBaseScoreBonus(int amount)
    {
        roundBaseScoreBonus += amount;
        UpdateCurrentBaseScore();
    }
    // 【新增】3. 供“类固醇”调用 (特殊本轮加成)
    public void ApplySteroidBaseScoreBonus(int amount)
    {
        roundBaseScoreBonus += amount;
        isSteroidActive = true; // 标记此加成来自“类固醇”
        UpdateCurrentBaseScore();
    }
    // 【新增】4. 供“仙酒”调用 (永久乘法)
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
            // 可以在这里触发一个 OnProtocolsChanged 事件，让UI更新
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
        // 1. 调用现有的方法来计算总倍率并更新倍率文本
        // RecalculateBlockMultiplier() 会计算并更新 this.blockMultiplier 字段
        RecalculateBlockMultiplier();

        // 2. 从 Spawner 获取当前的方块池
        var prefabs = spawner.GetActivePrefabs();

        // 3. 获取刚计算出的总倍率
        float totalMultiplier = this.blockMultiplier;

        // 4. 调用 GameUIController 来更新方块列表的UI显示
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
    // 【新增】1. 供“气球”和“神之救济”调用
    public void ApplyPermanentSpeedBonus(int amount)
    {
        permanentSpeedBonus += amount;
        UpdateFallSpeedAndApplyToCurrentBlock();
    }
    // 【新增】2. 供“降落伞”调用
    public void ApplyRoundSpeedBonus(int amount)
    {
        roundSpeedBonus += amount;
        UpdateFallSpeedAndApplyToCurrentBlock();
    }
    // 【新增】3. 供“喷气背包”调用
    public void ApplyCountedSpeedBonus(int amount, int blockCount)
    {
        countedSpeedBonus = amount; // 效果可叠加或覆盖，暂定为覆盖
        countedBonusBlocksRemaining = blockCount;
        UpdateFallSpeedAndApplyToCurrentBlock();
    }
    // 【新增】4. 辅助方法，用于立即刷新速度
    private void UpdateFallSpeedAndApplyToCurrentBlock()
    {
        UpdateFallSpeed(); // 重新计算速度
        var currentTetromino = FindObjectOfType<Tetromino>();
        if (currentTetromino != null)
        {
            // 立即将新速度应用到当前下落的方块
            currentTetromino.UpdateFallSpeedNow(this.currentFallSpeed);
        }
    }
    // 【新增】供 Spawner 调用，用于“喷气背包”计数
    public void NotifyBlockSpawned()
    {
        if (countedBonusBlocksRemaining > 0)
        {
            countedBonusBlocksRemaining--;
            // 如果是最后一个方块，则在它生成后重置加成并更新速度
            if (countedBonusBlocksRemaining == 0)
            {
                countedSpeedBonus = 0; // 效果结束
                UpdateFallSpeed(); // 更新全局速度，下一个方块将恢复正常
            }
        }
    }
    // --- 私有辅助方法 ---
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
        // 【新增】“超算力”逻辑
        BlockRewardWeights adjustedWeights = new BlockRewardWeights
        {
            level1Weight = weights.level1Weight,
            level2Weight = weights.level2Weight,
            level3Weight = weights.level3Weight
        };

        if (isChaoSuanLiActive)
        {
            // 检查玩家是否已有 L3 方块
            bool hasLevel3 = spawner.GetActivePrefabs().Any(p => IsInLevel(p, 2));
            if (hasLevel3)
            {
                // 权重提高200%（即变为原来的3倍）
                adjustedWeights.level3Weight *= 3f;
                // （可选：重新归一化权重，使总和为1）
                float total = adjustedWeights.level1Weight + adjustedWeights.level2Weight + adjustedWeights.level3Weight;
                adjustedWeights.level1Weight /= total;
                adjustedWeights.level2Weight /= total;
                adjustedWeights.level3Weight /= total;
            }
        }
        // --- 超算力逻辑结束 ---
        var source = spawner.GetMasterList();
        var level1 = source.Where(p => IsInLevel(p, 0)).ToList();
        var level2 = source.Where(p => IsInLevel(p, 1)).ToList();
        var level3 = source.Where(p => IsInLevel(p, 2)).ToList();
        var result = new List<GameObject>();
        for (int i = 0; i < count; i++)
        {
            GameObject chosenBlock = null;
            float roll = Random.value;
            if (roll < adjustedWeights.level1Weight && level1.Count > 0)
                chosenBlock = level1[Random.Range(0, level1.Count)];
            else if (roll < adjustedWeights.level1Weight + adjustedWeights.level2Weight && level2.Count > 0)
                chosenBlock = level2[Random.Range(0, level2.Count)];

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
            Debug.LogError("[IsInLevel] 错误: 传入的 prefab 为 null!");
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

        // 【诊断日志】
        // 我们只打印失败的检查，以减少日志 spam
        if (!result && (levelIndex == 1 || levelIndex == 2))
        {
            Debug.Log($"[IsInLevel] 检查失败: 预制件 '{prefabName}' (来自 MasterList) 与 Level {levelIndex} (T{levelIndex + 3}-) 的命名规则不匹配。");
        }
        else if (result)
        {
            Debug.Log($"<color=green>[IsInLevel] 匹配成功: '{prefabName}' 属于 Level {levelIndex}</color>");
        }

        return result;
    }

    private void UpdateFallSpeed()
    {
        // 1. 获取基础速度 (例如 10)
        int baseSpeed = settings.baseDisplayedSpeed;

        // 2. 【修复】只对基础速度应用难度乘数
        int baseSpeedWithDifficulty = (int)(baseSpeed * this.difficultySpeedMultiplier);

        // 3. 【修复】单独获取“胡牌加成” (不受难度影响)
        int huBonus = scoreManager.GetHuCount() * settings.speedIncreasePerHu_Int;

        // 4. 【修复】单独获取“道具/条约加成” (不受难度影响)
        int currentCountedBonus = (countedBonusBlocksRemaining > 0) ? countedSpeedBonus : 0;
        int totalBonus = permanentSpeedBonus + roundSpeedBonus + currentCountedBonus;

        // 5. 【修复】将三部分相加：(基础*难度) + (胡牌) + (道具)
        int totalDisplayedSpeed = baseSpeedWithDifficulty + huBonus + totalBonus;

        // 6. 【保留】根据“喷气背包”需求，速度最低为 1
        if (totalDisplayedSpeed < 1) totalDisplayedSpeed = 1;

        // 7. 应用新公式：下落时间 = 20 / 显示速度
        currentFallSpeed = 20.0f / totalDisplayedSpeed;

        // 8. 更新UI
        gameUI.UpdateSpeedText(totalDisplayedSpeed);
    }

    private void OnScoreUpdated(int newScore)
    {
        // 【修改】使用 currentSessionConfig，并检查其是否为null
        if (isEndlessMode || currentSessionConfig == null || currentSessionConfig.DifficultyScoreLevels.Count == 0) return;

        // 【修改】使用 currentSessionConfig
        while (currentScoreLevelIndex < currentSessionConfig.DifficultyScoreLevels.Count && newScore >= currentSessionConfig.DifficultyScoreLevels[currentScoreLevelIndex].targetScore)
        {
            if (GameSession.Instance != null)
            {
                // 【修改】使用 currentSessionConfig
                GameSession.Instance.AddGold(currentSessionConfig.DifficultyScoreLevels[currentScoreLevelIndex].goldReward);
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.SoundLibrary.targetReached);
            }

            currentScoreLevelIndex++;

            // 【修改】使用 currentSessionConfig
            if (currentScoreLevelIndex >= currentSessionConfig.DifficultyScoreLevels.Count)
            {
                HandleGameWon();
            }

            UpdateTargetScoreUI();
        }

        // (此行来自“进度条”功能，保持不变)
        gameUI.UpdateScoreProgress(newScore);
    }

    private void UpdateTargetScoreUI()
    {
        if (isEndlessMode)
        {
            gameUI.UpdateTargetScoreDisplay("无尽模式");
        }
        // 【修改】使用 currentSessionConfig
        else if (currentScoreLevelIndex < currentSessionConfig.DifficultyScoreLevels.Count)
        {
            // 【修改】使用 currentSessionConfig
            var level = currentSessionConfig.DifficultyScoreLevels[currentScoreLevelIndex];
            gameUI.UpdateTargetScoreDisplay(level.targetScore, level.goldReward);

            // (此行来自“进度条”功能，保持不变)
            gameUI.UpdateScoreProgress(scoreManager.GetCurrentScore());
        }
    }

    private void HandleGameOver()
    {
        Time.timeScale = 0f;
        int finalScore = scoreManager.GetCurrentScore();
        bool isNewHighScore = scoreManager.CheckForNewHighScore(finalScore); // 需要在ScoreManager中实现此方法
        gameUI.ShowGameEndPanel(false, finalScore, isNewHighScore);
    }

    public void TogglePause()
    {
        // 如果游戏已结束，不允许暂停
        if (Time.timeScale == 0f && !isPaused) return;

        if (isPaused)
        {
            // --- 取消暂停 ---
            isPaused = false;
            Time.timeScale = 1f;
            gameUI.ShowPausePanel(false);
        }
        else
        {
            // --- 尝试暂停 ---
            if (remainingPauses > 0)
            {
                isPaused = true;
                remainingPauses--;
                Time.timeScale = 0f;
                gameUI.ShowPausePanel(true);
            }
            else
            {
                Debug.Log("暂停次数已用完!");
                // (可选) 可以在此触发一个UI提示
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
        gameUI.HideAllPanels(); // 确保结束面板被隐藏
        UpdateTargetScoreUI();
    }
    public void SetStopwatchActive(bool isActive)
    {
        isStopwatchActive = isActive;
    }
    // 添加新变量
    private ItemData lastUsedItem = null;

    // 添加新的公共属性，以便道具脚本可以访问 InventoryManager
    public InventoryManager Inventory => inventoryManager;

    // 添加新方法
    public void SetLastUsedItem(ItemData item)
    {
        // 不记录复制器本身
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
            // 立即更新UI，让玩家看到奖励翻倍了
            UpdateTargetScoreUI();
        }
    }
    // 【新增】用于计算和更新所有基础分加成
    private void UpdateCurrentBaseScore()
    {
        // 1. 从设置中获取默认基础分
        int defaultScore = settings.baseFanScore;

        // 2. 累加所有加法加成
        int addedScore = defaultScore + permanentBaseScoreBonus + roundBaseScoreBonus;

        // 3. 【修改】应用乘法加成
        int calculatedScore = (int)(addedScore * permanentBaseScoreMultiplier);

        // 4. 应用“类固醇”的特殊规则（最低为1）
        if (isSteroidReversalActive && calculatedScore < 1)
        {
            calculatedScore = 1;
        }

        // 5. 设置最终的基础分
        baseFanScore = calculatedScore;

        // 6. 更新UI
        gameUI.UpdateBaseScoreText(baseFanScore);
    }
    // 【新增】供“快进按钮”调用
    public void AddHuCount(int amount)
    {
        if (scoreManager != null)
        {
            scoreManager.AddHuCount(amount);
        }
    }

    // 【新增】供“垃圾筒”调用
    public void SetIgnoreMahjongCheck(int count)
    {
        ignoreMahjongCheckCount = count;
    }

    // 【新增】供“试用小样”调用
    public void ActivateBonusBlocksOnHu(string prefabName, int count)
    {
        bonusBlockPrefabName = prefabName;
        bonusBlocksOnHuCount = count;
    }
    public bool TryFindAndAddRandomSetFromPool()
    {
        // 1. 检查胡牌区是否已满
        if (huPaiArea.GetSetCount() >= settings.setsForHu)
        {
            return false; // 胡牌区已满，无法使用
        }
        // 2. 获取当前牌库
        List<int> availableIds = blockPool.GetAvailableBlockIDs();
        if (availableIds.Count < 3)
        {
            return false; // 牌库不足
        }

        List<List<int>> potentialSets = new List<List<int>>();

        // 3. 查找所有可能的“刻子”
        var pungGroups = availableIds.GroupBy(id => id % 27) // 按牌值分组
                                     .Where(g => g.Count() >= 3);
        foreach (var group in pungGroups)
        {
            potentialSets.Add(group.Take(3).ToList());
        }

        // 4. 查找所有可能的“顺子”
        var tilesBySuit = availableIds.GroupBy(id => (id % 27) / 9); // 按花色分组
        foreach (var suitGroup in tilesBySuit)
        {
            var uniqueTilesInSuit = suitGroup.Select(id => id % 27).Distinct().OrderBy(val => val).ToList();
            for (int i = 0; i < uniqueTilesInSuit.Count - 2; i++)
            {
                int v1_val = uniqueTilesInSuit[i];
                int v2_val = uniqueTilesInSuit[i + 1];
                int v3_val = uniqueTilesInSuit[i + 2];

                // 检查是否连续
                if ((v1_val % 9 <= 6) && (v2_val == v1_val + 1) && (v3_val == v1_val + 2))
                {
                    // 找到顺子，从牌库中获取对应的3张牌ID
                    int id1 = suitGroup.First(id => (id % 27) == v1_val);
                    int id2 = suitGroup.First(id => (id % 27) == v2_val);
                    int id3 = suitGroup.First(id => (id % 27) == v3_val);
                    potentialSets.Add(new List<int> { id1, id2, id3 });
                }
            }
        }

        // 5. 如果没有找到任何可组成的牌
        if (potentialSets.Count == 0)
        {
            return false; // 牌库中没有可用的组合
        }

        // 6. 随机选择一个并添加到胡牌区
        List<int> chosenSet = potentialSets[Random.Range(0, potentialSets.Count)];
        if (blockPool.RemoveSpecificBlockIds(chosenSet))
        {
            huPaiArea.AddSets(new List<List<int>> { chosenSet });
            return true; // 道具使用成功
        }

        return false; // 移除牌时出错（理论上不应发生）
    }
    // 【新增】供“点金手”调用
    public void ActivateMidas(float duration)
    {
        midasTimer = duration;
        midasGoldValue = 1; // 激活时，将初始金币设为1
    }
    // 【新增】供“计分板”调用
    public void ActivateScoreboard(float duration)
    {
        scoreboardTimer = duration;
    }
    // 【新增】供“流星雨”条约在方块生成时临时更新UI
    public void UpdateSpeedUITemp(int speedValue)
    {
        gameUI.UpdateSpeedText(speedValue);
    }
}