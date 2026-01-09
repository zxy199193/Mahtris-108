// FileName: GameManager.cs
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // ========================================================================
    // 1. 单例与核心配置
    // ========================================================================
    public static GameManager Instance { get; private set; }

    [Header("核心配置")]
    [SerializeField] private GameSettings settings;

    // ========================================================================
    // 2. 模块引用
    // ========================================================================
    [Header("模块引用")]
    [SerializeField] private Spawner spawner;
    [SerializeField] private TetrisGrid tetrisGrid;
    [SerializeField] private HuPaiArea huPaiArea;
    [SerializeField] private GameUIController gameUI;
    [SerializeField] private BlockPool blockPool;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private InventoryManager inventoryManager;

    // 内部核心实例
    private MahjongCore mahjongCore;
    public Spawner Spawner => spawner;
    public HuPaiArea HuPaiArea => huPaiArea;

    // 【修正】防止 Inventory 定义冲突，这里保留唯一的定义
    public InventoryManager Inventory => inventoryManager;
    public BlockPool BlockPool => blockPool;
    // ========================================================================
    // 3. 游戏核心状态
    // ========================================================================
    [HideInInspector] public float currentFallSpeed;
    private bool isProcessingRows = false;
    private float remainingTime;
    private bool isEndlessMode = false;
    private bool hasClearedRowsInThisRound = false;

    // 流程控制
    private bool _hasDeclaredHuThisFrame = false;
    private bool _pendingGameWin = false;
    private bool _lastBulletTimeState = false;
    private bool _tempIsTianHu = false;
    private bool _tempIsDiHu = false;
    private bool _isBombOrSpecialClear = false;
    private string _currentGameOverReason = "";

    // ========================================================================
    // 4. 难度与会话配置
    // ========================================================================
    private GameSessionConfig currentSessionConfig;
    private int currentScoreLevelIndex;
    private float difficultySpeedMultiplier = 1.0f;

    // ========================================================================
    // 5. 数值倍率与基础分系统 (V4.1+)
    // ========================================================================
    [Header("数值系统")]
    private float blockMultiplier;
    private float extraMultiplier;
    private int baseFanScore;

    private int permanentBaseScoreBonus = 0;
    private int roundBaseScoreBonus = 0;
    private float permanentBaseScoreMultiplier = 1f;
    private float permanentBlockMultiplierModifier = 0f;

    private int permanentSpeedBonus = 0;
    private int roundSpeedBonus = 0;
    private int countedSpeedBonus = 0;
    private int countedBonusBlocksRemaining = 0;

    // ========================================================================
    // 6. 条约系统状态 (Protocols)
    // ========================================================================
    private List<ProtocolData> activeProtocols = new List<ProtocolData>();
    private List<ProtocolData> protocolsMarkedForRemoval = new List<ProtocolData>();
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
    public bool isAttackOnGiantActive = false;
    public bool isCheapWarehouseActive = false;
    public bool isMarshLandActive = false;
    public bool isRenewableEnergyActive = false;
    public bool isAllMenEqualActive = false;
    public bool isStrongWorldActive = false;
    public bool isRoutineWorkActive = false;
    public bool isUnstableCurrentActive = false;
    public bool isSSSVIPActive = false;
    public bool isAdventFoodActive = false;
    private bool _snapshotSSSVIP = false;
    private bool _snapshotStrongWorld = false;
    public bool isDelayGratificationActive = false;
    public bool isDrMahjongActive = false;
    public bool isOldSchoolActive = false;
    public bool isBerserkerActive = false;
    public bool isTimeIsMoneyActive = false;
    public bool isBulletTimeActive = false;
    public bool isLogBridgeActive = false;
    public bool isGreatRevolutionActive = false;
    public bool isTrinityActive = false;
    public bool isRealpolitikActive = false;
    public bool isNatureReserveActive = false;
    public bool isLastGaspGoalActive = false;
    private int lastGaspGoalBonus = 0;
    public bool isBloomingOnKongActive = false;
    public bool isSubspaceActive = false;
    private int cumulativeHuSpeedBonus = 0;
    public bool isBottomMoonActive = false;
    public bool isLastStandActive = false;
    public bool isOneManArmyActive = false;
    public bool isMistActive = false;
    private float _omaCurrentGrowth = 2f;
    private float _omaAppliedFactor = 1f;
    private bool _pendingPassportShuffle = false;
    private float _tempLastHandMult = 1f;
    // ========================================================================
    // 7. 道具与特殊机制状态
    // ========================================================================
    public int luckyCapStack = 0;
    public bool isFilterActive = false;

    // 【修正】防止 lastUsedItem 定义冲突，这里保留唯一的定义
    private ItemData lastUsedItem = null;

    private float kidsMealTimer = 0f;
    private float marshLandTimer = 0f;
    private float adventFoodTimer = 0f;
    private float unstableCurrentTimer = 0f;
    private float filterTimer = 0f;
    private float midasTimer = 0f;
    private float scoreboardTimer = 0f;

    private int delayGratificationBonus = 0;
    private int adventFoodBonus = 0;
    private int midasGoldValue = 0;
    private bool hasReviveStone = false;
    private float reviveAddedTime = 0f;
    private bool isSteroidActive = false;
    private bool isSteroidReversalActive = false;

    private bool isWantedPosterActive = false;
    private int wantedPosterGoldMult = 1;
    private bool isBountyActive = false;
    private int ignoreMahjongCheckCount = 0;

    private bool isChampagneActive = false;
    private int champagneSpawnCount = 0;

    private int activePassportSuit = -1;
    private float passportTimer = 0f;

    // ========================================================================
    // 8. 暂停控制
    // ========================================================================
    [Header("暂停功能")]
    private bool isPaused = false;

    // ========================================================================
    // 9. 教学
    // ========================================================================
    private const string PREF_HAS_SEEN_TUTORIAL = "HasSeenTutorial";

    // ========================================================================
    // 10. 其他
    // ========================================================================
    public int CurrentDisplaySpeed { get; private set; }
    private int _itemsUsedThisGame = 0;
    private int _protocolsObtainedThisGame = 0;
    private int _currentRefreshCost;
    private int _gameExecutionId = 0;
    private int _loopOfLastLegendaryAppearance = 0;
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

    void Start()
    {
        // 【新增】根据当前难度播放对应的游戏 BGM
        if (AudioManager.Instance != null && DifficultyManager.Instance != null)
        {
            AudioManager.Instance.PlayGameBGM(DifficultyManager.Instance.CurrentDifficulty);
        }

        StartNewGame();
        if (AchievementManager.Instance != null)
        {
            AchievementManager.Instance.CheckAllUnlockProgress(settings);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) TogglePause();
        if (isPaused || isProcessingRows || Time.timeScale == 0f) return;

        // 【修复】移除这里的实时高度检测，改为使用缓存的状态 _lastBulletTimeState
        // 这样正在下落的方块不会导致状态频繁闪烁或误判

        // 计算时间流逝
        float logicDeltaTime = Time.deltaTime;

        // 使用缓存的状态来决定时间流速
        if (_lastBulletTimeState)
        {
            logicDeltaTime *= 0.2f;
        }

        remainingTime -= logicDeltaTime;

        // 更新时间UI
        gameUI.UpdateTimerText(remainingTime, _lastBulletTimeState);
        if (isLastGaspGoalActive)
        {
            // 判定条件：剩余时间 <= 3秒
            bool shouldTrigger = remainingTime <= 3.0f;

            if (shouldTrigger && lastGaspGoalBonus == 0)
            {
                // 触发绝杀：加 36 分
                lastGaspGoalBonus = 36;
                UpdateCurrentBaseScore();
                // 可选：播放个心跳音效或视觉提示
            }
            else if (!shouldTrigger && lastGaspGoalBonus != 0)
            {
                // 时间回升 (吃了道具/胡牌加时)：撤销 36 分
                lastGaspGoalBonus = 0;
                UpdateCurrentBaseScore();
            }
        }
        else if (lastGaspGoalBonus != 0)
        {
            // 条约被移除/失效：清理残余分数
            lastGaspGoalBonus = 0;
            UpdateCurrentBaseScore();
        }
        if (kidsMealTimer > 0) kidsMealTimer -= logicDeltaTime;

        if (isMarshLandActive)
        {
            marshLandTimer -= logicDeltaTime;
            if (marshLandTimer <= 0) { marshLandTimer = 10f; ForceClearRowsFromBottom(1); }
        }

        if (isAdventFoodActive)
        {
            adventFoodTimer -= logicDeltaTime;
            if (adventFoodTimer <= 0)
            {
                adventFoodTimer = 1f;
                if (baseFanScore > 1)
                {
                    adventFoodBonus--;
                    UpdateCurrentBaseScore();
                }
            }
        }

        if (isUnstableCurrentActive)
        {
            unstableCurrentTimer -= logicDeltaTime;
            if (unstableCurrentTimer <= 0)
            {
                unstableCurrentTimer = 6f;
                int change = Random.Range(1, 37);
                if (Random.value < 0.5f) change = -change;
                if (baseFanScore + change < 1) change = 1 - baseFanScore;
                ApplyRoundBaseScoreBonus(change);
            }
        }
        if (isFilterActive)
        {
            filterTimer -= logicDeltaTime;
            if (filterTimer <= 0) isFilterActive = false;
        }

        if (midasTimer > 0) midasTimer -= logicDeltaTime;
        if (scoreboardTimer > 0) scoreboardTimer -= logicDeltaTime;

        if (remainingTime <= 0) TriggerGameOver("GAME_OVER_TIMEUP");
        if (activePassportSuit != -1)
        {
            passportTimer -= Time.deltaTime; // 使用非逻辑时间，或者 logicDeltaTime 取决于是否受暂停影响
                                             // 建议使用 logicDeltaTime (受暂停影响)
                                             // passportTimer -= logicDeltaTime; 

            if (passportTimer <= 0)
            {
                activePassportSuit = -1; // 过期失效

                // 【核心修改】只设置标记，不立即洗牌
                _pendingPassportShuffle = true;

                // 也不要刷新 UI 了，保持界面稳定
                // if (spawner != null) spawner.RefreshPreviewTilesOnly(); <--- 删除这行

                Debug.Log("护照失效：已标记为待洗牌，将在下一次生成方块时执行。");
            }
        }
    }
    public void TriggerGameOver(string reasonKey)
    {
        _currentGameOverReason = reasonKey;
        GameEvents.TriggerGameOver();
    }
    void OnEnable()
    {
        GameEvents.OnRowsCleared += HandleRowsCleared;
        GameEvents.OnHuDeclared += HandleHuDeclared;
        GameEvents.OnGameOver += HandleGameOver;
        GameEvents.OnPoolCountChanged += CheckCheapWarehouse;
        ScoreManager.OnScoreChanged += OnScoreUpdated;
    }

    void OnDisable()
    {
        GameEvents.OnRowsCleared -= HandleRowsCleared;
        GameEvents.OnHuDeclared -= HandleHuDeclared;
        GameEvents.OnGameOver -= HandleGameOver;
        GameEvents.OnPoolCountChanged -= CheckCheapWarehouse;
        ScoreManager.OnScoreChanged -= OnScoreUpdated;
    }

    public void StartNewGame()
    {
        _gameExecutionId++;
        // === 第1部分: 创建并填充当前游戏会话的配置 ===
        currentSessionConfig = new GameSessionConfig();
        Difficulty difficulty = DifficultyManager.Instance.CurrentDifficulty;

        // 定义难度乘数
        float scoreMultiplier = 1f;
        float speedMultiplier = 1f;

        // 【新增：测试模式检查】
        if (settings.isTestMode)
        {
            // 1. 使用Spawner的默认列表
            currentSessionConfig.InitialTetrominoes = new List<GameObject>(spawner.GetInitialTetrominoPrefabs());

            // 2. 使用“普通”难度的乘数
            scoreMultiplier = 2f;
            speedMultiplier = 1.5f;
            Debug.LogWarning("【测试模式已激活】读取 GameSettings 配置...");
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
                    scoreMultiplier = 100f;
                    speedMultiplier = 1.5f;
                    break;
                case Difficulty.Normal:
                default:
                    currentSessionConfig.InitialTetrominoes = L2_Blocks; // 使用筛选好的列表
                    scoreMultiplier = 20f;
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
        permanentBlockMultiplierModifier = 0f;

        baseFanScore = settings.baseFanScore;
        extraMultiplier = 1f;

        // 【新增】重置所有基础分加成
        permanentBaseScoreBonus = 0;
        roundBaseScoreBonus = 0;
        permanentBaseScoreMultiplier = 1f;
        isSteroidActive = false;
        isSteroidReversalActive = false;
        ignoreMahjongCheckCount = 0;
        useDuanYaoJiuFilter = false;
        useQueYiMenFilter = false;
        queYiMenSuitToRemove = -1;
        isHunYaoShiTingActive = false;
        isChaoSuanLiActive = false;
        isDarkFantasyActive = false;
        isTyphoonActive = false;
        isMeteorShowerActive = false;
        isTrickRoomActive = false;
        kidsMealTimer = 0f;
        hasReviveStone = false;
        isAttackOnGiantActive = false;
        isCheapWarehouseActive = false;
        isMarshLandActive = false;
        isRenewableEnergyActive = false;
        isAllMenEqualActive = false;
        isStrongWorldActive = false;
        isAdventFoodActive = false;
        isRoutineWorkActive = false;
        isUnstableCurrentActive = false;
        isSSSVIPActive = false;
        adventFoodBonus = 0;
        isDelayGratificationActive = false;
        delayGratificationBonus = 0;
        isDrMahjongActive = false;
        isOldSchoolActive = false;
        isBerserkerActive = false;
        isTimeIsMoneyActive = false;
        isBulletTimeActive = false;
        isLogBridgeActive = false;
        isGreatRevolutionActive = false;
        isTrinityActive = false;
        isRealpolitikActive = false;
        isWantedPosterActive = false;
        wantedPosterGoldMult = 1;
        luckyCapStack = 0;
        isFilterActive = false;
        filterTimer = 0f;
        isChampagneActive = false;
        champagneSpawnCount = 0;
        _pendingGameWin = false;
        activePassportSuit = -1;
        passportTimer = 0f;
        isNatureReserveActive = false;
        isLastGaspGoalActive = false;
        lastGaspGoalBonus = 0;
        isBloomingOnKongActive = false;
        isSubspaceActive = false;
        isBottomMoonActive = false;
        isLastStandActive = false;
        cumulativeHuSpeedBonus = 0;
        isOneManArmyActive = false;
        isMistActive = false;
        _itemsUsedThisGame = 0;
        _protocolsObtainedThisGame = 0;
        _loopOfLastLegendaryAppearance = 0;
        if (gameUI != null) gameUI.SetMistActive(false);
        _omaCurrentGrowth = 2f;
        _omaAppliedFactor = 1f;
        _currentRefreshCost = settings.refreshBaseCost;
        gameUI.UpdateLoopProgressText(scoreManager.GetLoopProgressString());
        _currentGameOverReason = "";
        remainingTime = settings.initialTimeLimit;
        currentScoreLevelIndex = 0;
        isEndlessMode = false;

        tetrisGrid.ClearAllBlocks();
        huPaiArea.ClearAll();
        scoreManager.ResetScore();
        inventoryManager.ClearInventory();
        protocolsMarkedForRemoval.Clear();
        GrantStartingRewards(DifficultyManager.Instance.CurrentDifficulty);
        if (isAdventFoodActive) { adventFoodBonus = 60; adventFoodTimer = 1f; }
        else { adventFoodBonus = 0; }

        // 4. 不稳定电流：初始化计时器
        if (isUnstableCurrentActive) unstableCurrentTimer = 6f;
        UpdateCurrentBaseScore();
        // 【修复】必须先计算速度，再生成方块
        blockPool.ResetFullDeck();
        UpdateFallSpeed();

        // 使用会话配置中的数据来初始化
        spawner.InitializeForNewGame(settings, currentSessionConfig.InitialTetrominoes);

        UpdateActiveBlockListUI();
        isProcessingRows = false;
        gameUI.HideAllPanels();
        gameUI.SetEndlessModeLabelActive(false);
        UpdateTargetScoreUI();
        if (currentSessionConfig != null && currentSessionConfig.DifficultyScoreLevels != null)
        {
            int total = currentSessionConfig.DifficultyScoreLevels.Count;
            // currentScoreLevelIndex 初始为 0，所以显示 1
            gameUI.UpdateLevelProgress(currentScoreLevelIndex + 1, total);
        }
        gameUI.UpdateBaseScoreText(baseFanScore);
        gameUI.UpdateExtraMultiplierText(extraMultiplier);
        gameUI.UpdateLoopProgressText(scoreManager.GetLoopProgressString());
        if (gameUI != null)
        {
            gameUI.UpdateProtocolUI(activeProtocols);
        }
        CheckAndShowTutorial();
    }

    private void HandleHuDeclared(List<List<int>> huHand)
    {
        float finalExtraMult = extraMultiplier;
        _snapshotSSSVIP = isSSSVIPActive;
        _snapshotStrongWorld = isStrongWorldActive;
        isProcessingRows = true;
        Time.timeScale = 0f;
        if (AudioManager.Instance) AudioManager.Instance.PauseCountdownSound();
        // 每次胡牌后更新圈数显示
        gameUI.UpdateLoopProgressText(scoreManager.GetLoopProgressString());
        if (isBottomMoonActive)
        {
            // 1. 获取当前牌库剩余的牌数
            int remainingTiles = blockPool.GetAvailableBlockIDs().Count;

            // 2. 获取下一个方块需要的牌数 (例如 Lv3方块可能需要5张)
            int neededForNext = spawner.GetNextBlockRequiredTileCount();

            // 3. 动态判断：如果剩余 < 需要，即判定为“牌库不足/会出现黑块”
            if (neededForNext > 0 && remainingTiles < neededForNext)
            {
                ApplyRoundBaseScoreBonus(36);
                Debug.Log($"海底捞月触发！剩余 {remainingTiles} 张 < 下个方块需要 {neededForNext} 张。基础分 +36");
            }
        }

        bool isAdvancedReward = scoreManager.IncrementHuCountAndCheckCycle();
        if (isOneManArmyActive && isAdvancedReward)
        {
            // 只有当条约处于“生效状态”且“完成了一圈”时，倍率才翻倍
            // 我们调用 Recalculate 方法，并传入 true 表示这是一次成长事件
            RecalculateOneManArmy(true);
        }
        // 1. 基础番数计算
        var analysisResult = mahjongCore.CalculateHandFan(huHand, settings, _tempIsTianHu, _tempIsDiHu);
        if (AchievementManager.Instance != null)
        {
            // 解析牌型名称字符串，例如 "清一色 · 对对" -> ["清一色", "对对"]
            List<string> patterns = new List<string>();
            if (!string.IsNullOrEmpty(analysisResult.PatternName))
            {
                string[] split = analysisResult.PatternName.Split(new string[] { " · " }, System.StringSplitOptions.None);
                patterns.AddRange(split);
            }
            int currentLoop = scoreManager.GetCurrentLoop();
            int completedLoops = Mathf.Max(0, currentLoop - 1);
            AchievementManager.Instance.CheckOnHu(
            analysisResult.TotalFan,
            patterns,
            completedLoops,
            activeProtocols.Count
            );
        }
        if (isBloomingOnKongActive)
        {
            int kongCount = huHand.Count(set => set.Count == 4);
            if (kongCount > 0)
            {
                // MahjongCore 按默认设置算了 (比如1番/杠)，我们这里手动补上额外的 1番/杠
                analysisResult.TotalFan += kongCount * 1;
            }
        }
        // 使用完后重置临时变量
        _tempIsTianHu = false;
        _tempIsDiHu = false;


        // 3. 【修复】麻将博士 (DrMahjong)
        // 依赖修正后的 PatternName
        if (isDrMahjongActive)
        {
            // 如果是平胡 (且未被混淆视听改为清一色)，倍率为0
            if (analysisResult.PatternName == "HU_TYPE_PING" && !isOldSchoolActive)
            {
                finalExtraMult *= 0f;
            }
            else
            {
                // 其他牌型，底数变3
                analysisResult.BaseMultiplier = 3f;
                // FanMultiplier 是属性 (getter)，会自动读取新的 BaseMultiplier 进行计算，无需手动重算
            }
        }

        // 4. 【修复】老派玩家 (OldSchool)
        // 强制覆盖为平胡，但此时已过了麻将博士的检查
        // 需求：老派玩家虽然显示平胡，但要按 10 * 2^行数 计分。
        // 这里只是处理胡牌倍率。根据规则，老派玩家胡牌视为平胡(1番)。
        if (isOldSchoolActive)
        {
            analysisResult.PatternName = "平胡";
            analysisResult.TotalFan = 1; // 强制1番
        }

        double scorePart = baseFanScore * analysisResult.FanMultiplier;
        long finalScore = (long)(scorePart * blockMultiplier * finalExtraMult);
        scoreManager.AddScore(finalScore);

        float addedTime = 0f;

        // 【修复】时间就是金钱 (TimeIsMoney)
        // 只有在未激活该条约时，才奖励时间
        if (!isTimeIsMoneyActive)
        {
            // 1. 加上基础时间 (通常是 60秒)
            addedTime += settings.huTimeBonus;

            // 2. 如果有新能源，额外 +20秒
            if (isRenewableEnergyActive)
            {
                addedTime += 20f;
            }

            // 3. 真正把时间加到游戏里！(之前这行代码丢了)
            remainingTime += addedTime;

            // 刷新 UI
            gameUI.UpdateLoopProgressText(scoreManager.GetLoopProgressString());
        }
        // 计算下一轮速度增加值 (仅用于显示)
        // 这里的逻辑是：每一轮胡牌，速度等级增加 settings.speedIncreasePerHu_Int
        int perHuSpeed = settings.speedIncreasePerHu_Int;
        if (isSubspaceActive)
        {
            perHuSpeed += 2;
        }
        cumulativeHuSpeedBonus += perHuSpeed;
        int speedIncrease = perHuSpeed;

        ProcessPendingProtocolRemovals();
        _tempLastHandMult = finalExtraMult;
        var rewards = GenerateHuRewards(isAdvancedReward, finalExtraMult);

        // 狂战士 (Berserker) 逻辑
        int autoBlockIndex = -1;
        int autoItemIndex = -1;
        int autoProtocolIndex = -1;
        if (isBerserkerActive)
        {
            // 后台自动随机选择索引
            if (rewards.BlockChoices.Count > 0) autoBlockIndex = Random.Range(0, rewards.BlockChoices.Count);
            if (rewards.ItemChoices.Count > 0) autoItemIndex = Random.Range(0, rewards.ItemChoices.Count);
            if (rewards.ProtocolChoices.Count > 0) autoProtocolIndex = Random.Range(0, rewards.ProtocolChoices.Count);

            // 立即应用效果
            if (autoBlockIndex != -1) spawner.AddTetrominoToPool(rewards.BlockChoices[autoBlockIndex]);
            if (autoItemIndex != -1) inventoryManager.AddItem(rewards.ItemChoices[autoItemIndex]);
            if (autoProtocolIndex != -1) AddProtocol(rewards.ProtocolChoices[autoProtocolIndex]);
        }
        _currentRefreshCost = settings.refreshBaseCost;
        gameUI.ShowHuPopup(
            huHand,
            analysisResult,
            baseFanScore,
            blockMultiplier,
            finalExtraMult,
            finalScore,
            rewards,
            isAdvancedReward,
            isBerserkerActive,
            addedTime,      // <--- 传入正确的时间 (如 60)
            speedIncrease,  // <--- 传入正确的速度 (如 2)
            autoBlockIndex, // <--- 这里的 -1 才是正确的 autoIdx
            autoItemIndex,
            autoProtocolIndex
        );
    }
    public void MarkProtocolForRemoval(ProtocolData protocol)
    {
        if (!protocolsMarkedForRemoval.Contains(protocol))
        {
            protocolsMarkedForRemoval.Add(protocol);
            Debug.Log($"条约 [{protocol.protocolName}] 已标记为待删除，将在下次胡牌后移除。");
        }
    }

    // 【新增】检查条约是否已标记（用于UI刷新）
    public bool IsProtocolMarkedForRemoval(ProtocolData protocol)
    {
        return protocolsMarkedForRemoval.Contains(protocol);
    }
    public void ContinueAfterHu()
    {
        delayGratificationBonus = 0;
        kidsMealTimer = 0f;
        hasReviveStone = false;
        if (isGreatRevolutionActive) spawner.RandomizeActivePool();
        if (isBerserkerActive) inventoryManager.UseAllItems();
        UpdateCurrentBaseScore();
        midasTimer = 0f; midasGoldValue = 0; scoreboardTimer = 0f;
        ignoreMahjongCheckCount = 0;
        roundSpeedBonus = 0; countedSpeedBonus = 0; countedBonusBlocksRemaining = 0;
        isFilterActive = false; filterTimer = 0f; luckyCapStack = 0;

        if (isSteroidReversalActive) { roundBaseScoreBonus = 0; isSteroidReversalActive = false; }
        else if (isSteroidActive) { roundBaseScoreBonus = -16; isSteroidActive = false; isSteroidReversalActive = true; }
        else { roundBaseScoreBonus = 0; }

        if (isAdventFoodActive) { adventFoodBonus = 60; adventFoodTimer = 1f; } else { adventFoodBonus = 0; }
        UpdateCurrentBaseScore();

        if (isRoutineWorkActive) remainingTime = 95f;
        if (!isTimeIsMoneyActive)
        {
            if (isRoutineWorkActive)
            {
                remainingTime = 95f;
                if (isRenewableEnergyActive)
                {
                    remainingTime += 20f;
                }
            }
        }
        if (isUnstableCurrentActive) unstableCurrentTimer = 6f;
        isChampagneActive = false;
        champagneSpawnCount = 0;
        activePassportSuit = -1;
        // 【核心修复 2】重置子弹时间状态记录
        // 这样新一轮开始时，系统会重新检测高度，不会沿用上一轮的 true 状态
        _lastBulletTimeState = false;
        _hasDeclaredHuThisFrame = false;

        if (_snapshotStrongWorld) spawner.AddRandomLevel3Block();

        UpdateCurrentBaseScore();
        isPaused = false;

        gameUI.PlayHuPopupExitAnimation(() =>
        {
            // ==========================================
            // 【核心修复】逻辑顺序调整
            // ==========================================
            _gameExecutionId++;
            // 1. 【第一优先】检查是否达成胜利 (且非无尽模式)
            // 如果赢了，直接阻断后续的"恢复游戏"逻辑，保持时间静止，弹出胜利窗口
            if (!isEndlessMode && _pendingGameWin)
            {
                _pendingGameWin = false; // 重置标记

                // 此时 Time.timeScale 依然是 0 (从胡牌状态继承过来的)
                // 我们直接处理胜利，不进行任何重置或生成
                HandleGameWon();

                return; // 【关键】直接退出，绝不执行下面的代码
            }

            // 如果没赢，或者已经是无尽模式，才执行下面的重置逻辑
            _pendingGameWin = false;

            // ==========================================
            // 2. 只有确定继续游戏了，才重置状态
            // ==========================================

            // 重置暂停状态
            isPaused = false;

            // 重置各种变量 (保持原有逻辑)
            delayGratificationBonus = 0;
            if (isGreatRevolutionActive) spawner.RandomizeActivePool();
            if (isBerserkerActive) inventoryManager.UseAllItems();

            midasTimer = 0f; midasGoldValue = 0; scoreboardTimer = 0f;
            ignoreMahjongCheckCount = 0;
            roundSpeedBonus = 0; countedSpeedBonus = 0; countedBonusBlocksRemaining = 0;
            isFilterActive = false; filterTimer = 0f; luckyCapStack = 0;

            if (isAdventFoodActive) { adventFoodBonus = 60; adventFoodTimer = 1f; } else { adventFoodBonus = 0; }

            UpdateCurrentBaseScore();

            if (isUnstableCurrentActive) unstableCurrentTimer = 6f;
            _isBombOrSpecialClear = false;
            _lastBulletTimeState = false;
            _hasDeclaredHuThisFrame = false;

            if (_snapshotStrongWorld) spawner.AddRandomLevel3Block();

            UpdateCurrentBaseScore();

            // ==========================================
            // 3. 恢复游戏核心系统
            // ==========================================

            // 恢复时间
            Time.timeScale = 1f;
            if (AudioManager.Instance) AudioManager.Instance.ResumeCountdownSound();

            // 移除到期条约
            ProcessPendingProtocolRemovals();

            // 重置网格与牌库
            hasClearedRowsInThisRound = false;
            blockPool.ResetFullDeck();

            // 清理场面 (这会清除上一轮剩下的牌)
            tetrisGrid.ClearAllBlocks();
            CheckAndApplyBulletTime();
            huPaiArea.ClearAll();

            if (remainingTime <= 0.1f && !isTimeIsMoneyActive) remainingTime = 1f;

            // 计算速度并生成新方块
            UpdateFallSpeed();
            spawner.StartNextRound();
            gameUI.UpdateLoopProgressText(scoreManager.GetLoopProgressString());
            isProcessingRows = false;
        });
    }

    private void HandleRowsCleared(List<int> rowIndices)
    {
        if (isProcessingRows) return;

        // 开启协程处理动画和逻辑
        StartCoroutine(ProcessRowsClearedRoutine(rowIndices, _gameExecutionId));
    }

    // 【新增】协程处理消行逻辑
    // 【核心修复】拆分逻辑为三阶段，绕过 C# yield 限制，并确保 Finally 必执行
    // 【核心修复】拆分逻辑为三阶段，绕过 C# yield 限制，并确保 Finally 必执行
    private IEnumerator ProcessRowsClearedRoutine(List<int> rowIndices, int capturedExecutionId)
    {
        isProcessingRows = true;
        _hasDeclaredHuThisFrame = false;
        rowIndices.Sort();

        List<Transform> allClearedTransforms = new List<Transform>();
        List<int> finalIdsToReturn = new List<int>();
        HashSet<Transform> specialTransforms = new HashSet<Transform>();
        HashSet<Transform> pairTransforms = new HashSet<Transform>();

        // 暂存胡牌数据 (用于延迟弹窗)
        List<List<int>> pendingHuHand = null;
        List<int> pendingPairIds = null;
        List<int> trashedByItemIds = new List<int>();
        bool logicSuccess = false;
        Tetromino activeTetromino = null;
        var allTetrominos = FindObjectsOfType<Tetromino>();
        foreach (var t in allTetrominos)
        {
            if (t.enabled) { activeTetromino = t; break; }
        }
        Transform ignoreTransform = activeTetromino != null ? activeTetromino.transform : null;
        try
        {
            if (_gameExecutionId != capturedExecutionId) yield break;

            List<List<int>> rowsBlockIds = new List<List<int>>();

            // 获取数据
            foreach (var y in rowIndices)
            {
                if (tetrisGrid == null) break;

                // 【关键】传入 ignoreTransform
                var rowData = tetrisGrid.GetRowDataAndClear(y, ignoreTransform);

                // 只有当这一行真的清除了东西才记录 (防止全是下落方块导致空行)
                if (rowData.blockIds.Count > 0)
                {
                    allClearedTransforms.AddRange(rowData.transforms);
                    rowsBlockIds.Add(rowData.blockIds);
                    ApplyRowClearRewards();
                }
            }

            bool wasCleanRound = !hasClearedRowsInThisRound;
            hasClearedRowsInThisRound = true;
            if (!isOldSchoolActive && rowIndices.Count > 0)
            {
                int defaultScore = rowIndices.Count * 10;
                scoreManager.AddScore(defaultScore);
            }
            if (isOldSchoolActive && rowIndices.Count > 0)
            {
                int lineCount = rowIndices.Count;

                // 使用位移运算计算 2^N (比 Pow 更高效且精确)
                // 1 << 4 等于 16
                long multiplier = 1L << lineCount;

                long clearScore = baseFanScore * multiplier;

                // 加分 (确保不溢出)
                scoreManager.AddScore(clearScore);

                // Log 方便调试
                // Debug.Log($"老派玩家消行: {lineCount} 行, 得分: {clearScore}");
            }
            if (isDelayGratificationActive && rowIndices.Count >= 3)
            {
                delayGratificationBonus += 7;
                UpdateCurrentBaseScore(); // 立即刷新 UI 显示

                // Debug.Log($"延迟满足生效：一次性消除 {rowIndices.Count} 行，基础分 +7！");
            }

            // 【移除】移除了之前错误的 TryFindAndAddRandomSetFromPool 调用

            bool isHandEmptyAtStart = huPaiArea.GetSetCount() == 0;

            // ==========================================================
            // 麻将判定逻辑 (根据是否激活“合纵连横”分流)
            // ==========================================================

            // 1. 先处理“垃圾桶”道具 (忽略判定行数)
            // 无论是否合纵连横，都要先剔除被忽略的行
            int rowsTotal = rowsBlockIds.Count;
            int rowsToRemove = (ignoreMahjongCheckCount > 0) ? Mathf.Min(rowsTotal, ignoreMahjongCheckCount) : 0;
            ignoreMahjongCheckCount -= rowsToRemove;

            // 将被忽略的行直接加入回收列表
            for (int i = 0; i < rowsToRemove; i++)
            {
                trashedByItemIds.AddRange(rowsBlockIds[i]);

                Debug.Log($"垃圾桶生效：第 {i + 1} 行被移除游戏 (不回牌库)。");
            }

            // 2. 处理剩余的行
            if (isRealpolitikActive)
            {
                // === 方案 A: 合纵连横 (合并判定) ===
                // 将剩余所有行的方块合并到一个大池子里
                List<int> combinedTileIds = new List<int>();

                for (int i = rowsToRemove; i < rowsTotal; i++)
                {
                    // 如果中途已经胡牌了，剩下的直接回收
                    if (_hasDeclaredHuThisFrame)
                    {
                        finalIdsToReturn.AddRange(rowsBlockIds[i]);
                        continue;
                    }
                    combinedTileIds.AddRange(rowsBlockIds[i]);
                }

                // 对合并后的大列表进行一次性判定
                if (combinedTileIds.Count > 0 && !_hasDeclaredHuThisFrame)
                {
                    ProcessMahjongDetection(combinedTileIds, ref finalIdsToReturn, allClearedTransforms, wasCleanRound, ref pendingHuHand, ref pendingPairIds, isHandEmptyAtStart);
                }
            }
            else
            {
                // === 方案 B: 默认逻辑 (逐行判定) ===
                for (int i = rowsToRemove; i < rowsTotal; i++)
                {
                    if (_hasDeclaredHuThisFrame)
                    {
                        finalIdsToReturn.AddRange(rowsBlockIds[i]);
                        continue;
                    }

                    ProcessMahjongDetection(rowsBlockIds[i], ref finalIdsToReturn, allClearedTransforms, wasCleanRound, ref pendingHuHand, ref pendingPairIds, isHandEmptyAtStart);
                }
            }

            // --- 筛选方块类型 (垃圾/牌组/将牌) ---
            List<int> trashIdsCopy = new List<int>(finalIdsToReturn);
            trashIdsCopy.AddRange(trashedByItemIds);
            List<int> pairIdsCopy = (pendingPairIds != null) ? new List<int>(pendingPairIds) : new List<int>();

            foreach (var t in allClearedTransforms)
            {
                if (t == null) continue;
                var unit = t.GetComponent<BlockUnit>();
                if (unit == null) continue;

                int id = unit.blockId;

                if (pairIdsCopy.Contains(id))
                {
                    pairIdsCopy.Remove(id);
                    pairTransforms.Add(t);
                }
                else if (trashIdsCopy.Contains(id))
                {
                    trashIdsCopy.Remove(id);
                }
                else
                {
                    specialTransforms.Add(t);
                }
            }

            logicSuccess = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Logic Error] {e.Message}\n{e.StackTrace}");
        }

        // ... (后续动画和清理代码保持不变) ...

        // ==========================================================
        // 2. 动画阶段
        // ==========================================================
        if (logicSuccess)
        {
            if (_gameExecutionId != capturedExecutionId) yield break;

            if (tetrisGrid != null)
            {
                yield return StartCoroutine(tetrisGrid.AnimateRowsClear(allClearedTransforms, specialTransforms, pairTransforms, settings.rowClearAnimationDuration));
            }
        }

        // ==========================================================
        // 3. 收尾阶段
        // ==========================================================
        try
        {
            if (_gameExecutionId != capturedExecutionId) yield break;

            if (logicSuccess)
            {
                // 1. 如果胡牌了，触发事件
                if (pendingHuHand != null)
                {
                    GameEvents.TriggerHuDeclared(pendingHuHand);
                    _isBombOrSpecialClear = true;
                    yield break;
                }

                // 2. 没胡牌，常规清理
                if (!_hasDeclaredHuThisFrame)
                {
                    if (!isLastStandActive && blockPool != null) blockPool.ReturnBlockIds(finalIdsToReturn);
                    if (tetrisGrid != null) tetrisGrid.DestroyTransforms(allClearedTransforms);
                }

                if (tetrisGrid != null)
                {
                    tetrisGrid.CompactAllColumns(rowIndices, ignoreTransform);
                }

                if (!_hasDeclaredHuThisFrame)
                {
                    if (spawner != null) spawner.RefreshPreviewUI();
                    CheckAndApplyBulletTime();
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Cleanup Error] {e.Message}\n{e.StackTrace}");
        }
        finally
        {
            if (_gameExecutionId == capturedExecutionId)
            {
                if (!_isBombOrSpecialClear && !_hasDeclaredHuThisFrame)
                {
                    if (spawner != null) spawner.SpawnBlock();
                }

                _isBombOrSpecialClear = false;
                if (!_hasDeclaredHuThisFrame) isProcessingRows = false;
            }
        }
    }

    // 【辅助方法】封装麻将判定
    private void ProcessMahjongDetection(
        List<int> tileIds,
        ref List<int> idsToReturn,
        List<Transform> transformsToDestroy,
        bool wasCleanRound,
        ref List<List<int>> outPendingHand,
        ref List<int> outPendingPair,
        bool isHandEmptyAtStart) // <--- 参数在这里
    {
        if (_hasDeclaredHuThisFrame) return;

        var result = mahjongCore.DetectSets(tileIds);

        // 三位一体
        if (isTrinityActive)
        {
            int setBonus = (result.Pungs.Count + result.Kongs.Count) * 5;
            if (setBonus > 0) ApplyPermanentBaseScoreBonus(setBonus);
        }

        var setsToAdd = new List<List<int>>();
        setsToAdd.AddRange(result.Kongs); setsToAdd.AddRange(result.Pungs); setsToAdd.AddRange(result.Chows);

        int needed = settings.setsForHu - huPaiArea.GetSetCount();
        if (setsToAdd.Count > needed)
        {
            var shuffled = setsToAdd.OrderBy(a => Random.value).ToList();
            var chosen = shuffled.Take(needed).ToList();
            result.RemainingIds.AddRange(shuffled.Skip(needed).SelectMany(set => set));
            setsToAdd = chosen;
        }

        if (setsToAdd.Count > 0)
        {
            huPaiArea.AddSets(setsToAdd, 0.5f);
        }

        // 检查是否胡牌
        if (huPaiArea.GetSetCount() >= settings.setsForHu)
        {
            var pair = mahjongCore.FindPair(result.RemainingIds);
            if (pair != null)
            {
                // --- 发现胡牌 ---
                _hasDeclaredHuThisFrame = true;

                result.RemainingIds.Remove(pair[0]);
                result.RemainingIds.Remove(pair[1]);
                if (isNatureReserveActive)
                {
                    foreach (var id in pair)
                    {
                        // 逻辑与 OnHuPaiTileAdded 保持一致：检查是否为 ID 0 (一条/一筒)
                        if (id % 27 == 0)
                        {
                            int bonus = 6;
                            ApplyRoundBaseScoreBonus(bonus);
                            Debug.Log("自然保护区生效：将牌包含目标牌，基础分 +6");
                        }
                    }
                }
                var finalHand = huPaiArea.GetAllSets();
                finalHand.Add(pair);

                // 【修复】直接使用传入的参数 isHandEmptyAtStart，不再使用不存在的 isHuPaiAreaEmpty
                bool isTianHu = isHandEmptyAtStart && wasCleanRound;
                bool isDiHu = isHandEmptyAtStart && !wasCleanRound;

                this._tempIsTianHu = isTianHu;
                this._tempIsDiHu = isDiHu;

                // 输出数据
                outPendingHand = finalHand;
                outPendingPair = pair;

                idsToReturn.AddRange(result.RemainingIds);
                return;
            }
        }

        // 没胡，所有剩余ID都是垃圾
        idsToReturn.AddRange(result.RemainingIds);
    }

    // 【辅助方法】避免代码重复
    private void ApplyRowClearRewards()
    {
        if (midasTimer > 0)
        {
            GameSession.Instance.AddGold(midasGoldValue);
            if (AchievementManager.Instance != null)
            {
                AchievementManager.Instance.AddProgress(AchievementType.AccumulateGold, midasGoldValue);
            }
            midasGoldValue *= 2;
        }
        if (scoreboardTimer > 0)
        {
            long currentScore = scoreManager.GetCurrentScore();
            long bonusScore = (long)(currentScore * 0.05d);
            scoreManager.AddScore(bonusScore);
        }
    }
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

        // 【修复】确保当前会话配置存在，且索引有效
        if (currentSessionConfig != null &&
            currentSessionConfig.DifficultyScoreLevels != null &&
            currentScoreLevelIndex < currentSessionConfig.DifficultyScoreLevels.Count)
        {
            // 1. 获取运行时关卡数据 (而不是 settings)
            var level = currentSessionConfig.DifficultyScoreLevels[currentScoreLevelIndex];

            // 2. 修改目标分
            int oldTarget = level.targetScore;
            level.targetScore = (int)(level.targetScore * multiplier);

            // 防止变为0或负数，至少保留1分
            if (level.targetScore < 1) level.targetScore = 1;

            Debug.Log($"优惠券生效：目标分从 {oldTarget} 降低至 {level.targetScore}");

            // 3. 立即刷新 UI (现在读取的是正确的数据了，UI会变)
            UpdateTargetScoreUI();

            // 4. 【关键】立即检查当前分数是否已经达标
            // 调用 OnScoreUpdated 传入当前分，复用已有的"达标-发奖-升级"逻辑
            if (scoreManager != null)
            {
                OnScoreUpdated(scoreManager.GetCurrentScore());
            }
        }
    }

    public void AddProtocol(ProtocolData protocol)
    {
        if (activeProtocols.Count < settings.maxProtocolCount && !activeProtocols.Contains(protocol))
        {
            activeProtocols.Add(protocol);
            _protocolsObtainedThisGame++;
            if (AchievementManager.Instance != null)
            {
                // 累计获得条约总数 (No.42)
                AchievementManager.Instance.AddProgress(AchievementType.AccumulateProtocolGet, 1);

                // 传奇物品统计 (No.43 - 条约也算传奇奖励)
                if (protocol.isLegendary)
                {
                    AchievementManager.Instance.AddProgress(AchievementType.AccumulateLegendary, 1);
                }
                AchievementManager.Instance.CheckSingleGameRealtimeStats(_itemsUsedThisGame, _protocolsObtainedThisGame);
            }
            protocol.ApplyEffect(this);
            // 更新条约栏UI
            gameUI.UpdateProtocolUI(activeProtocols);

            // 【关键修复】如果新条约是“时间就是金钱”，立即刷新金币目标UI
            // 或者更通用一点：只要添加了条约就刷新一次 UI，防止有其他影响数值的条约
            UpdateTargetScoreUI();
            RecalculateOneManArmy();
        }
        gameUI.UpdateLoopProgressText(scoreManager.GetLoopProgressString());
        UpdateFallSpeed();

        AchievementManager.Instance.AddProgress(AchievementType.AccumulateProtocolGet, 1);
    }
    public void RemoveProtocolImmediately(ProtocolData protocol)
    {
        if (activeProtocols.Contains(protocol))
        {
            // 1. 移除效果 (数值还原)
            protocol.RemoveEffect(this);

            // 2. 从列表中移除
            activeProtocols.Remove(protocol);

            // 3. 立即刷新 UI (重新绘制槽位)
            if (gameUI != null)
            {
                gameUI.UpdateProtocolUI(activeProtocols);
            }

            // 4. 触发必要的数值刷新
            UpdateFallSpeed();
            gameUI.UpdateLoopProgressText(scoreManager.GetLoopProgressString());
            RecalculateOneManArmy();
            UpdateTargetScoreUI();
        }
    }
    public void RecalculateBlockMultiplier()
    {
        // 【修复】从永久修正值开始计算，而不是从 0 开始
        blockMultiplier = permanentBlockMultiplierModifier;

        if (spawner.GetActivePrefabs() != null)
        {
            foreach (var prefab in spawner.GetActivePrefabs())
            {
                // 【修改】人人平等逻辑
                if (isAllMenEqualActive)
                {
                    blockMultiplier += 3f;
                }
                else
                {
                    blockMultiplier += prefab.GetComponent<Tetromino>().extraMultiplier;
                }
            }
        }

        if (blockMultiplier < 1f) blockMultiplier = 1f;

        gameUI.UpdateBlockMultiplierText(blockMultiplier);
    }
    public void UpdateActiveBlockListUI()
    {
        // 1. 先计算全局生效倍率 (更新 HUD 顶部文本)
        RecalculateBlockMultiplier();

        var prefabs = spawner.GetActivePrefabs();

        // 2. 【核心修改】计算“方块倍率之和”
        // 这里需要体现“人人平等”对基础方块数值的改变
        float rawBlockSum = 0f;
        if (prefabs != null)
        {
            foreach (var p in prefabs)
            {
                // 特殊情况：人人平等条约
                // 该条约强制将所有方块视为 3倍，所以这里累加 3
                if (isAllMenEqualActive)
                {
                    rawBlockSum += 3f;
                }
                else
                {
                    // 正常情况：累加方块自身的倍率
                    var t = p.GetComponent<Tetromino>();
                    if (t != null)
                    {
                        rawBlockSum += t.extraMultiplier;
                    }
                }
            }
        }

        // 3. 准备人人平等的显示覆盖值 (影响列表项单项显示)
        float overrideMult = isAllMenEqualActive ? 3f : -1f;

        if (gameUI != null)
        {
            // 传入修正后的 rawBlockSum
            gameUI.UpdateTetrominoList(prefabs, rawBlockSum, overrideMult);
        }
    }
    public void ApplyBlockMultiplierModifier(float amount)
    {
        // 【修复】不再直接修改 blockMultiplier，而是修改永久修正值
        permanentBlockMultiplierModifier += amount;
        RecalculateBlockMultiplier(); // 立即重新计算
    }

    public void ApplyExtraMultiplier(float factor)
    {
        extraMultiplier *= factor;
        gameUI.UpdateExtraMultiplierText(extraMultiplier);
    }

    public void ForceClearRowsFromBottom(int count)
    {
        // 查找当前正在控制的方块 (enabled == true 的那个)
        Tetromino activeTetromino = null;
        var allTetrominos = FindObjectsOfType<Tetromino>();
        foreach (var t in allTetrominos)
        {
            if (t.enabled) { activeTetromino = t; break; }
        }

        Transform ignoreTransform = activeTetromino != null ? activeTetromino.transform : null;

        // 传入忽略目标
        bool hasCleared = tetrisGrid.ForceClearBottomRows(count, ignoreTransform);

        if (hasCleared)
        {
            _isBombOrSpecialClear = true;
        }
        else
        {
            Debug.Log("强制底部消除：没有可消除的行(已忽略下落方块)，跳过。");
        }
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
        // 1. 处理喷气背包计数
        if (countedBonusBlocksRemaining > 0)
        {
            countedBonusBlocksRemaining--;
            if (countedBonusBlocksRemaining == 0)
            {
                countedSpeedBonus = 0;
                UpdateFallSpeed();
            }
        }
        if (isChampagneActive)
        {
            champagneSpawnCount++;

            // 逻辑推演：
            // 0: 使用道具 (当前方块A正在下落)
            // 1: 方块B生成 (Notify触发)
            // 2: 方块C生成 (Notify触发) -> 这就是“下下个方块”

            if (champagneSpawnCount == 2)
            {
                // 到了预定回合，基础分 +10
                // 这会实时更新UI，玩家能看到分数变多了
                ApplyRoundBaseScoreBonus(10);
            }
            else if (champagneSpawnCount == 3)
            {
                // 过了预定回合还没胡 (方块D生成了)，把分扣回去
                ApplyRoundBaseScoreBonus(-10);
                isChampagneActive = false;
            }
        }
        // 2. 【关键修复】在方块生成时，检测一次子弹时间状态
        // 此时检测的是底部堆积的静态高度，不会被正在下落的方块干扰
        CheckAndApplyBulletTime();
    }
    // --- 私有辅助方法 ---
    private HuRewardPackage GenerateHuRewards(bool isAdvanced, float currentExtraMult)
    {
        // 【新增】麻将博士：如果是平胡（extraMultiplier被置为0），则无法获得任何奖励
        if (isDrMahjongActive && currentExtraMult == 0)
        {
            return new HuRewardPackage(); // 返回空包
        }

        var package = new HuRewardPackage();

        // 【新增】独木桥：每种奖励数量变为1个
        // 如果是独木桥模式，数量固定为1；否则按原有逻辑（高级5/2/2，普通3/3）
        int blockCount = isLogBridgeActive ? 1 : (isAdvanced ? 5 : 3);
        int itemCount = isLogBridgeActive ? 1 : 3;
        int protocolCount = isLogBridgeActive ? 1 : 3;

        // 生成方块奖励
        package.BlockChoices = GetWeightedRandomBlocks(blockCount, isAdvanced ? settings.advancedBlockRewardWeights : settings.commonBlockRewardWeights).ToList();

        if (isAdvanced)
        {
            // 【修改】高级道具池筛选：测试模式 或 已解锁
            var unlockedItems = settings.advancedItemPool
                .Where(i => settings.isTestMode || SaveManager.IsItemUnlocked(i.itemName, i.isInitial))
                .ToList();

            // 使用加权随机
            package.ItemChoices = GetWeightedRandomList(unlockedItems, itemCount);

            // 【修改】条约池筛选：排除已激活 && (测试模式 或 已解锁)
            var availableProtocols = settings.protocolPool
                .Except(activeProtocols)
                .Where(p => settings.isTestMode || SaveManager.IsProtocolUnlocked(p.protocolName, p.isInitial))
                .ToList();

            package.ProtocolChoices = GetWeightedRandomList(availableProtocols, protocolCount);
        }
        else
        {
            // 【修改】普通道具池筛选：测试模式 或 已解锁
            var unlockedItems = settings.commonItemPool
                .Where(i => settings.isTestMode || SaveManager.IsItemUnlocked(i.itemName, i.isInitial))
                .ToList();

            // 使用加权随机
            package.ItemChoices = GetWeightedRandomList(unlockedItems, itemCount);
        }
        bool hasLegendary = false;

        if (package.ItemChoices != null)
        {
            foreach (var item in package.ItemChoices)
            {
                if (item != null && item.isLegendary) { hasLegendary = true; break; }
            }
        }

        if (!hasLegendary && package.ProtocolChoices != null)
        {
            foreach (var proto in package.ProtocolChoices)
            {
                if (proto != null && proto.isLegendary) { hasLegendary = true; break; }
            }
        }

        if (hasLegendary)
        {
            _loopOfLastLegendaryAppearance = scoreManager.GetCurrentLoop();
            // Debug.Log($"[概率平衡] 传奇物品出现于第 {_loopOfLastLegendaryAppearance} 圈，概率权重已重置。");
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
        int safeCounter = 0;
        while (result.Count < count && safeCounter < 50)
        {
            safeCounter++;

            GameObject chosenBlock = null;
            float roll = Random.value;

            // 【修改】直接使用传入的参数 weights，不再使用 currentWeights 或 adjustedWeights
            if (roll < weights.level1Weight && level1.Count > 0)
                chosenBlock = level1[Random.Range(0, level1.Count)];
            else if (roll < weights.level1Weight + weights.level2Weight && level2.Count > 0)
                chosenBlock = level2[Random.Range(0, level2.Count)];
            else if (level3.Count > 0)
                chosenBlock = level3[Random.Range(0, level3.Count)];

            // 保底逻辑：如果随机到的等级没方块，降级查找
            if (chosenBlock == null)
            {
                if (level2.Count > 0) chosenBlock = level2[Random.Range(0, level2.Count)];
                else if (level1.Count > 0) chosenBlock = level1[Random.Range(0, level1.Count)];
            }

            // 去重添加
            if (chosenBlock != null && !result.Contains(chosenBlock))
            {
                result.Add(chosenBlock);
            }
        }
        return result;
    }

    private bool IsInLevel(GameObject prefab, int levelIndex)
    {
        if (prefab == null)
        {
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

        return result;
    }
    private void CheckAndApplyBulletTime()
    {
        if (!isBulletTimeActive)
        {
            if (_lastBulletTimeState) // 如果之前是激活的，现在关掉
            {
                _lastBulletTimeState = false;
                UpdateFallSpeed();
            }
            return;
        }

        // 检测高度 (GetMaxColumnHeight 计算的是锁定在网格里的方块)
        bool shouldTrigger = tetrisGrid.GetMaxColumnHeight() > 8;

        if (shouldTrigger != _lastBulletTimeState)
        {
            _lastBulletTimeState = shouldTrigger;
            UpdateFallSpeed(); // 状态改变，刷新速度
        }
    }
    private void UpdateFallSpeed()
    {
        int baseSpeed = settings.baseDisplayedSpeed;
        int baseSpeedWithDifficulty = (int)(baseSpeed * this.difficultySpeedMultiplier);
        int perHuIncrease = settings.speedIncreasePerHu_Int;
        if (isSubspaceActive)
        {
            perHuIncrease += 2; // 亚空间额外 +2
        }
        int huBonus = cumulativeHuSpeedBonus;
        int currentCountedBonus = (countedBonusBlocksRemaining > 0) ? countedSpeedBonus : 0;
        int totalBonus = permanentSpeedBonus + roundSpeedBonus + currentCountedBonus;

        int totalDisplayedSpeed = baseSpeedWithDifficulty + huBonus + totalBonus;
        if (totalDisplayedSpeed < 1) totalDisplayedSpeed = 1;

        currentFallSpeed = 20.0f / totalDisplayedSpeed;

        // 【修复】使用缓存的状态 _lastBulletTimeState
        if (_lastBulletTimeState)
        {
            totalDisplayedSpeed = 5;
            currentFallSpeed = 20.0f / 5.0f; // 4.0
        }
        CurrentDisplaySpeed = totalDisplayedSpeed;
        gameUI.UpdateSpeedText(totalDisplayedSpeed, _lastBulletTimeState);

        // 尝试推送到当前方块 (解决生成瞬间速度不匹配问题)
        var currentTetromino = FindObjectOfType<Tetromino>();
        if (currentTetromino != null)
        {
            currentTetromino.UpdateFallSpeedNow(currentFallSpeed);
        }
    }

    private void OnScoreUpdated(long newScore)
    {
        // 1. 处理悬赏令 (Wanted Poster)
        // 这里的逻辑是：只要分数变化了（通常是因为消除），就检查是否激活了悬赏令
        // 但悬赏令通常作用于"金币奖励"。
        // 我们将其移动到发放奖励的时刻判断，或者保持在这里修改 multiplier
        // 之前的逻辑：isWantedPosterActive -> 奖励翻倍 -> 重置。
        // 这里需要注意：AddGold 是在循环内部调用的。

        if (currentSessionConfig == null || currentSessionConfig.DifficultyScoreLevels == null) return;

        // 【修复】增加索引越界检查，防止无尽模式或通关后报错
        while (currentScoreLevelIndex < currentSessionConfig.DifficultyScoreLevels.Count &&
               newScore >= currentSessionConfig.DifficultyScoreLevels[currentScoreLevelIndex].targetScore)
        {
            // 获取基础奖励
            int baseReward = currentSessionConfig.DifficultyScoreLevels[currentScoreLevelIndex].goldReward;

            int totalMult = GetCurrentGoldMultiplier();
            int finalReward = baseReward * totalMult;

            // 【修复】悬赏令逻辑：在发放前应用倍率
            if (isWantedPosterActive)
            {
                isWantedPosterActive = false;
                wantedPosterGoldMult = 1;
            }

            if (GameSession.Instance != null)
            {
                GameSession.Instance.AddGold(finalReward);
                if (AchievementManager.Instance != null)
                {
                    AchievementManager.Instance.AddProgress(AchievementType.AccumulateGold, finalReward);
                }
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.SoundLibrary.targetReached);
            }

            currentScoreLevelIndex++;
            int total = currentSessionConfig.DifficultyScoreLevels.Count;
            gameUI.UpdateLevelProgress(currentScoreLevelIndex + 1, total);
            // 【修复】如果索引已达到上限（通关），立即停止循环并触发获胜
            if (currentScoreLevelIndex >= currentSessionConfig.DifficultyScoreLevels.Count)
            {
                // 如果当前正在处理胡牌 (Time.timeScale == 0) 或者正在处理消行
                // 我们不立即显示胜利面板，而是推迟到 ContinueAfterHu
                if (Time.timeScale == 0f || isProcessingRows || _hasDeclaredHuThisFrame)
                {
                    _pendingGameWin = true;
                    Debug.Log("达成胜利条件！但当前处于胡牌/暂停中，将推迟显示胜利面板。");
                }
                else
                {
                    // 只有在正常游戏过程中达成，才立即显示
                    HandleGameWon();
                }
                break;
            }

            UpdateTargetScoreUI();
        }

        // 更新进度条
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

            int mult = GetCurrentGoldMultiplier();
            int displayedReward = level.goldReward * mult;

            gameUI.UpdateTargetScoreDisplay(level.targetScore, displayedReward, mult > 1);

            // (此行来自“进度条”功能，保持不变)
            gameUI.UpdateScoreProgress(scoreManager.GetCurrentScore());
        }
    }

    private void HandleGameOver()
    {
        // 【新增】复活石逻辑
        if (hasReviveStone)
        {
            Debug.Log("复活石触发！");
            hasReviveStone = false; // 消耗道具
            remainingTime += reviveAddedTime; // 增加时间

            // 1. 获取胡牌区已有的牌，防止被重置掉
            List<int> excludedIds = new List<int>();
            foreach (var set in huPaiArea.GetAllSets())
            {
                excludedIds.AddRange(set);
            }

            // 2. 重置牌库 (使用我们在第一模块修改过的 ResetFullDeck)
            // 这会将场上的牌对应的ID归还回牌库，但保留胡牌区的牌
            blockPool.ResetFullDeck(excludedIds);

            // 3. 清空场上所有方块
            tetrisGrid.ClearAllBlocks();

            // 4. 【关键修复】重启游戏流程
            isProcessingRows = false; // 解除消除锁定
            Time.timeScale = 1f;      // 恢复时间流动
            spawner.StartNextRound(); // 立即生成新方块继续游戏

            return; // 阻止后续的游戏结束流程
        }

        // 正常游戏结束流程
        Time.timeScale = 0f;
        if (AudioManager.Instance) AudioManager.Instance.StopCountdownSound();
        long finalScore = scoreManager.GetCurrentScore();
        bool isNewHighScore = scoreManager.CheckForNewHighScore(finalScore);
        if (isEndlessMode && AchievementManager.Instance != null)
        {
            // 准备数据
            Difficulty currentDiff = DifficultyManager.Instance.CurrentDifficulty;
            int currentGold = GameSession.Instance != null ? GameSession.Instance.CurrentGold : 0;

            // 使用 CurrentDisplaySpeed 获取准确的速度等级
            int finalSpeed = CurrentDisplaySpeed;

            AchievementManager.Instance.CheckGameWin(
                false,              // isWin = false (因为是 Game Over)
                (int)currentDiff,
                finalSpeed,         // 最终速度
                remainingTime,      // 剩余时间 (通常是0，但在无尽模式可能不重要)
                currentGold,
                finalScore,
                _itemsUsedThisGame,
                _protocolsObtainedThisGame,
                tetrisGrid.GetAllBlocksCount(),
                true                // isEndlessMode = true (开启无尽模式检查权限)
            );
        }
        gameUI.ShowGameEndPanel(false, finalScore, isNewHighScore, _currentGameOverReason);
    }

    public void TogglePause()
    {
        // 如果游戏已结束 (Time.timeScale == 0) 且不是处于暂停状态 (例如在胡牌结算中)，则不允许暂停
        if (Time.timeScale == 0f && !isPaused) return;

        if (isPaused)
        {
            // --- 取消暂停 (继续游戏) ---
            isPaused = false;
            Time.timeScale = 1f;

            // 关闭 UI (不带回调)
            if (gameUI != null) gameUI.ShowPausePanel(false);

            if (AudioManager.Instance) AudioManager.Instance.ResumeCountdownSound();
        }
        else
        {
            // --- 暂停游戏 (无限次) ---
            isPaused = true;
            Time.timeScale = 0f;

            // 打开 UI
            if (gameUI != null) gameUI.ShowPausePanel(true);

            if (AudioManager.Instance) AudioManager.Instance.PauseCountdownSound();
        }
    }
    public void QuitGameFromPause()
    {
        // 1. 逻辑状态重置
        isPaused = false;

        // 2. 触发标准的游戏结束流程
        // 这会调用 HandleGameOver -> 计算分数 -> ShowGameEndPanel
        GameEvents.TriggerGameOver();
    }

    private void HandleGameWon()
    {
        Time.timeScale = 0f;
        isPaused = true;

        // 【新增】通关时触发难度解锁检查
        // 获取当前正在玩的难度
        Difficulty currentDiff = DifficultyManager.Instance.CurrentDifficulty;
        // 尝试解锁下一级
        DifficultyManager.Instance.CompleteDifficulty(currentDiff);

        long finalScore = scoreManager.GetCurrentScore();
        bool isNewHighScore = scoreManager.CheckForNewHighScore(finalScore);
        if (AchievementManager.Instance != null)
        {
            // 获取当前金币 (防止空引用)
            int currentGold = GameSession.Instance != null ? GameSession.Instance.CurrentGold : 0;

            // 下落速度需要转换一下，您脚本里用的是 currentFallSpeed，或者重新计算一下等级
            // 既然成就表里写的是"速度>=15"，我们这里直接传 gameUI 上显示的那个速度值比较准
            // 但 gameUI.speedText 是 UI 文本。
            // 我们可以用 settings.baseDisplayedSpeed 和 difficultySpeedMultiplier 反推，
            // 或者直接用您计算速度时的公式。
            // 这里为了方便，我们直接传一个估算的数值，或者您需要在 UpdateFallSpeed 里把 calculatedSpeed 存个变量。
            // 简单起见，我们暂时传 0，或者您去 UpdateFallSpeed 里把 totalDisplayedSpeed 提升为类成员变量。

            // 为了更严谨，建议您在 UpdateFallSpeed 方法里把 `totalDisplayedSpeed` 存到一个类成员变量里，比如 `currentDisplayedSpeed`。
            // 如果还没做，那您可以先填 0，或者暂时用 currentFallSpeed 反推。

            AchievementManager.Instance.CheckGameWin(
                true,                           // isWin = true
                (int)currentDiff,               // 难度
                CurrentDisplaySpeed,            // 【优化】直接使用 CurrentDisplaySpeed
                remainingTime,
                currentGold,
                finalScore,
                _itemsUsedThisGame,
                _protocolsObtainedThisGame,
                tetrisGrid.GetAllBlocksCount(),
                false                           // isEndlessMode = false (此时还没进无尽模式)
            );
        }
        gameUI.ShowGameEndPanel(true, finalScore, isNewHighScore, "");
    }

    public void StartEndlessMode()
    {
        // 【核心修改】先播放退出动画，再执行逻辑
        gameUI.PlayGameOverExitAnimation(() =>
        {
            isEndlessMode = true;

            // 这里的 HideAllPanels 其实已经在动画回调里处理了隐藏，
            // 但保留它作为双重保险也没问题，或者可以删掉
            gameUI.HideAllPanels();
            gameUI.HideLevelProgress();

            UpdateTargetScoreUI();
            gameUI.SetEndlessModeLabelActive(true);
            if (AudioManager.Instance != null && DifficultyManager.Instance != null)
            {
                // 根据当前难度，重新播放对应的 BGM
                // 因为之前 ShowGameEndPanel 时调用了 StopBGM()，所以这里需要重新 Play
                AudioManager.Instance.PlayGameBGM(DifficultyManager.Instance.CurrentDifficulty);
            }
            // 继续游戏
            ContinueAfterHu();
        });
    }

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
    public void UpdateCurrentBaseScore()
    {
        int defaultScore = settings.baseFanScore;
        // 【新增】延迟满足 (-10)
        int delayPenalty = isDelayGratificationActive ? -10 : 0;

        int addedScore = defaultScore + permanentBaseScoreBonus + roundBaseScoreBonus + adventFoodBonus + delayGratificationBonus + delayPenalty + lastGaspGoalBonus;
        int calculatedScore = (int)(addedScore * permanentBaseScoreMultiplier);

        if (isSteroidReversalActive && calculatedScore < 1) calculatedScore = 1;
        // 【新增】延迟满足最低为1
        if (isDelayGratificationActive && calculatedScore < 1) calculatedScore = 1;

        baseFanScore = calculatedScore;
        gameUI.UpdateBaseScoreText(baseFanScore);

        if (AchievementManager.Instance != null)
        {
            // 【核心修复】
            // 成就目标：同时拥有 >= 20 个方块 (指牌库 Deck 的大小)
            // 原逻辑错误：tetrisGrid.GetAllBlocksCount() (这是场上的格子数)
            // 新逻辑正确：spawner.GetActivePrefabs().Count() (这是当前牌库里的方块总数)

            int currentDeckCount = 0;
            if (spawner != null && spawner.GetActivePrefabs() != null)
            {
                currentDeckCount = spawner.GetActivePrefabs().Count();
            }

            AchievementManager.Instance.CheckRealtimeStats(
                baseFanScore,
                blockMultiplier,
                extraMultiplier,
                currentDeckCount // <--- 传入牌库数量
            );
        }
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
        ignoreMahjongCheckCount += count;

        Debug.Log($"垃圾桶效果叠加：当前将忽略接下来的 {ignoreMahjongCheckCount} 行消除。");
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
    // 【新增】供儿童餐调用
    public void ActivateKidsMeal(float duration)
    {
        kidsMealTimer = duration;
        // 立即刷新下一个方块
        spawner.ForceRerollToLevel(0); // Lv.1
    }

    // 【新增】供复活石调用
    public void ActivateReviveStone(float addedTime)
    {
        hasReviveStone = true;
        reviveAddedTime = addedTime;
    }

    // 【新增】供Spawner查询
    public bool IsKidsMealActive() => kidsMealTimer > 0;
    private void CheckCheapWarehouse(int count)
    {
        if (isCheapWarehouseActive && count < 36)
        {
            TriggerGameOver("GAME_OVER_NO_BLOCK_CHEAP_WEARHOUSE");

            Debug.Log("便宜仓库效果触发：牌库不足36张，游戏失败。");
        }
    }
    public bool ActivateMagnetV2()
    {
        // 1. 找刻子 (必须是 count==3 且所有牌ID对应的数值相同)
        // Set: List<int> ids. check values: id % 27
        var pungs = huPaiArea.GetAllSets().Where(set =>
            set.Count == 3 &&
            set.Select(id => id % 27).Distinct().Count() == 1 // 确保3张牌是同一种
        ).ToList();

        if (pungs.Count == 0) return false;

        var shuffledPungs = pungs.OrderBy(x => Random.value).ToList();

        foreach (var pung in shuffledPungs)
        {
            int targetValue = pung[0] % 27;
            Transform targetTransform = tetrisGrid.GetBlockTransformByValue(targetValue);

            if (targetTransform != null)
            {
                var blockUnit = targetTransform.GetComponent<BlockUnit>();
                int targetId = blockUnit.blockId;

                // 尝试升级
                if (huPaiArea.UpgradePungToKong(targetValue, targetId))
                {
                    // 【修复】确保物理移除
                    tetrisGrid.RemoveSpecificBlock(targetTransform);

                    if (isTrinityActive) ApplyPermanentBaseScoreBonus(5);
                    return true;
                }
            }
        }
        return false;
    }

    public void ActivateWantedPoster(int goldMult, float scorePercent)
    {
        // 设置状态
        if (isWantedPosterActive)
        {
            wantedPosterGoldMult *= goldMult; // 3 * 3 = 9
        }
        else
        {
            isWantedPosterActive = true;
            wantedPosterGoldMult = goldMult; // 初始 = 3
        }

        // 【关键】立即刷新 UI
        // 这会让界面上的金币目标数字立刻变红，并显示 x3 后的数值
        UpdateTargetScoreUI();

        Debug.Log($"悬赏令激活成功: 金币倍率 x{goldMult}");

        // 处理加分 (如果您希望该道具仅加倍不加分，请在 Inspector 中将 scoreIncreasePercent 设为 0)
        // 这里保留代码逻辑以防万一您以后想加回来
        if (scorePercent > 0)
        {
            long currentScore = scoreManager.GetCurrentScore();
            long bonus = (long)(currentScore * scorePercent);
            if (bonus > 0)
            {
                scoreManager.AddScore(bonus);
            }
        }
    }
    // 【新增】供 UI 调用，获取当前允许的选择数量
    // 使用快照数据，确保本次新获得的 SSSVIP 不会立刻生效
    public int GetCurrentRewardSelectionLimit()
    {
        return _snapshotSSSVIP ? 2 : 1;
    }
    public GameSettings GetSettings()
    {
        return settings;
    }
    // 【新增】幸运瓶盖
    public void ActivateLuckyCap() { luckyCapStack++; }
    public void ConsumeLuckyCap() { luckyCapStack = 0; } // 使用后消耗

    // 【新增】漏斗
    public void ActivateFilter(float duration)
    {
        isFilterActive = true;
        filterTimer = duration;

        // 【新增】道具生效瞬间，立即检查并处理当前的“下一个方块”
        if (spawner != null)
        {
            spawner.ForceRerollIfLevel3();
        }
    }

    // 【新增】通用加权随机选择 (用于道具和条约)
    // 根据传奇标签动态计算权重
    public T GetWeightedRandomItem<T>(List<T> pool) where T : ScriptableObject
    {
        if (pool == null || pool.Count == 0) return null;

        // 1. 计算当前传奇权重
        // 公式：基础 + (基础 * 0.5 * (圈数 - 1))
        // 也就是每一圈增加 50% 的基础权重
        float legendaryBase = settings.legendaryWeightBase;
        int currentLoop = scoreManager.GetCurrentLoop();

        // 【修改】计算逻辑：不再直接用 currentLoop，而是用“距离上次出现过的圈数差”
        // 比如：第1圈(未出现)，delta = 1 - 0 - 1 = 0 (基础权重)
        // 第5圈出现过，第6圈时，delta = 6 - 5 - 1 = 0 (重置回基础权重)
        // 第10圈还没出，delta = 10 - 5 - 1 = 4 (权重增加)
        int deltaLoops = Mathf.Max(0, currentLoop - _loopOfLastLegendaryAppearance - 1);

        float currentLegendaryWeight = legendaryBase + (legendaryBase * settings.legendaryWeightIncreasePerLoop * deltaLoops);

        float normalWeight = settings.normalWeight;

        // 2. 构建权重列表
        Dictionary<T, float> weightedPool = new Dictionary<T, float>();
        float totalWeight = 0f;

        foreach (var item in pool)
        {
            // 通过反射或接口检查 isLegendary 属性
            // 由于 ItemData 和 ProtocolData 都有 isLegendary，我们可以强制转换检查
            bool isLegendary = false;
            if (item is ItemData iData) isLegendary = iData.isLegendary;
            else if (item is ProtocolData pData) isLegendary = pData.isLegendary;

            float w = isLegendary ? currentLegendaryWeight : normalWeight;
            weightedPool[item] = w;
            totalWeight += w;
        }

        // 3. 随机抽取
        float randomValue = Random.value * totalWeight;
        foreach (var kvp in weightedPool)
        {
            if (randomValue < kvp.Value) return kvp.Key;
            randomValue -= kvp.Value;
        }

        return pool[pool.Count - 1]; // 保底
    }

    // 【新增】获取列表的加权随机子集 (用于 GenerateHuRewards)
    public List<T> GetWeightedRandomList<T>(List<T> sourcePool, int count) where T : ScriptableObject
    {
        List<T> result = new List<T>();
        List<T> tempPool = new List<T>(sourcePool); // 复制一份，避免重复选择

        for (int i = 0; i < count; i++)
        {
            if (tempPool.Count == 0) break;
            T selected = GetWeightedRandomItem(tempPool);
            result.Add(selected);
            tempPool.Remove(selected); // 选过的不由再选
        }
        return result;
    }
    public int GetCurrentGoldMultiplier()
    {
        int mult = 1;

        // 使用变量，而不是硬编码的 3
        if (isWantedPosterActive)
        {
            mult *= wantedPosterGoldMult;
        }

        // 支持与“时间就是金钱”叠加
        if (isTimeIsMoneyActive)
        {
            mult *= 2;
        }

        return mult;
    }

    private void GrantStartingRewards(Difficulty difficulty)
    {
        if (difficulty == Difficulty.Easy) return;

        // 【修改】添加 (isTestMode || ...) 判断
        // 逻辑：如果是测试模式，或者道具已解锁，则允许放入随机池
        var validItems = settings.commonItemPool
            .Where(i => !i.isLegendary && (settings.isTestMode || SaveManager.IsItemUnlocked(i.itemName, i.isInitial)))
            .ToList();

        var validProtocols = settings.protocolPool
            .Where(p => !p.isLegendary && (settings.isTestMode || SaveManager.IsProtocolUnlocked(p.protocolName, p.isInitial)))
            .ToList();

        if (difficulty == Difficulty.Normal)
        {
            AddRandomItems(validItems, 1);
            Debug.Log($"普通难度开局奖励 (TestMode:{settings.isTestMode})");
        }
        else if (difficulty == Difficulty.Hard)
        {
            AddRandomItems(validItems, 2);
            AddRandomProtocol(validProtocols);
            Debug.Log($"困难难度开局奖励 (TestMode:{settings.isTestMode})");
        }
    }
    private void AddRandomItems(List<ItemData> sourcePool, int count)
    {
        if (sourcePool == null || sourcePool.Count == 0) return;

        for (int i = 0; i < count; i++)
        {
            ItemData randomItem = sourcePool[Random.Range(0, sourcePool.Count)];
            inventoryManager.AddItem(randomItem);
        }
    }

    private void AddRandomProtocol(List<ProtocolData> sourcePool)
    {
        if (sourcePool == null || sourcePool.Count == 0) return;

        // 随机取一个，确保不重复添加已有的（虽然开局通常是空的，但为了健壮性）
        // 排除掉已经激活的条约
        var available = sourcePool.Except(activeProtocols).ToList();

        if (available.Count > 0)
        {
            ProtocolData randomProtocol = available[Random.Range(0, available.Count)];
            AddProtocol(randomProtocol);
        }
    }
    public bool IsProtocolListFull()
    {
        return activeProtocols.Count >= settings.maxProtocolCount;
    }
    private void ProcessPendingProtocolRemovals()
    {
        if (protocolsMarkedForRemoval.Count > 0)
        {
            foreach (var proto in protocolsMarkedForRemoval)
            {
                if (activeProtocols.Contains(proto))
                {
                    // 1. 移除效果
                    proto.RemoveEffect(this);
                    // 2. 从激活列表中移除
                    activeProtocols.Remove(proto);
                }
            }
            // 清空待删除列表
            protocolsMarkedForRemoval.Clear();

            // 刷新 UI
            if (gameUI != null)
            {
                gameUI.UpdateProtocolUI(activeProtocols);
                RecalculateOneManArmy();
            }
        }
        gameUI.UpdateLoopProgressText(scoreManager.GetLoopProgressString());
        UpdateFallSpeed();
    }
    public void TogglePoolViewer()
    {
        // 检查是否在不能中断的核心流程中
        if (gameUI.IsHuPopupActive() || gameUI.IsGameOverPanelActive())
        {
            // 游戏处于重要流程，禁止打开/关闭牌库
            gameUI.ShowToast("当前流程无法打开牌库预览");
            return;
        }

        // 切换状态
        if (gameUI.IsPoolViewerActive())
        {
            // 如果已打开，则关闭
            gameUI.HidePoolViewer();
            if (!isPaused)
            {
                Time.timeScale = 1f;
                if (AudioManager.Instance) AudioManager.Instance.ResumeCountdownSound();
            }
        }
        else
        {
            if (gameUI.IsPatternViewerActive())
            {
                gameUI.HidePatternViewer();
                // 注意：这里不需要恢复时间，因为马上要打开牌库预览
            }
            // 如果已关闭，则打开
            gameUI.ShowPoolViewer();
            Time.timeScale = 0f;
            if (AudioManager.Instance) AudioManager.Instance.PauseCountdownSound();
        }
    }
    public void TogglePatternViewer()
    {
        // 1. 核心流程互斥检查
        // 如果正在胡牌结算，或者游戏结束了，不允许打开图鉴
        if (gameUI.IsHuPopupActive() || gameUI.IsGameOverPanelActive())
        {
            gameUI.ShowToast("当前流程无法打开图鉴");
            return;
        }

        // 2. 切换逻辑
        if (gameUI.IsPatternViewerActive())
        {
            gameUI.HidePatternViewer();
            if (!isPaused)
            {
                Time.timeScale = 1f;
                if (AudioManager.Instance) AudioManager.Instance.ResumeCountdownSound();
            }
        }
        else
        {
            if (gameUI.IsPoolViewerActive())
            {
                gameUI.HidePoolViewer();
            }
            gameUI.ShowPatternViewer();
            Time.timeScale = 0f;
            if (AudioManager.Instance) AudioManager.Instance.PauseCountdownSound();
        }
    }
    // 1. 激活空投炸弹
    public void ActivateDropBomb()
    {
        Tetromino currentTetromino = FindObjectOfType<Tetromino>();
        Transform ignoreTransform = currentTetromino != null ? currentTetromino.transform : null;

        bool hasCleared = tetrisGrid.ForceClearTopRows(3, ignoreTransform);

        if (hasCleared)
        {
            _isBombOrSpecialClear = true;
        }
    }

    public bool ActivateFryingPan(float timeToAdd, int scoreToAdd)
    {
        if (spawner == null) return false;

        // 1. 尝试移除随机方块
        bool success = spawner.RemoveRandomBlock();

        if (success)
        {
            // 2. 成功移除后 -> 增加时间
            remainingTime += timeToAdd;
            // 立即刷新时间UI (防止游戏处于暂停/菜单状态时UI不更新)
            if (gameUI != null) gameUI.UpdateTimerText(remainingTime);

            // 3. 增加永久基础分
            ApplyPermanentBaseScoreBonus(scoreToAdd);

            Debug.Log($"平底锅使用成功：时间+{timeToAdd}s, 基础分+{scoreToAdd}");
            return true;
        }
        else
        {
            // 4. 方块不足，使用失败 (道具不消耗)
            string msg = "方块数量不足，无法使用！"; // 默认兜底文本

            if (LocalizationManager.Instance != null)
            {
                msg = LocalizationManager.Instance.GetText("ITEM_TIPS_11");
            }

            if (gameUI != null)
            {
                gameUI.ShowToast(msg);
            }
            return false;
        }
    }

    // 2. 激活剪刀
    public bool ActivateScissors()
    {
        bool success = spawner.RemoveHighestMultiplierBlock();
        return success;
    }

    // 3. 激活金苹果
    public void ActivateGoldenApple()
    {
        int uniqueCount = spawner.GetUniqueBlockCount();
        int bonus = uniqueCount * 3;

        // 永久增加基础分
        ApplyPermanentBaseScoreBonus(bonus);
    }

    // 4. 激活魔术幕布
    public void ActivateMagicCurtain()
    {
        tetrisGrid.ShuffleAllBoardTiles();
    }
    public void ActivateChampagne()
    {
        isChampagneActive = true;
        champagneSpawnCount = 0;
    }
    public void ActivatePassport(int suitIndex)
    {
        // 1. 设置状态
        activePassportSuit = suitIndex;
        passportTimer = 15f; // 15秒

        // 2. 立即刷新当前预览的方块
        // 这样玩家能立刻看到下一个方块变成了指定花色
        if (spawner != null)
        {
            spawner.RefreshPreviewTilesOnly();
        }

        // 3. UI 提示
        string suitName = suitIndex == 0 ? "筒子" : (suitIndex == 1 ? "条子" : "万子");
        // 如果您移除了 ShowToast，这里可以不写，或者保留 Debug
        Debug.Log($"护照生效：接下来 15秒 尽可能是 {suitName}");
    }
    public int GetActivePassportSuit()
    {
        return activePassportSuit;
    }
    public void OnHuPaiTileAdded(int blockId)
    {
        if (isNatureReserveActive)
        {
            if (blockId % 27 == 0)
            {
                int bonus = 6;
                ApplyRoundBaseScoreBonus(bonus);
            }
        }
    }
    public int GetEffectiveFanPerKong()
    {
        int baseFan = settings.fanBonusPerKong; // 默认为 1
        return isBloomingOnKongActive ? (baseFan + 1) : baseFan;
    }
    public void RecalculateOneManArmy(bool loopFinished = false)
    {
        if (!isOneManArmyActive)
        {
            // 如果条约都没了，确保存储的倍率被移除
            if (_omaAppliedFactor != 1f)
            {
                ApplyExtraMultiplier(1f / _omaAppliedFactor); // 移除旧倍率
                _omaAppliedFactor = 1f;
            }
            _omaCurrentGrowth = 2f; // 重置潜能
            return;
        }

        // 判断生效条件：当前只有 1 个条约 (就是它自己)
        bool conditionMet = activeProtocols.Count == 1;

        if (conditionMet)
        {
            // --- 处于生效状态 ---

            if (_omaAppliedFactor == 1f)
            {
                // 情况A：刚被激活，或者刚从“失效状态”恢复
                // 规则：变回 x2
                _omaCurrentGrowth = 2f;

                ApplyExtraMultiplier(_omaCurrentGrowth); // 应用 x2
                _omaAppliedFactor = _omaCurrentGrowth;

                Debug.Log("千里走单骑：激活！倍率 x2");
            }
            else if (loopFinished)
            {
                // 情况B：一直生效中，且完成了一圈
                // 规则：倍率翻倍
                float oldFactor = _omaAppliedFactor;
                _omaCurrentGrowth *= 2f;

                // 更新全局倍率 (先除旧的，再乘新的)
                ApplyExtraMultiplier(1f / oldFactor);
                ApplyExtraMultiplier(_omaCurrentGrowth);
                _omaAppliedFactor = _omaCurrentGrowth;

                Debug.Log($"千里走单骑：完成一圈！倍率成长为 x{_omaCurrentGrowth}");
            }
        }
        else
        {
            // --- 处于失效状态 (有其他条约) ---

            if (_omaAppliedFactor != 1f)
            {
                // 移除当前施加的倍率，退回 x1
                ApplyExtraMultiplier(1f / _omaAppliedFactor);
                _omaAppliedFactor = 1f;

                // 规则：期间加入条约则失效，潜能重置为 2 (下次激活时从2开始)
                _omaCurrentGrowth = 2f;

                Debug.Log("千里走单骑：因其他条约加入而失效。倍率恢复 x1");
            }
        }
    }
    public void ToggleMistUI(bool isActive)
    {
        if (gameUI != null)
        {
            gameUI.SetMistActive(isActive);
        }
    }
    private void CheckAndShowTutorial()
    {
        // 检查 PlayerPrefs，默认值为 0 (未看过)
        bool hasSeen = PlayerPrefs.GetInt(PREF_HAS_SEEN_TUTORIAL, 0) == 1;

        if (!hasSeen)
        {
            // === 第一次进入 ===

            // 1. 显示弹窗
            gameUI.ShowTutorialPanel(true);

            // 2. 暂停游戏 (系统级暂停，不消耗次数)
            Time.timeScale = 0f;
            if (AudioManager.Instance) AudioManager.Instance.PauseCountdownSound();

            Debug.Log("首次进入游戏，触发新手教学");
        }
        else
        {
            // === 非第一次 ===
            // 确保面板是关闭的
            gameUI.ShowTutorialPanel(false);
        }
    }
    public void CloseTutorial()
    {
        // 1. 隐藏 UI
        gameUI.ShowTutorialPanel(false);

        // 2. 标记为已看 (永久保存)
        PlayerPrefs.SetInt(PREF_HAS_SEEN_TUTORIAL, 1);
        PlayerPrefs.Save();

        // 3. 恢复游戏
        // 注意：只有在没有手动暂停的情况下才恢复
        if (!isPaused)
        {
            Time.timeScale = 1f;
            if (AudioManager.Instance) AudioManager.Instance.ResumeCountdownSound();
        }

        Debug.Log("新手教学结束，游戏开始");
    }
    public void IncrementItemUsedCount()
    {
        _itemsUsedThisGame++;
        if (AchievementManager.Instance != null)
        {
            // 1. 检查单局成就 (如: 单局使用20个)
            AchievementManager.Instance.CheckSingleGameRealtimeStats(_itemsUsedThisGame, _protocolsObtainedThisGame);

            // 2. 累积成就 (如: 累计使用1000个) - 顺便补上这个逻辑
            AchievementManager.Instance.AddProgress(AchievementType.AccumulateItemUse, 1);
        }
    }
    // 1. 检查是否所有内容已解锁 (开启功能的条件)
    public bool IsAllContentUnlocked()
    {
        // 检查普通道具
        foreach (var item in settings.commonItemPool)
            if (!SaveManager.IsItemUnlocked(item.itemName, item.isInitial)) return false;

        // 检查高级道具
        foreach (var item in settings.advancedItemPool)
            if (!SaveManager.IsItemUnlocked(item.itemName, item.isInitial)) return false;

        // 检查条约
        foreach (var proto in settings.protocolPool)
            if (!SaveManager.IsProtocolUnlocked(proto.protocolName, proto.isInitial)) return false;

        return true;
    }

    // 2. 获取当前刷新价格
    public int GetCurrentRefreshCost()
    {
        return _currentRefreshCost;
    }

    // 3. 尝试消费金币进行刷新
    public bool TrySpendRefreshCost()
    {
        int currentGold = GameSession.Instance.CurrentGold;
        if (currentGold >= _currentRefreshCost)
        {
            GameSession.Instance.AddGold(-_currentRefreshCost); // 扣钱
            _currentRefreshCost *= 2; // 价格翻倍
            return true;
        }
        return false;
    }

    // 4. 执行刷新 (保留被锁定的类别)
    public HuRewardPackage RefreshRewardPackage(HuRewardPackage currentPackage, bool keepBlocks, bool keepItems, bool keepProtocols, bool isAdvanced)
    {
        // 生成一套全新的奖励
        HuRewardPackage newPackage = GenerateHuRewards(isAdvanced, _tempLastHandMult);

        // 如果某一项被锁定了(keep=true)，则把旧数据覆盖回去 (保留旧的)
        if (keepBlocks) newPackage.BlockChoices = currentPackage.BlockChoices;
        if (keepItems) newPackage.ItemChoices = currentPackage.ItemChoices;
        if (keepProtocols) newPackage.ProtocolChoices = currentPackage.ProtocolChoices;

        return newPackage;
    }
    public void ActivateBonusBlocksImmediately(string prefabName, int count)
    {
        // 从 Spawner 的总表 (Master List) 中找到对应的方块预制体
        var prefab = spawner.GetMasterList().FirstOrDefault(p => p.name == prefabName);

        if (prefab != null)
        {
            // 循环添加指定数量到活跃池
            for (int i = 0; i < count; i++)
            {
                spawner.AddTetrominoToPool(prefab);
            }

            Debug.Log($"试用小样生效：立即添加了 {count} 个 {prefabName}");
            UpdateCurrentBaseScore();
        }
        else
        {
            Debug.LogError($"无法找到名为 {prefabName} 的方块预制体！请检查 TrialSampleItem 配置的名字是否正确。");
        }
    }
    public void SkipToLastRoundOfLoop()
    {
        if (scoreManager != null)
        {
            // 1. 修改核心数据
            scoreManager.SetProgressToLastRound();

            // 2. 立即刷新左上角的圈数 UI (例如从 "1/4" 变成 "4/4")
            if (gameUI != null)
            {
                gameUI.UpdateLoopProgressText(scoreManager.GetLoopProgressString());
            }
        }
    }
    public void ActivateElixirWine(float multiplier)
    {
        // 1. 获取当前基础分 (此时已包含所有之前的加成)
        int currentScore = baseFanScore;

        // 2. 计算需要增加的数值
        // 逻辑：目标分 = 当前分 * 倍率
        // 增量 = 目标分 - 当前分 = 当前分 * (倍率 - 1)
        // 例如：15分 * 1.5倍 = 22.5 -> 增量 7.5 -> 四舍五入为 8
        int bonusToAdd = Mathf.RoundToInt(currentScore * (multiplier - 1f));

        if (bonusToAdd > 0)
        {
            // 3. 将增量应用为永久固定加分 (就像吃了好几个果汁一样)
            // 这样后续再吃果汁(+3)，就只是单纯的 +3，不会被倍率放大
            ApplyPermanentBaseScoreBonus(bonusToAdd);

            Debug.Log($"仙酒生效：当前分 {currentScore} -> 增加 {bonusToAdd} (倍率 {multiplier})");
        }
        else
        {
            Debug.Log("仙酒生效：但当前分数为0或倍率无效，未增加分数。");
        }
    }
    public void TryExecutePendingPassportShuffle()
    {
        if (_pendingPassportShuffle)
        {
            if (blockPool != null)
            {
                blockPool.ShuffleRemainingBlocks();
                Debug.Log("【GameManager】执行延迟洗牌：牌库已重置随机。");
            }
            _pendingPassportShuffle = false;
        }
    }
}