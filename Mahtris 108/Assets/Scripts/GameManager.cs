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
    private float permanentBlockMultiplierModifier = 0f; // 【新增】存储条约的永久方块倍率修正

    private GameSessionConfig currentSessionConfig; // 【新增】持有当前游戏会话的配置
    private float difficultySpeedMultiplier = 1.0f; // 【新】难度带来的速度乘数

    // 【新增】V4.2 道具/条约 变量
    private float kidsMealTimer = 0f;
    private bool hasReviveStone = false;
    private float reviveAddedTime = 0f;

    public bool isAttackOnGiantActive = false;
    public bool isCheapWarehouseActive = false;
    public bool isMarshLandActive = false;
    private float marshLandTimer = 0f;
    public bool isRenewableEnergyActive = false;
    public bool isAllMenEqualActive = false;
    public bool isStrongWorldActive = false;
    public bool isAdventFoodActive = false;
    private float adventFoodTimer = 0f;
    private int adventFoodBonus = 0; // 临期食品提供的动态加分
    public bool isRoutineWorkActive = false;
    public bool isUnstableCurrentActive = false;
    private float unstableCurrentTimer = 0f;
    public bool isSSSVIPActive = false;

    // 【新增】V4.3 条约状态
    public bool isDelayGratificationActive = false;
    private int delayGratificationBonus = 0;
    public bool isDrMahjongActive = false;
    public bool isOldSchoolActive = false;
    public bool isBerserkerActive = false;
    public bool isTimeIsMoneyActive = false;
    public bool isBulletTimeActive = false;
    public bool isLogBridgeActive = false;
    public bool isGreatRevolutionActive = false;
    public bool isTrinityActive = false;
    public bool isRealpolitikActive = false;

    private bool isWantedPosterActive = false;
    private int wantedPosterGoldMult = 1;
    // 【新增】条约状态快照 (用于处理生效时机)
    private bool _snapshotSSSVIP = false;
    private bool _snapshotStrongWorld = false;
    // 【新增】V4.4 道具状态
    public bool isLuckyCapActive = false;
    public bool isFilterActive = false;
    private float filterTimer = 0f;
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
        if (Input.GetKeyDown(KeyCode.Escape)) TogglePause();
        if (isPaused || isProcessingRows || Time.timeScale == 0f) return;

        // 【新增】子弹时间：仅影响倒计时流逝，不影响 System.Time
        float logicDeltaTime = Time.deltaTime;

        if (isBulletTimeActive && tetrisGrid.GetMaxColumnHeight() > 8)
        {
            logicDeltaTime *= 0.2f; // 时间流速变慢
        }

        // 道具计时器也受子弹时间影响吗？通常是的，统一使用 logicDeltaTime
        remainingTime -= logicDeltaTime;
        gameUI.UpdateTimerText(remainingTime);

        if (kidsMealTimer > 0) kidsMealTimer -= logicDeltaTime;

        if (isMarshLandActive)
        {
            marshLandTimer -= logicDeltaTime;
            if (marshLandTimer <= 0) { marshLandTimer = 10f; ForceClearRowsFromBottom(1); }
        }

        if (isAdventFoodActive && adventFoodBonus > 1)
        {
            adventFoodTimer -= logicDeltaTime;
            if (adventFoodTimer <= 0) { adventFoodTimer = 1f; adventFoodBonus--; UpdateCurrentBaseScore(); }
        }

        if (isUnstableCurrentActive)
        {
            unstableCurrentTimer -= logicDeltaTime;
            if (unstableCurrentTimer <= 0)
            {
                unstableCurrentTimer = 6f;
                int change = Random.Range(1, 37);
                if (Random.value < 0.5f) change = -change;

                // 【修复】防止基础分 < 1
                // 我们不能直接在 ApplyRoundBaseScoreBonus 里限制，因为那是通用方法。
                // 我们预先计算一下：
                if (baseFanScore + change < 1) change = 1 - baseFanScore; // 保证最少为1

                ApplyRoundBaseScoreBonus(change);
            }
        }
        if (isFilterActive)
        {
            filterTimer -= logicDeltaTime; // 使用受子弹时间影响的 delta
            if (filterTimer <= 0)
            {
                isFilterActive = false;
            }
        }
        // 点金手和计分板也应受影响
        if (midasTimer > 0) midasTimer -= logicDeltaTime; // 如果要归零逻辑需补全
        if (scoreboardTimer > 0) scoreboardTimer -= logicDeltaTime;

        if (remainingTime <= 0) GameEvents.TriggerGameOver();
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
        permanentBlockMultiplierModifier = 0f;

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
        // 重置 V4.2 变量
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
        // 重置 V4.3 变量
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
        isLuckyCapActive = false;
        isFilterActive = false;
        filterTimer = 0f;
        gameUI.UpdateLoopProgressText(scoreManager.GetLoopProgressString());

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
        gameUI.UpdateLoopProgressText(scoreManager.GetLoopProgressString());
    }

    private void HandleHuDeclared(List<List<int>> huHand)
    {
        _snapshotSSSVIP = isSSSVIPActive;
        _snapshotStrongWorld = isStrongWorldActive;
        isProcessingRows = true;
        Time.timeScale = 0f;
        // 每次胡牌后更新圈数显示
        gameUI.UpdateLoopProgressText(scoreManager.GetLoopProgressString());


        bool isAdvancedReward = scoreManager.IncrementHuCountAndCheckCycle();

        // 1. 基础番数计算
        var analysisResult = mahjongCore.CalculateHandFan(huHand, settings);

        // 2. 【修复】混淆视听 (HunYaoShiTing) 优先级最高
        // 如果牌型只有2种花色，强制视为清一色
        if (isHunYaoShiTingActive)
        {
            var allTileIds = huHand.SelectMany(s => s).ToList();
            int suitCount = allTileIds.Select(id => ((id % 27) / 9)).Distinct().Count();
            if (suitCount == 2)
            {
                analysisResult.PatternName = "清一色";
                // (可选择是否要调整番数，这里暂只调整名称以触发后续逻辑)
            }
        }

        // 3. 【修复】麻将博士 (DrMahjong)
        // 依赖修正后的 PatternName
        if (isDrMahjongActive)
        {
            // 如果是平胡 (且未被混淆视听改为清一色)，倍率为0
            if (analysisResult.PatternName == "平胡" && !isOldSchoolActive)
            {
                extraMultiplier = 0;
            }
            else
            {
                // 其他牌型，底数变3
                analysisResult.BaseMultiplier = 3f;
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
        long finalScore = (long)(scorePart * blockMultiplier * extraMultiplier);
        scoreManager.AddScore((int)Mathf.Min(finalScore, int.MaxValue));

        // 【修复】时间就是金钱 (TimeIsMoney)
        // 只有在未激活该条约时，才奖励时间
        if (!isTimeIsMoneyActive)
        {
            remainingTime += settings.huTimeBonus;
            // 处理新能源 (RenewableEnergy) 的额外加时
            if (isRenewableEnergyActive)
            {
                remainingTime += 20f;
            }
            gameUI.UpdateLoopProgressText(scoreManager.GetLoopProgressString());
        }

        var rewards = GenerateHuRewards(isAdvancedReward);

        // 狂战士 (Berserker) 逻辑
        if (isBerserkerActive)
        {
            if (rewards.BlockChoices.Count > 0) rewards.BlockChoices = rewards.BlockChoices.Take(1).ToList();
            if (rewards.ItemChoices.Count > 0) rewards.ItemChoices = rewards.ItemChoices.Take(1).ToList();
            if (rewards.ProtocolChoices.Count > 0) rewards.ProtocolChoices = rewards.ProtocolChoices.Take(1).ToList();

            if (rewards.BlockChoices.Count > 0) spawner.AddTetrominoToPool(rewards.BlockChoices[0]);
            if (rewards.ItemChoices.Count > 0) inventoryManager.AddItem(rewards.ItemChoices[0]);
            if (rewards.ProtocolChoices.Count > 0) AddProtocol(rewards.ProtocolChoices[0]);
        }

        gameUI.ShowHuPopup(huHand, analysisResult, baseFanScore, blockMultiplier, extraMultiplier, finalScore, rewards, isAdvancedReward, isBerserkerActive);
    }

    public void ContinueAfterHu()
    {
        delayGratificationBonus = 0; // 重置延迟满足
        if (isGreatRevolutionActive) spawner.RandomizeActivePool(); // 大革命
        if (isBerserkerActive) inventoryManager.UseAllItems(); // 狂战士
        UpdateCurrentBaseScore();
        // 【新增】胡牌时重置“点金手”和“计分板”
        midasTimer = 0f;
        midasGoldValue = 0;
        scoreboardTimer = 0f;
        ignoreMahjongCheckCount = 0; // 【新增】胡牌后重置垃圾筒
        // 【新增】胡牌时，清零“本轮”和“计数”加成，但保留“永久”加成
        roundSpeedBonus = 0;
        countedSpeedBonus = 0;
        countedBonusBlocksRemaining = 0;
        isFilterActive = false;
        filterTimer = 0f;
        isLuckyCapActive = false;
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
        // 【新增】临期食品：重置为120
        if (isAdventFoodActive)
        {
            adventFoodBonus = 120;
            adventFoodTimer = 1f;
        }
        else
        {
            adventFoodBonus = 0;
        }
        UpdateCurrentBaseScore();

        // 【新增】朝九晚五：固定时间95秒
        if (isRoutineWorkActive)
        {
            remainingTime = 95f;
        }
        // 【新增】新能源：加时
        if (isRenewableEnergyActive)
        {
            remainingTime += 20f;
        }
        // 【修复】不稳定电流：新一轮开始后重置计时器（6秒后触发）
        if (isUnstableCurrentActive) unstableCurrentTimer = 6f;
        // ... (调用 Spawner.StartNextRound 之前) ...

        spawner.StartNextRound();
        // 【修改】使用快照变量 _snapshotStrongWorld
        // 只有在胡牌“前”就已经拥有该条约，才会触发效果
        if (_snapshotStrongWorld)
        {
            spawner.AddRandomLevel3Block();
        }

        // 【新增】强者世界：加入Lv3方块
        if (isStrongWorldActive)
        {
            spawner.AddRandomLevel3Block();
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
        gameUI.UpdateLoopProgressText(scoreManager.GetLoopProgressString());
        // ...
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

        List<Transform> allClearedTransforms = new List<Transform>();
        List<List<int>> rowsBlockIds = new List<List<int>>();

        // 1. 先清理Grid，收集数据
        foreach (var y in rowIndices)
        {
            var rowData = tetrisGrid.GetRowDataAndClear(y);
            allClearedTransforms.AddRange(rowData.transforms);
            rowsBlockIds.Add(rowData.blockIds);
            ApplyRowClearRewards(); // 点金手/计分板
        }

        // 2. 计分 (老派玩家 vs 正常)
        // 如果垃圾桶生效，通常不加消除分，但如果规则允许，可移出else
        if (ignoreMahjongCheckCount <= 0)
        {
            if (isOldSchoolActive)
            {
                // 老派玩家：Base * 2^Count
                long oldSchoolScore = (long)(baseFanScore * Mathf.Pow(2, rowIndices.Count));
                scoreManager.AddScore((int)Mathf.Min(oldSchoolScore, int.MaxValue));
            }
            else
            {
                scoreManager.AddScore(settings.scorePerRow * rowIndices.Count);
            }
        }

        // 3. 延迟满足 (+8)
        if (isDelayGratificationActive && rowIndices.Count >= 4)
        {
            delayGratificationBonus += 8;
            UpdateCurrentBaseScore();
        }

        List<int> finalIdsToReturn = new List<int>();

        // 4. 判定逻辑 (垃圾桶 vs 合纵连横 vs 正常)
        if (ignoreMahjongCheckCount > 0)
        {
            // --- 垃圾桶模式 ---
            // 假设规则：垃圾桶消耗次数 = 消除的行数。
            // 例如大垃圾桶(3)，一次消4行，前3行被移除，第4行正常判定？
            // 这里简化处理：只要有垃圾桶计数，这次消除涉及的所有牌都视为被移除（或只移除前N行）
            // 为了符合“最先消除的行被移除”：

            int rowsTotal = rowsBlockIds.Count;
            int rowsToRemove = Mathf.Min(rowsTotal, ignoreMahjongCheckCount);
            ignoreMahjongCheckCount -= rowsToRemove;

            // 将被移除的行的牌丢弃 (不加入 finalIdsToReturn)
            // 将剩下的行的牌进行判定

            if (rowsToRemove < rowsTotal)
            {
                // 有剩余行需要判定
                List<List<int>> remainingRowsData = new List<List<int>>();
                for (int i = rowsToRemove; i < rowsTotal; i++)
                {
                    remainingRowsData.Add(rowsBlockIds[i]);
                }

                // 对剩余行进行判定
                if (isRealpolitikActive)
                {
                    List<int> mergedIds = new List<int>();
                    foreach (var list in remainingRowsData) mergedIds.AddRange(list);
                    ProcessMahjongDetection(mergedIds, ref finalIdsToReturn, allClearedTransforms);
                }
                else
                {
                    foreach (var list in remainingRowsData)
                    {
                        ProcessMahjongDetection(list, ref finalIdsToReturn, allClearedTransforms);
                    }
                }
            }
        }
        else
        {
            // --- 正常模式 ---
            if (isRealpolitikActive)
            {
                // 合纵连横：合并所有ID一次性判定
                List<int> mergedIds = new List<int>();
                foreach (var list in rowsBlockIds) mergedIds.AddRange(list);
                ProcessMahjongDetection(mergedIds, ref finalIdsToReturn, allClearedTransforms);
            }
            else
            {
                // 正常：逐行判定
                foreach (var list in rowsBlockIds)
                {
                    ProcessMahjongDetection(list, ref finalIdsToReturn, allClearedTransforms);
                }
            }
        }

        // 5. 清理
        blockPool.ReturnBlockIds(finalIdsToReturn);
        tetrisGrid.DestroyTransforms(allClearedTransforms);
        tetrisGrid.CompactAllColumns(rowIndices);

        if (!_isBombOrSpecialClear) spawner.SpawnBlock();
        _isBombOrSpecialClear = false;
        isProcessingRows = false;
    }

    // 【辅助方法】封装麻将判定
    private void ProcessMahjongDetection(List<int> tileIds, ref List<int> idsToReturn, List<Transform> transformsToDestroy)
    {
        var result = mahjongCore.DetectSets(tileIds);

        // 三位一体：检测碰/杠
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

        if (setsToAdd.Count > 0) huPaiArea.AddSets(setsToAdd);

        if (huPaiArea.GetSetCount() >= settings.setsForHu)
        {
            var pair = mahjongCore.FindPair(result.RemainingIds);
            if (pair != null)
            {
                result.RemainingIds.Remove(pair[0]); result.RemainingIds.Remove(pair[1]);
                var finalHand = huPaiArea.GetAllSets(); finalHand.Add(pair);

                GameEvents.TriggerHuDeclared(finalHand);

                idsToReturn.AddRange(result.RemainingIds);
                blockPool.ReturnBlockIds(idsToReturn);
                tetrisGrid.DestroyTransforms(transformsToDestroy);

                _isBombOrSpecialClear = true; // 阻止生成新方块
                return; // 退出本层检测
            }
        }

        idsToReturn.AddRange(result.RemainingIds);
    }

    // 【辅助方法】避免代码重复
    private void ApplyRowClearRewards()
    {
        if (midasTimer > 0)
        {
            GameSession.Instance.AddGold(midasGoldValue);
            midasGoldValue *= 2;
        }
        if (scoreboardTimer > 0)
        {
            int currentScore = scoreManager.GetCurrentScore();
            int bonusScore = Mathf.RoundToInt(currentScore * 0.05f);
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
        RecalculateBlockMultiplier();
        var prefabs = spawner.GetActivePrefabs();
        float totalMultiplier = this.blockMultiplier;

        // 【修复】检查是否有人人平等，传递 3f 或 -1f
        float overrideMult = isAllMenEqualActive ? 3f : -1f;

        if (gameUI != null)
        {
            gameUI.UpdateTetrominoList(prefabs, totalMultiplier, overrideMult);
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
        // 【新增】麻将博士：如果是平胡（extraMultiplier被置为0），则无法获得任何奖励
        if (isDrMahjongActive && extraMultiplier == 0)
        {
            return new HuRewardPackage(); // 返回空包，不显示任何选项
        }

        var package = new HuRewardPackage();

        // 【新增】独木桥：每种奖励数量变为1个
        // 如果是独木桥模式，数量固定为1；否则按原有逻辑（高级5/2/2，普通3/3）
        int blockCount = isLogBridgeActive ? 1 : (isAdvanced ? 5 : 3);
        int itemCount = isLogBridgeActive ? 1 : (isAdvanced ? 2 : 3);
        int protocolCount = isLogBridgeActive ? 1 : 2;

        // 生成方块奖励
        package.BlockChoices = GetWeightedRandomBlocks(blockCount, isAdvanced ? settings.advancedBlockRewardWeights : settings.commonBlockRewardWeights).ToList();

        if (isAdvanced)
        {
            // 【修改】使用加权随机
            package.ItemChoices = GetWeightedRandomList(settings.advancedItemPool, itemCount);

            var availableProtocols = settings.protocolPool.Except(activeProtocols).ToList();
            package.ProtocolChoices = GetWeightedRandomList(availableProtocols, protocolCount);
        }
        else
        {
            // 【修改】使用加权随机
            package.ItemChoices = GetWeightedRandomList(settings.commonItemPool, itemCount);
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
        int baseSpeed = settings.baseDisplayedSpeed;
        int baseSpeedWithDifficulty = (int)(baseSpeed * this.difficultySpeedMultiplier);
        int huBonus = scoreManager.GetHuCount() * settings.speedIncreasePerHu_Int;
        int currentCountedBonus = (countedBonusBlocksRemaining > 0) ? countedSpeedBonus : 0;
        int totalBonus = permanentSpeedBonus + roundSpeedBonus + currentCountedBonus;

        int totalDisplayedSpeed = baseSpeedWithDifficulty + huBonus + totalBonus;
        if (totalDisplayedSpeed < 1) totalDisplayedSpeed = 1;

        currentFallSpeed = 20.0f / totalDisplayedSpeed;

        // 【新增】子弹时间逻辑 (覆盖最终计算)
        if (isBulletTimeActive && tetrisGrid.GetMaxColumnHeight() > 8)
        {
            totalDisplayedSpeed = 5; // 显示为5
            currentFallSpeed = 20.0f / 5.0f; // 实际速度也是5
        }

        gameUI.UpdateSpeedText(totalDisplayedSpeed);
    }

    private void OnScoreUpdated(int newScore)
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
            int reward = currentSessionConfig.DifficultyScoreLevels[currentScoreLevelIndex].goldReward;

            // 【修复】悬赏令逻辑：在发放前应用倍率
            if (isWantedPosterActive)
            {
                reward *= wantedPosterGoldMult;
                isWantedPosterActive = false; // 消耗掉效果
                wantedPosterGoldMult = 1;
            }

            if (GameSession.Instance != null)
            {
                GameSession.Instance.AddGold(reward);
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.SoundLibrary.targetReached);
            }

            currentScoreLevelIndex++;

            // 【修复】如果索引已达到上限（通关），立即停止循环并触发获胜
            if (currentScoreLevelIndex >= currentSessionConfig.DifficultyScoreLevels.Count)
            {
                HandleGameWon();
                break; // 【关键】必须退出循环，否则下次判断条件时会越界
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
            gameUI.UpdateTargetScoreDisplay(level.targetScore, level.goldReward);

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
        int finalScore = scoreManager.GetCurrentScore();
        bool isNewHighScore = scoreManager.CheckForNewHighScore(finalScore);
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
    public void UpdateCurrentBaseScore()
    {
        int defaultScore = settings.baseFanScore;
        // 【新增】延迟满足 (-10)
        int delayPenalty = isDelayGratificationActive ? -10 : 0;

        int addedScore = defaultScore + permanentBaseScoreBonus + roundBaseScoreBonus + adventFoodBonus + delayGratificationBonus + delayPenalty;
        int calculatedScore = (int)(addedScore * permanentBaseScoreMultiplier);

        if (isSteroidReversalActive && calculatedScore < 1) calculatedScore = 1;
        // 【新增】延迟满足最低为1
        if (isDelayGratificationActive && calculatedScore < 1) calculatedScore = 1;

        baseFanScore = calculatedScore;
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
            GameEvents.TriggerGameOver();
        }
    }
    public bool ActivateMagnetV2()
    {
        // 1. 找刻子 (胡牌区中拥有3张一样的牌)
        var pungs = huPaiArea.GetAllSets().Where(s => s.Count == 3).ToList();
        if (pungs.Count == 0) return false;

        // 2. 随机打乱刻子顺序，避免每次都只吸第一个刻子
        var shuffledPungs = pungs.OrderBy(x => Random.value).ToList();

        // 3. 遍历每一个刻子，尝试在场上找对应的第4张牌
        foreach (var pung in shuffledPungs)
        {
            int targetValue = pung[0] % 27; // 获取牌面值 (0-26)

            // 使用 TetrisGrid 的新方法精确查找
            Transform targetTransform = tetrisGrid.GetBlockTransformByValue(targetValue);

            if (targetTransform != null)
            {
                // 找到了！
                var blockUnit = targetTransform.GetComponent<BlockUnit>();
                int targetId = blockUnit.blockId;

                // 4. 逻辑移除 (加入胡牌区)
                if (huPaiArea.UpgradePungToKong(targetValue, targetId))
                {
                    // 5. 【关键修复】物理移除 (销毁物体)
                    // 使用 RemoveSpecificBlock 彻底从网格中移除并销毁 GameObject
                    tetrisGrid.RemoveSpecificBlock(targetTransform);

                    // 触发三位一体条约效果
                    if (isTrinityActive) ApplyPermanentBaseScoreBonus(5);

                    return true; // 成功使用
                }
            }
        }

        return false; // 没找到任何可吸的牌
    }

    public void ActivateWantedPoster(int goldMult, float scorePercent)
    {
        // 1. 先设置状态，确保 AddScore 触发 OnScoreUpdated 时倍率已生效
        isWantedPosterActive = true;
        wantedPosterGoldMult = goldMult;

        // 2. 增加当前分数 (例如增加 20%)
        int currentScore = scoreManager.GetCurrentScore();
        int bonus = (int)(currentScore * scorePercent);

        // 这次加分可能会直接触发升级并获得金币，正好应用上面的倍率
        scoreManager.AddScore(bonus);
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
    public void ActivateLuckyCap() { isLuckyCapActive = true; }
    public void ConsumeLuckyCap() { isLuckyCapActive = false; } // 使用后消耗

    // 【新增】漏斗
    public void ActivateFilter(float duration) { isFilterActive = true; filterTimer = duration; }

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
        float currentLegendaryWeight = legendaryBase + (legendaryBase * settings.legendaryWeightIncreasePerLoop * (currentLoop - 1));

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
}