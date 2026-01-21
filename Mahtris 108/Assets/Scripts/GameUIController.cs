// FileName: GameUIController.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using System;

public static class TransformExtensions
{
    public static void DestroyChildren(this Transform t)
    {
        foreach (Transform child in t)
        {
            GameObject.Destroy(child.gameObject);
        }
    }
}

// 新增：用于在UI和逻辑间传递奖励数据的结构体
public class HuRewardPackage
{
    public List<GameObject> BlockChoices = new List<GameObject>();
    public List<ItemData> ItemChoices = new List<ItemData>();
    public List<ProtocolData> ProtocolChoices = new List<ProtocolData>();
}

public class GameUIController : MonoBehaviour
{
    // ========================================================================
    // 1. 核心 HUD 显示 (顶部/侧边栏)
    // ========================================================================
    [Header("核心 HUD 显示")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text timerText;
    [SerializeField] private Text speedText;
    [SerializeField] private Text goldText;
    [SerializeField] private Text poolCountText;      // 牌库剩余量
    [SerializeField] private Text loopProgressText;   // 圈数

    [Header("倍率与分数详情")]
    [SerializeField] private Text baseScoreText;      // 基础番
    [SerializeField] private Text blockMultiplierText;// 方块倍率
    [SerializeField] private Text extraMultiplierText;// 额外倍率

    // ========================================================================
    // 2. 目标与进度 (Slider & Level Info)
    // ========================================================================
    [Header("目标与进度")]
    [SerializeField] private Slider targetProgressBar;
    [SerializeField] private Text currentScoreForBarText; // ex: "1200 / 5000"
    [SerializeField] private Text goldRewardText;         // ex: "100" 或 "无尽模式"

    [Header("无尽模式标识")]
    [SerializeField] private GameObject endlessModeLabel;

    // ========================================================================
    // 3. 容器与插槽 (道具/条约)
    // ========================================================================
    [Header("道具栏")]
    [SerializeField] private Transform itemContainer;
    [SerializeField] private GameObject itemSlotPrefab;
    [SerializeField] private GameObject itemBarPanel;

    [Header("条约栏")]
    [SerializeField] private Transform protocolContainer;
    [SerializeField] private GameObject protocolSlotPrefab;
    [SerializeField] private int maxProtocolSlots = 5;
    [SerializeField] private GameObject protocolBarPanel;

    // ========================================================================
    // 4. 下一个方块预览
    // ========================================================================
    [Header("预览区域")]
    [SerializeField] private Transform nextBlockPreviewArea;
    private GameObject currentPreviewObject;

    // ========================================================================
    // 5. 胡牌弹窗 (Hu Popup)
    // ========================================================================
    [Header("胡牌弹窗 - 结构")]
    [SerializeField] private GameObject huPopupRoot;       // 父节点 (黑色全屏遮罩)
    [SerializeField] private RectTransform huPopupContainer; // 动画容器 (弹窗本体)
    [SerializeField] private GameObject huStage1Panel;     // 第一页: 得分
    [SerializeField] private GameObject huStage2Panel;     // 第二页: 奖励

    [Header("胡牌弹窗 - 第一页 (得分)")]
    [SerializeField] private Transform huHandDisplayArea;  // 手牌展示
    [SerializeField] private Text patternNameText;         // 牌型名
    [SerializeField] private Text patternFanText;          // 牌型番
    [SerializeField] private GameObject kongInfoGroup;     // 杠信息组
    [SerializeField] private Text kongCountText;           // 杠数量
    [SerializeField] private Text kongFanText;             // 杠番数
    [SerializeField] private Button nextStepButton;        // "下一步"按钮

    [Header("胡牌展示 - 布局设置")]
    [SerializeField] private float uiTileWidth = 45f;    // 单张麻将牌UI宽度
    [SerializeField] private float uiTileHeight = 64f;   // 单张麻将牌UI高度
    [SerializeField] private float uiKongOffsetY = 18f;  // 杠牌第4张的向上偏移量
    [SerializeField] private float uiSetSpacing = 20f;   // 组与组之间的间距

    [Header("胡牌弹窗 - 分数公式")]
    [SerializeField] private Text formulaBaseScoreText;
    [SerializeField] private Text formulaFanBaseText;
    [SerializeField] private Text formulaFanExpText;
    [SerializeField] private Text formulaBlockMultText;
    [SerializeField] private Text formulaExtraMultText;
    [SerializeField] private Text formulaFinalScoreText;   // 最终得分 (带滚动动画)

    [Header("胡牌弹窗 - 第二页 (奖励)")]
    [SerializeField] private Text nextRoundTimeText;       // ex: +60s
    [SerializeField] private Text nextRoundSpeedText;      // ex: Lv.+2
    [SerializeField] private Button continueButton;        // "继续"按钮

    [Header("奖励选项容器")]
    [SerializeField] private GameObject commonRewardPanel;
    [SerializeField] private Transform commonRewardBlockArea;
    [SerializeField] private Transform commonRewardItemArea;

    [SerializeField] private GameObject advancedRewardPanel;
    [SerializeField] private Transform advancedRewardBlockArea;
    [SerializeField] private Transform advancedRewardItemArea;
    [SerializeField] private Transform advancedRewardProtocolArea;

    [Header("奖励刷新")]
    [SerializeField] private GameObject rewardOptionPrefab; // 奖励按钮预制件
    [SerializeField] private GameObject refreshRoot;
    [SerializeField] private Button refreshButton;
    [SerializeField] private Text refreshCostText;
    private HuRewardPackage _currentRewardPackage;
    private bool _isLastHuAdvanced;
    private List<int> _selectedBlockIndices = new List<int>();
    private List<int> _selectedItemIndices = new List<int>();
    private List<int> _selectedProtocolIndices = new List<int>();
    private Transform currentBlockContainer;
    private Transform currentItemContainer;
    private Transform currentProtocolContainer;
    // ========================================================================
    // 6. 游戏结束弹窗 (Game Over)
    // ========================================================================
    [Header("游戏结束弹窗")]
    [SerializeField] private GameObject gameOverPanel;         // 父节点 (遮罩)
    [SerializeField] private RectTransform gameOverContainer;  // 动画容器
    [SerializeField] private Text gameOverTitleText;
    [SerializeField] private Text finalScoreText;
    [SerializeField] private Text gameOverReasonText;
    [SerializeField] private GameObject newHighScoreIndicator;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button endlessModeButton;

    [Header("结算详情")]
    [SerializeField] private Text completionRateText;    // 完成度文本 (ex: "完成度: 85%")
    [SerializeField] private GameObject rewardInfoRoot;  // 奖励信息的父节点 (无尽模式隐藏)
    [SerializeField] private Text baseRewardText;        // 基础奖金文本
    [SerializeField] private GameObject extraRewardRoot; // 额外奖金父节点 (为0隐藏)
    [SerializeField] private Text extraRewardText;       // 额外奖金文本
    // ========================================================================
    // 7. 暂停面板 (Pause)
    // ========================================================================
    [Header("暂停面板")]
    [SerializeField] private GameObject pausePanel;       // 父节点 (黑色遮罩)
    [SerializeField] private RectTransform pauseContainer;// 弹窗本体

    [Header("暂停按钮 (HUD)")]
    [SerializeField] private Button pauseButton;          // 仅保留点击功能，不换图

    [Header("面板内按钮")]
    [SerializeField] private Button resumeButton;         // "继续游戏"
    [SerializeField] private Button endGameButton;        // "结束游戏"

    // ========================================================================
    // 8. 列表与杂项
    // ========================================================================
    [Header("Tetromino 统计列表")]
    [SerializeField] private Transform tetrominoListContent;
    [SerializeField] private Text totalMultiplierText;
    [SerializeField] private GameObject tetrominoListItemPrefab;

    [Header("其他 UI 预制件")]
    [SerializeField] private GameObject uiBlockPrefab; // 用于展示单个麻将方块

    [Header("提示信息 (Toast)")]
    [SerializeField] private Text toastText;
    [SerializeField] private CanvasGroup toastCanvasGroup;

    [Header("牌库预览弹窗")]
    [SerializeField] private GameObject poolViewerRoot;       // 父节点 (全屏遮罩)
    [SerializeField] private RectTransform poolViewerContainer; // 动画主体
    [SerializeField] private Button poolViewerCloseButton;     // 关闭按钮 (UI上的)

    [Header("牌库预览 - 容器")]
    [SerializeField] private Transform dotsContainer;  // 筒子 (Dots) 牌面 1-9
    [SerializeField] private Transform bamboosContainer; // 索子 (Bamboos) 牌面 1-9
    [SerializeField] private Transform charactersContainer; // 万子 (Characters) 牌面 1-9


    // ========================================================================
    // 9. 牌型图鉴弹窗 (Pattern Viewer) - NEW
    // ========================================================================
    [Header("牌型图鉴弹窗")]
    [SerializeField] private GameObject patternViewerRoot;       // 父节点 (全屏遮罩)
    [SerializeField] private RectTransform patternViewerContainer; // 动画主体 (您的牌型内容放在这里)
    [SerializeField] private Button patternViewerCloseButton;     // 关闭按钮

    // ========================================================================
    // 10. 模块引用
    // ========================================================================
    [Header("模块引用")]
    [SerializeField] private BlockPool blockPool;

    // ========================================================================
    // 11. 内部状态与动画 Tweens
    // ========================================================================
    // 引用缓存
    private InventoryManager inventoryManager;
    private ScoreManager scoreManager;
    private List<ItemSlotUI> activeItemSlotUIs = new List<ItemSlotUI>();
    private List<ProtocolSlotUI> activeProtocolSlotUIs = new List<ProtocolSlotUI>();
    private List<GameObject> activeTileUIs = new List<GameObject>();

    // 动画 Tweens
    private Tween poolCountBlinkTween;
    private Tween timerBlinkTween;
    private const float POPUP_SLIDE_DURATION = 0.6f;
    private const float POPUP_HIDDEN_Y = -1500f; // 弹窗滑出屏幕外的Y坐标
    private const float POPUP_SHOWN_Y = 0f;      // 弹窗显示时的中心Y坐标 (假设锚点在中心)

    [Header("动画配置")]
    [Tooltip("分数滚动动画持续时间 (秒)")]
    [SerializeField] private float scoreRollDuration = 2.0f;
    private Tween scoreRollTween;
    private long _currentDisplayScoreTarget; // 滚动动画的目标分缓存

    private Dictionary<Transform, int> selectionCounts = new Dictionary<Transform, int>();

    [Header("迷雾效果")]
    [SerializeField] private GameObject mistOverlayRoot; // 迷雾的父节点 (Panel)
    [SerializeField] private UnityEngine.UI.Image[] mistImages; // 迷雾图片数组 (3-5张叠加)
    [SerializeField] private float mistChangeSpeed = 0.5f; // 透明度变化速度
    [SerializeField] private Vector2 mistAlphaRange = new Vector2(0.2f, 0.8f); // 透明度随机范围
    private Coroutine mistCoroutine;

    // ========================================================================
    // 12. 新手教学 (Tutorial) - NEW
    // ========================================================================
    [Header("新手教学")]
    [SerializeField] private GameObject tutorialRoot;       // 教学弹窗父节点 (含遮罩)
    [SerializeField] private RectTransform tutorialContainer;
    [SerializeField] private Button tutorialCloseButton;    // 关闭按钮

    void Awake()
    {
        inventoryManager = FindObjectOfType<InventoryManager>();
        scoreManager = FindObjectOfType<ScoreManager>();
        SetupButtonListeners();

        // 【核心修改】
        // 将关闭按钮的事件委托给 GameManager，确保能触发时间恢复逻辑
        if (poolViewerCloseButton != null)
        {
            poolViewerCloseButton.onClick.AddListener(() =>
            {
                if (GameManager.Instance) GameManager.Instance.TogglePoolViewer();
            });
        }
        if (patternViewerCloseButton != null)
        {
            patternViewerCloseButton.onClick.AddListener(() =>
            {
                if (GameManager.Instance) GameManager.Instance.TogglePatternViewer();
            });
        }
        if (tutorialCloseButton != null)
        {
            tutorialCloseButton.onClick.AddListener(() =>
            {
                if (GameManager.Instance) GameManager.Instance.CloseTutorial();
            });
        }
        if (refreshButton)
        {
            refreshButton.onClick.AddListener(OnRefreshClicked);
        }
    }
    void Start()
    {
        // 初始化金币显示
        if (GameSession.Instance != null)
        {
            UpdateGoldText(GameSession.Instance.CurrentGold);
            // 订阅金币变化事件
            GameSession.OnGoldChanged += UpdateGoldText;
        }
        if (activeProtocolSlotUIs.Count == 0) InitializeProtocolSlots();
        if (activeItemSlotUIs.Count == 0) InitializeItemSlots();
    }
    void OnEnable()
    {
        GameEvents.OnNextBlockReady += UpdateNextBlockPreview;
        ScoreManager.OnScoreChanged += UpdateScoreText;
        GameEvents.OnPoolCountChanged += UpdatePoolCountText;
        if (inventoryManager != null) inventoryManager.OnInventoryChanged += UpdateInventoryUI;
    }

    void OnDisable()
    {
        GameEvents.OnNextBlockReady -= UpdateNextBlockPreview;
        ScoreManager.OnScoreChanged -= UpdateScoreText;
        GameEvents.OnPoolCountChanged -= UpdatePoolCountText;
        if (inventoryManager != null) inventoryManager.OnInventoryChanged -= UpdateInventoryUI;
    }
    void OnDestroy()
    {
        if (GameSession.Instance != null)
            GameSession.OnGoldChanged -= UpdateGoldText;
    }
    void Update()
    {
        if (inventoryManager == null) return;

        // 道具快捷键
        if (Input.GetKeyDown(KeyCode.Alpha1)) TryUseItemByIndex(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) TryUseItemByIndex(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) TryUseItemByIndex(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) TryUseItemByIndex(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) TryUseItemByIndex(4);

        // 【修改】全局点击检测：如果点击了鼠标左键
        if (Input.GetMouseButtonDown(0))
        {
            // 检查鼠标是否点击在 UI 上 (如果不包含 IsPointerOverGameObject，说明点在了场景空地)
            if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                HideAllDeleteButtons();
            }
        }
    }
    private void InitializeItemSlots()
    {
        foreach (Transform child in itemContainer) Destroy(child.gameObject);
        activeItemSlotUIs.Clear();

        // 假设最大道具数为 5
        for (int i = 0; i < 5; i++)
        {
            GameObject go = Instantiate(itemSlotPrefab, itemContainer);
            ItemSlotUI slotUI = go.GetComponent<ItemSlotUI>();

            // 初始化为空，并传入索引
            slotUI.Setup(null, i);

            // 动态添加 Tooltip
            TooltipTriggerUI trigger = go.GetComponent<TooltipTriggerUI>();
            if (trigger == null) trigger = go.AddComponent<TooltipTriggerUI>();

            activeItemSlotUIs.Add(slotUI);
        }
    }
    private void InitializeProtocolSlots()
    {
        // 先清理可能存在的（比如编辑器里留下的）
        foreach (Transform child in protocolContainer) Destroy(child.gameObject);
        activeProtocolSlotUIs.Clear();

        for (int i = 0; i < maxProtocolSlots; i++)
        {
            GameObject go = Instantiate(protocolSlotPrefab, protocolContainer);
            ProtocolSlotUI slotUI = go.GetComponent<ProtocolSlotUI>();

            // 初始化为空
            slotUI.Setup(null);

            // 动态添加 Tooltip (如果需要)
            TooltipTriggerUI trigger = go.GetComponent<TooltipTriggerUI>();
            if (trigger == null) trigger = go.AddComponent<TooltipTriggerUI>();
            // 空状态下 Tooltip 数据设为 null 即可

            activeProtocolSlotUIs.Add(slotUI);
        }
    }
    private void SetupButtonListeners()
    {
        if (continueButton)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(() =>
            {
                DistributeSelectedRewards();
                HideAllDeleteButtons();
                GameManager.Instance.ContinueAfterHu();
            });
        }
        if (restartButton) restartButton.onClick.AddListener(() => { ReturnToMainMenu(); });
        if (endlessModeButton) endlessModeButton.onClick.AddListener(() => GameManager.Instance.StartEndlessMode());
        if (nextStepButton) nextStepButton.onClick.AddListener(OnNextStepClicked);
        if (pauseButton) pauseButton.onClick.AddListener(() => GameManager.Instance.TogglePause());

        // 2. 面板内的 "继续游戏" -> 关闭面板，恢复游戏
        if (resumeButton)
        {
            resumeButton.onClick.AddListener(() =>
            {
                // 调用 GameManager 解除暂停
                GameManager.Instance.TogglePause();
            });
        }

        // 3. 面板内的 "结束游戏" -> 先关面板，再触发 GameOver
        if (endGameButton)
        {
            endGameButton.onClick.AddListener(() =>
            {
                // 先播放关闭动画
                ShowPausePanel(false, () =>
                {
                    // 动画播完后，通知 Manager 结束游戏
                    GameManager.Instance.QuitGameFromPause();
                });
            });
        }
    }
    private void OnNextStepClicked()
    {
        // 1. 停止分数滚动
        if (scoreRollTween != null) { scoreRollTween.Kill(); scoreRollTween = null; }
        if (formulaFinalScoreText) formulaFinalScoreText.text = $"{_currentDisplayScoreTarget:N0}";

        // 2. 切换页面：隐藏第一页，显示第二页
        if (huStage1Panel) huStage1Panel.SetActive(false);

        if (huStage2Panel)
        {
            huStage2Panel.SetActive(true);
            HighlightBars(true);
            // 可选：给第二页单独加个微小的弹动效果，增加交互感
            RectTransform stage2Rect = huStage2Panel.GetComponent<RectTransform>();
            if (stage2Rect)
            {
                stage2Rect.anchoredPosition = new Vector2(0, -500); // 从稍微下面一点的地方
                stage2Rect.DOLocalMove(Vector2.zero, 0.4f).SetEase(Ease.OutBack).SetUpdate(true);
            }
        }
    }
    public void UpdateTimerText(float time, bool isSpecialState = false)
    {
        if (timerText)
        {
            float displayTime = Mathf.Max(0, time);
            timerText.text = $"{displayTime:F0}";

            if (isSpecialState)
            {
                // === 状态 1: 特殊状态 (子弹时间) ===
                StopTimerBlink();
                timerText.color = Color.cyan;
                timerText.DOFade(1f, 0.1f);
                if (AudioManager.Instance) AudioManager.Instance.StopCountdownSound();
            }
            else if (displayTime <= 30f && displayTime > 0f)
            {
                // === 状态 2: 低电量警报 ===

                // 【核心修复】
                // 1. 如果动画还没开始，强制设为红色不透明，并启动动画
                if (timerBlinkTween == null || !timerBlinkTween.IsActive())
                {
                    timerText.color = Color.red;

                    timerBlinkTween = timerText.DOFade(0.5f, 0.5f)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetUpdate(true);
                }
                else
                {
                    // 2. 如果动画正在运行，我们只确保它是红色的，但绝不能覆盖 Alpha！
                    // 获取当前颜色的 Alpha (这个 Alpha 正在被 DOTween 修改)
                    float currentAlpha = timerText.color.a;
                    // 只重置 RGB 为红，保留 Alpha
                    timerText.color = new Color(1f, 0f, 0f, currentAlpha);
                }
                if (AudioManager.Instance) AudioManager.Instance.PlayCountdownSound();
            }
            else
            {
                // === 状态 3: 正常 ===
                StopTimerBlink();
                timerText.color = Color.white;
                timerText.DOFade(1f, 0.1f);
                if (AudioManager.Instance) AudioManager.Instance.StopCountdownSound();
            }
        }
    }

    // 【新增】辅助方法：停止倒计时闪烁
    private void StopTimerBlink()
    {
        if (timerBlinkTween != null)
        {
            timerBlinkTween.Kill();
            timerBlinkTween = null;
        }
    }
    public void UpdateSpeedText(int speedValue, bool isSpecialState = false)
    {
        if (speedText)
        {
            speedText.text = $"{speedValue}";
            speedText.color = isSpecialState ? Color.cyan : Color.white;
        }
    }
    public void UpdateBlockMultiplierText(float multiplier) { if (blockMultiplierText) blockMultiplierText.text = $"{multiplier:F0}"; }
    public void UpdateBaseScoreText(int score) { if (baseScoreText) baseScoreText.text = $"{score}";}
    public void UpdateExtraMultiplierText(float multiplier) { if (extraMultiplierText) extraMultiplierText.text = $"{multiplier:F0}"; }
    private void UpdateScoreText(long newScore) { if (scoreText) scoreText.text = $"{newScore:N0}";}
    private void UpdatePoolCountText(int count) { if (poolCountText) poolCountText.text = $"{count}"; }
    // 【新增】供 GameManager 调用，更新圈数显示 (例如 "第1圈 2/4")
    public void UpdateLoopProgressText(string text)
    {
        if (loopProgressText) loopProgressText.text = text;
    }
    // 【新增】内部使用，更新金币显示
    private void UpdateGoldText(int gold)
    {
        if (goldText) goldText.text = gold.ToString();
    }
    private void UpdateInventoryUI(List<ItemData> items)
    {
        if (activeItemSlotUIs.Count == 0) InitializeItemSlots();

        for (int i = 0; i < activeItemSlotUIs.Count; i++)
        {
            ItemSlotUI slotUI = activeItemSlotUIs[i];

            if (i < items.Count && items[i] != null)
            {
                // === 填入道具 ===
                ItemData item = items[i];
                slotUI.Setup(item, i); // 传入 item 和 索引
            }
            else
            {
                // === 置空 ===
                slotUI.Setup(null, i);
            }
        }
    }

    public void UpdateProtocolUI(List<ProtocolData> protocols)
    {
        if (activeProtocolSlotUIs == null || activeProtocolSlotUIs.Count == 0)
        {
            InitializeProtocolSlots();
        }
        // 遍历所有固定槽位
        for (int i = 0; i < activeProtocolSlotUIs.Count; i++)
        {
            ProtocolSlotUI slotUI = activeProtocolSlotUIs[i];

            if (i < protocols.Count)
            {
                // === 填入条约 ===
                ProtocolData data = protocols[i];
                slotUI.Setup(data);
            }
            else
            {
                // === 置空 ===
                slotUI.Setup(null);
            }
        }
    }

    // 【修改】增加 overrideMultiplier 参数
    public void UpdateTetrominoList(IEnumerable<GameObject> prefabs, float totalMultiplier, Dictionary<string, float> buffs = null, float overrideMultiplier = -1f)
    {
        if (tetrominoListContent == null) return;

        // 清理旧列表
        foreach (Transform child in tetrominoListContent) Destroy(child.gameObject);

        if (prefabs != null && tetrominoListItemPrefab != null)
        {
            // 按 ID 分组统计数量
            var prefabCounts = prefabs.GroupBy(p => p.GetInstanceID()).ToDictionary(g => g.Key, g => g.ToList());

            foreach (var group in prefabCounts.Values)
            {
                var representativePrefab = group.First();
                int count = group.Count;
                var tetromino = representativePrefab.GetComponent<Tetromino>();
                if (tetromino == null) continue;

                var itemGO = Instantiate(tetrominoListItemPrefab, tetrominoListContent);
                var listItemUI = itemGO.GetComponent<TetrominoListItemUI>();
                if (listItemUI == null) continue;

                // --- 1. 计算显示数值与状态 ---
                float displayMult = tetromino.extraMultiplier;
                bool isBuffed = false; // 标记是否被强化

                // 检查是否有 Buff (荣誉勋章效果)
                if (buffs != null && buffs.ContainsKey(representativePrefab.name))
                {
                    displayMult += buffs[representativePrefab.name]; // 加上强化值
                    isBuffed = true; // 激活图标
                }

                // --- 2. 准备显示文本 ---
                string multStr = $"{displayMult:F0}";

                // --- 3. 初始化 UI (传入 isBuffed) ---
                // 注意：这里调用的是 TetrominoListItemUI 中带有 isBuffed 参数的新方法
                listItemUI.InitializeForPrefab(
                    representativePrefab.GetComponent<Tetromino>().uiPrefab,
                    multStr,
                    overrideMultiplier,
                    isBuffed // 关键修复：传递强化状态
                );

                listItemUI.SetStackCount(count);
            }
        }

        if (totalMultiplierText) totalMultiplierText.text = $"{totalMultiplier:F0}";
    }

    public void ShowHuPopup(List<List<int>> huHand, HandAnalysisResult analysis,
                            int baseScore, float blockMultiplier, float extraMultiplier, long finalScore,
                            HuRewardPackage rewards, bool isAdvanced, bool isBerserkerActive,
                            float addedTime, int speedIncrease,
                            List<int> autoBlockIndices = null,   // 改为 List
                            List<int> autoItemIndices = null,    // 改为 List
                            List<int> autoProtocolIndices = null // 改为 List
                            )
    {
        _currentRewardPackage = rewards;
        _isLastHuAdvanced = isAdvanced;
        _selectedBlockIndices.Clear();
        _selectedItemIndices.Clear();
        _selectedProtocolIndices.Clear();
        if (autoBlockIndices != null) _selectedBlockIndices.AddRange(autoBlockIndices);
        if (autoItemIndices != null) _selectedItemIndices.AddRange(autoItemIndices);
        if (autoProtocolIndices != null) _selectedProtocolIndices.AddRange(autoProtocolIndices);
        if (isAdvanced)
        {
            currentBlockContainer = advancedRewardBlockArea;
            currentItemContainer = advancedRewardItemArea;
            currentProtocolContainer = advancedRewardProtocolArea;
        }
        else
        {
            currentBlockContainer = commonRewardBlockArea;
            currentItemContainer = commonRewardItemArea;
            currentProtocolContainer = null; // 普通模式没有条约栏
        }

        if (refreshRoot)
        {
            bool showRefresh = GameManager.Instance.IsAllContentUnlocked();
            refreshRoot.SetActive(showRefresh);
            if (showRefresh) UpdateRefreshCostUI();
        }

        PopulateRewardSlots(_currentRewardPackage, isBerserkerActive, autoBlockIndices, autoItemIndices, autoProtocolIndices);

        selectionCounts.Clear();
        _currentDisplayScoreTarget = finalScore;
        PlayPopupShow(huPopupRoot, huPopupContainer);
        if (huStage1Panel) huStage1Panel.SetActive(true);
        if (huStage2Panel) huStage2Panel.SetActive(false);
        int kongCount = 0;
        if (huHand != null)
        {
            kongCount = huHand.Count(set => set.Count == 4);
        }

        // 2. 获取每杠番数配置
        int fanPerKong = 1; // 默认防空值
        if (GameManager.Instance != null)
        {
            fanPerKong = GameManager.Instance.GetEffectiveFanPerKong();
        }

        // 3. 计算具体番数
        int kongFanTotal = kongCount * fanPerKong;
        int patternFanOnly = analysis.TotalFan - kongFanTotal; // 牌型番 = 总番 - 杠番

        // 4. 更新 UI 文本
        if (patternNameText)
        {
            // 【修改】解析并翻译牌型名称
            if (!string.IsNullOrEmpty(analysis.PatternName))
            {
                // 1. 拆分 Key (MahjongCore 里用 " · " 连接)
                string[] keys = analysis.PatternName.Split(new string[] { " · " }, System.StringSplitOptions.RemoveEmptyEntries);

                // 2. 逐个翻译
                List<string> translatedNames = new List<string>();
                foreach (var key in keys)
                {
                    if (LocalizationManager.Instance)
                    {
                        translatedNames.Add(LocalizationManager.Instance.GetText(key));
                    }
                    else
                    {
                        translatedNames.Add(key); // 兜底
                    }
                }

                // 3. 重新拼接 (这里用空格分隔，或者你可以用 "+" 号)
                patternNameText.text = string.Join("  ", translatedNames);

                // 4. 【重要】刷新字体
                if (LocalizationManager.Instance) LocalizationManager.Instance.UpdateFont(patternNameText);
            }
            else
            {
                patternNameText.text = "";
            }
        }
        if (patternFanText)
        {
            patternFanText.text = patternFanOnly.ToString();
            // 【新增】刷新数字字体
            if (LocalizationManager.Instance) LocalizationManager.Instance.UpdateFont(patternFanText);
        }

        if (kongInfoGroup != null)
        {
            if (kongCount > 0)
            {
                kongInfoGroup.SetActive(true);
                if (kongCountText)
                {
                    kongCountText.text = kongCount.ToString();
                    if (LocalizationManager.Instance) LocalizationManager.Instance.UpdateFont(kongCountText); // 刷新
                }
                if (kongFanText)
                {
                    kongFanText.text = kongFanTotal.ToString();
                    if (LocalizationManager.Instance) LocalizationManager.Instance.UpdateFont(kongFanText); // 刷新
                }
            }
            else
            {
                kongInfoGroup.SetActive(false);
            }
        }

        // =========================================================

        if (formulaBaseScoreText) formulaBaseScoreText.text = $"{baseScore}";
        if (formulaFanBaseText) formulaFanBaseText.text = $"{analysis.BaseMultiplier}";
        if (formulaFanExpText) formulaFanExpText.text = $"{analysis.TotalFan}";
        if (formulaBlockMultText) formulaBlockMultText.text = $"{blockMultiplier:F0}";
        if (formulaExtraMultText) formulaExtraMultText.text = $"{extraMultiplier:F0}";
        if (formulaFinalScoreText)
        {
            // 1. 先重置为 0
            formulaFinalScoreText.text = "0";

            // 2. 杀掉旧动画 (防止连击出bug)
            if (scoreRollTween != null) scoreRollTween.Kill();

            // 3. 创建数字滚动动画
            // 使用 DOTween.To 处理 long 类型 (Docounter 只能处理 int)
            long startValue = 0;
            scoreRollTween = DOTween.To(
                () => startValue,               // Getter
                x => {                          // Setter
                    startValue = (long)x;
                    formulaFinalScoreText.text = $"{startValue:N0}";
                },
                finalScore,                     // Target
                scoreRollDuration               // Duration
            )
            .SetEase(Ease.OutExpo) // 使用 OutExpo 缓动，让数字在快结束时减速，更有质感
            .SetUpdate(true);      // 【关键】忽略 Time.timeScale = 0
        }
        BuildUIHand(huHandDisplayArea, huHand);
        if (nextRoundTimeText) nextRoundTimeText.text = $"+{addedTime:F0}";
        if (nextRoundSpeedText) nextRoundSpeedText.text = $"+{speedIncrease}";
        if (commonRewardPanel) commonRewardPanel.SetActive(!isAdvanced);
        if (advancedRewardPanel) advancedRewardPanel.SetActive(isAdvanced);

    }

    private void DisableOtherOptions(Transform container, RewardOptionUI selected)
    {
        int maxSelection = GameManager.Instance.GetCurrentRewardSelectionLimit();

        if (!selectionCounts.ContainsKey(container)) selectionCounts[container] = 0;
        selectionCounts[container]++;

        // 1. 设置当前按钮为“已选中”状态 (显示勾选标记)
        selected.SetSelected(true);
        // 2. 禁用交互 (防止重复点击)
        selected.SetInteractable(false);

        // 3. 检查是否达到上限
        if (selectionCounts[container] >= maxSelection)
        {
            // 满了：禁用该容器内所有剩余按钮
            foreach (Transform child in container)
            {
                var ui = child.GetComponent<RewardOptionUI>();
                if (ui != null)
                {
                    ui.SetInteractable(false);
                }
            }
        }
    }

    public void HideAllPanels()
    {
        if (huPopupRoot) huPopupRoot.SetActive(false);
        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (poolViewerRoot) poolViewerRoot.SetActive(false);
        if (patternViewerRoot) patternViewerRoot.SetActive(false);
    }
    public void PlayHuPopupExitAnimation(Action onComplete)
    {
        HighlightBars(false);
        PlayPopupHide(huPopupRoot, huPopupContainer, () =>
        {
            HideHuPopup();
            onComplete?.Invoke();
        });
    }
    public void HideHuPopup()
    {
        // 恢复条约栏层级
        HighlightBars(false);

        // 【新增】双重保险：弹窗关闭时，确保删除按钮也被隐藏
        HideAllDeleteButtons();

        if (huPopupRoot) huPopupRoot.SetActive(false);
    }
    public void ShowGameEndPanel(bool isWin, long finalScore, bool isNewHighScore,
                                 float completionRate, int baseGold, int extraGold,
                                 string reasonKey = "", bool isEndlessMode = false)
    {
        if (gameOverPanel)
        {
            // 1. 激活父节点 (遮罩立即显示)
            gameOverPanel.SetActive(true);
        }
        if (gameOverContainer)
        {
            // 确保锚点居中
            gameOverContainer.anchorMin = new Vector2(0.5f, 0.5f);
            gameOverContainer.anchorMax = new Vector2(0.5f, 0.5f);
            gameOverContainer.pivot = new Vector2(0.5f, 0.5f);
        }
        PlayPopupShow(gameOverPanel, gameOverContainer);
        if (AudioManager.Instance != null)
        {
            // 1. 关闭背景音乐
            AudioManager.Instance.StopBGM();

            // 2. 播放对应的胜利/失败音效
            if (AudioManager.Instance.SoundLibrary != null)
            {
                if (isWin)
                {
                    // 播放胜利音效
                    AudioManager.Instance.PlaySFX(AudioManager.Instance.SoundLibrary.gameWin);
                }
                else
                {
                    // 播放失败音效
                    AudioManager.Instance.PlaySFX(AudioManager.Instance.SoundLibrary.gameOver);
                }
            }
        }
        if (gameOverTitleText)
        {
            string key = isWin ? "GAME_OVER_CLEAR" : "GAME_OVER_LOSE"; // 确保 CSV 里有这两个 Key
            if (LocalizationManager.Instance)
            {
                gameOverTitleText.text = LocalizationManager.Instance.GetText(key);
                LocalizationManager.Instance.UpdateFont(gameOverTitleText);
            }
            else
            {
                gameOverTitleText.text = isWin ? "恭喜过关！" : "游戏结束"; // 兜底
            }
        }
        if (gameOverReasonText)
        {
            // 只有在失败(非Win) 且 有具体原因Key时才显示
            if (!isWin && !string.IsNullOrEmpty(reasonKey))
            {
                // 1. 先激活父节点 (背景容器)
                if (gameOverReasonText.transform.parent != null)
                {
                    gameOverReasonText.transform.parent.gameObject.SetActive(true);
                }

                // 2. 激活文本并设置内容
                gameOverReasonText.gameObject.SetActive(true);
                if (LocalizationManager.Instance)
                {
                    gameOverReasonText.text = LocalizationManager.Instance.GetText(reasonKey);
                    LocalizationManager.Instance.UpdateFont(gameOverReasonText);
                }
                else
                {
                    gameOverReasonText.text = reasonKey;
                }
            }
            else
            {
                // 1. 隐藏父节点 (连带背景一起隐藏)
                if (gameOverReasonText.transform.parent != null)
                {
                    gameOverReasonText.transform.parent.gameObject.SetActive(false);
                }

                // 2. 隐藏文本本体 (双重保险)
                gameOverReasonText.gameObject.SetActive(false);
            }
        }
        if (finalScoreText)
        {
            finalScoreText.text = $"{finalScore:N0}";
            if (LocalizationManager.Instance) LocalizationManager.Instance.UpdateFont(finalScoreText);
        }
        if (completionRateText)
        {
            // 显示百分比，例如 "完成度: 120%"
            float percent = completionRate * 100f;
            completionRateText.text = $"{percent:F0}%";
        }
        if (rewardInfoRoot)
        {
            // 判定条件更新：
            // 1. 普通模式：总是显示 (!isEndlessMode)
            // 2. 无尽模式：只要 基础奖励 > 0 或者 额外奖励 > 0 就显示
            bool shouldShow = !isEndlessMode || baseGold > 0 || extraGold > 0;

            if (shouldShow)
            {
                rewardInfoRoot.SetActive(true);

                // 显示基础奖励
                if (baseRewardText) baseRewardText.text = $"{baseGold}";

                // 显示/隐藏 额外奖励
                if (extraRewardRoot)
                {
                    if (extraGold > 0)
                    {
                        extraRewardRoot.SetActive(true);
                        if (extraRewardText) extraRewardText.text = $"{extraGold}";
                    }
                    else
                    {
                        extraRewardRoot.SetActive(false);
                    }
                }
            }
            else
            {
                // 无尽模式且没有任何奖励 -> 隐藏整个父节点
                rewardInfoRoot.SetActive(false);
            }
        }
        if (newHighScoreIndicator) newHighScoreIndicator.SetActive(isNewHighScore);
        if (endlessModeButton) endlessModeButton.gameObject.SetActive(isWin);
        if (baseRewardText != null && baseRewardText.transform.parent != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(baseRewardText.transform.parent.GetComponent<RectTransform>());
        }

        // 2. 刷新“额外奖励”所在的那一行 (文本+图标 的父节点)
        if (extraRewardText != null && extraRewardText.transform.parent != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(extraRewardText.transform.parent.GetComponent<RectTransform>());
        }

        // 3. 最后刷新最外层的总容器 (它包含上面两行)
        if (rewardInfoRoot != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rewardInfoRoot.GetComponent<RectTransform>());
        }
    }
    public void PlayGameOverExitAnimation(Action onComplete)
    {
        PlayPopupHide(gameOverPanel, gameOverContainer, onComplete);
    }
    private void BuildUIHand(Transform container, List<List<int>> hand)
    {
        if (container == null) return;

        // 1. 清理旧内容
        foreach (Transform child in container) Destroy(child.gameObject);

        // 2. 确保主容器有 HorizontalLayoutGroup 组件 (用于排列"组")
        HorizontalLayoutGroup layoutGroup = container.GetComponent<HorizontalLayoutGroup>();
        if (layoutGroup == null) layoutGroup = container.gameObject.AddComponent<HorizontalLayoutGroup>();

        // 配置主容器布局
        layoutGroup.spacing = uiSetSpacing;         // 设置组间距
        layoutGroup.childAlignment = TextAnchor.MiddleCenter; // 居中对齐
        layoutGroup.childControlWidth = false;      // 由子物体自己控制宽度
        layoutGroup.childControlHeight = false;     // 由子物体自己控制高度
        layoutGroup.childForceExpandWidth = false;
        layoutGroup.childForceExpandHeight = false;

        // 3. 遍历每一组牌 (面子/将牌)
        foreach (var set in hand)
        {
            // --- 创建“组容器” ---
            GameObject setContainer = new GameObject("SetContainer", typeof(RectTransform), typeof(LayoutElement));
            setContainer.transform.SetParent(container, false);
            RectTransform containerRT = setContainer.GetComponent<RectTransform>();

            // 【关键修复】设置 Pivot 为 (0, 0.5)，即“左中”
            // 这样 x=0 就在最左边，牌会向右填充，完美填满 LayoutElement 预留的空间
            containerRT.pivot = new Vector2(0f, 0.5f);
            // 计算该组的视觉宽度
            // 对子(2张) = 2宽
            // 刻子/顺子(3张) = 3宽
            // 杠(4张) = 3宽 (第4张叠在上面，不占横向空间)
            int visualCount = (set.Count == 4) ? 3 : set.Count;
            float setWidth = visualCount * uiTileWidth;

            // 设置 LayoutElement 告诉父级 LayoutGroup 这个组有多宽
            LayoutElement le = setContainer.GetComponent<LayoutElement>();
            le.minWidth = setWidth;
            le.preferredWidth = setWidth;
            le.minHeight = uiTileHeight + (set.Count == 4 ? uiKongOffsetY : 0); // 如果是杠，稍微高一点
            le.preferredHeight = le.minHeight;

            // --- 填充牌 ---
            for (int i = 0; i < set.Count; i++)
            {
                int blockId = set[i];
                GameObject tileObj = Instantiate(uiBlockPrefab, setContainer.transform);

                // 设置图片
                var img = tileObj.GetComponent<Image>();
                if (img) img.sprite = blockPool.GetSpriteForBlock(blockId);

                // 禁用逻辑组件
                var bu = tileObj.GetComponent<BlockUnit>();
                if (bu != null) bu.DisablePoolUI();

                // 设置 RectTransform (手动定位)
                RectTransform rt = tileObj.GetComponent<RectTransform>();

                // 锚点设为左下角，方便计算
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.zero;
                rt.pivot = Vector2.zero;
                rt.sizeDelta = new Vector2(uiTileWidth, uiTileHeight);

                // --- 核心：位置计算 ---
                float xPos = i * uiTileWidth;
                float yPos = 0;

                // 特殊处理：杠牌的第4张 (Index 3)
                // 放在第2张 (Index 1) 的正上方
                if (set.Count == 4 && i == 3)
                {
                    xPos = 1 * uiTileWidth; // 和第2张牌X一样
                    yPos = uiKongOffsetY;   // Y轴抬高
                }

                rt.anchoredPosition = new Vector2(xPos, yPos);
            }
        }
    }

    private void UpdateNextBlockPreview(GameObject prefab, List<int> ids)
    {
        if (nextBlockPreviewArea == null) return;
        if (currentPreviewObject != null) Destroy(currentPreviewObject);

        Tetromino tetrominoInfo = prefab.GetComponent<Tetromino>();
        GameObject prefabToSpawn = (tetrominoInfo != null && tetrominoInfo.uiPrefab != null) ? tetrominoInfo.uiPrefab : prefab;
        currentPreviewObject = Instantiate(prefabToSpawn, nextBlockPreviewArea);
        currentPreviewObject.transform.localPosition = Vector3.zero;

        if (currentPreviewObject.GetComponent<Tetromino>())
            currentPreviewObject.GetComponent<Tetromino>().enabled = false;

        var blockUnits = currentPreviewObject.GetComponentsInChildren<BlockUnit>();
        var sortedBlockUnits = blockUnits.OrderBy(bu => bu.gameObject.name).ToArray();
        for (int i = 0; i < sortedBlockUnits.Length && i < ids.Count; i++)
        {
            sortedBlockUnits[i].Initialize(ids[i], blockPool); // 【修改】使用排序后的数组
        }
        var allUnits = currentPreviewObject.GetComponentsInChildren<BlockUnit>();
        foreach (var unit in allUnits)
        {
            unit.DisablePoolUI();
        }
        bool isInsufficient = ids.Contains(-1);
        UpdatePoolCountVisuals(isInsufficient);
    }

    private void ReturnToMainMenu()
    {
        // 【核心修复】
        // 1. 先禁用 GameManager，彻底切断它的 Update 循环
        // 这样即使下面恢复了时间，GameManager 也不会再跑一帧去触发音效了
        if (GameManager.Instance)
        {
            GameManager.Instance.enabled = false;
        }

        // 2. 再次强制停止音效 (双重保险)
        if (AudioManager.Instance)
        {
            AudioManager.Instance.StopCountdownSound();
        }

        // 3. 恢复时间流速 (现在是安全的了)
        Time.timeScale = 1f;

        // 4. 加载场景
        SceneManager.LoadScene("MainMenuScene");
    }

    public void ShowPausePanel(bool show, Action onComplete = null)
    {
        if (show)
        {
            // 显示：播放滑入动画
            PlayPopupShow(pausePanel, pauseContainer);
        }
        else
        {
            // 隐藏：播放滑出动画，并在结束后执行回调
            PlayPopupHide(pausePanel, pauseContainer, onComplete);
        }
    }
    // 【新增】这个方法用于显示目标和进度条
    public void UpdateTargetScoreDisplay(int target, int reward, bool isBonusActive = false)
    {
        if (targetProgressBar)
        {
            targetProgressBar.gameObject.SetActive(true);
            targetProgressBar.maxValue = target; // 设置最大值为目标分
        }
        if (currentScoreForBarText) currentScoreForBarText.gameObject.SetActive(true);

        if (goldRewardText)
        {
            goldRewardText.text = $"{reward}";
            goldRewardText.color = isBonusActive ? Color.red : new Color32(0, 95, 115, 255);
        }
    }

    // 【新增】这个方法用于显示“无尽模式”
    public void UpdateTargetScoreDisplay(string endlessText)
    {
        if (targetProgressBar) targetProgressBar.gameObject.SetActive(false);
        if (currentScoreForBarText) currentScoreForBarText.gameObject.SetActive(false);
        if (goldRewardText) goldRewardText.text = endlessText;
    }
    public void UpdateScoreProgress(long currentScore)
    {
        if (targetProgressBar && targetProgressBar.gameObject.activeSelf)
        {
            targetProgressBar.value = currentScore;
            if (currentScoreForBarText)
            {
                currentScoreForBarText.text = $"{targetProgressBar.maxValue:N0}";
            }
        }
    }
    public void OnProtocolSlotClicked(ProtocolSlotUI clickedSlot)
    {
        // 1. 记录点击之前该按钮的状态
        bool wasActive = clickedSlot.IsDeleteButtonActive();

        // 2. 先隐藏所有条约的删除按钮 (满足需求2：点击其他隐藏当前)
        HideAllDeleteButtons();

        // 3. Toggle 逻辑 (满足需求1：再次点击隐藏)
        // 如果点击之前是关闭的，现在才显示；
        // 如果点击之前是开启的，第2步已经把它关了，这里就不再打开，实现了“关闭”效果。
        if (!wasActive)
        {
            clickedSlot.ShowDeleteButton();
        }
    }
    public void HideAllDeleteButtons()
    {
        // 1. 隐藏条约的
        foreach (var slot in activeProtocolSlotUIs)
        {
            if (slot != null) slot.HideDeleteButton();
        }

        // 2. 【新增】隐藏道具的
        foreach (var slot in activeItemSlotUIs)
        {
            if (slot != null) slot.HideDeleteButton();
        }
    }
    public void ShowToast(string message)
    {
        if (toastText == null || toastCanvasGroup == null) return;

        // 杀掉旧动画防止冲突
        toastCanvasGroup.DOKill();

        toastText.text = message;
        if (LocalizationManager.Instance)
        {
            LocalizationManager.Instance.UpdateFont(toastText);
        }
        if (toastText.transform.parent != null)
        {
            RectTransform layoutRoot = toastText.transform.parent.GetComponent<RectTransform>();
            if (layoutRoot != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRoot);
            }
        }
        toastCanvasGroup.alpha = 1; // 立刻显示

        // 持续 0.8s 后，用 0.4s 淡出
        Sequence seq = DOTween.Sequence();
        seq.AppendInterval(0.8f);
        seq.Append(toastCanvasGroup.DOFade(0, 0.4f));
        seq.SetUpdate(true); // 确保在 Time.timeScale=0 (暂停/胡牌) 时也能播放动画！
    }
    private void UpdatePoolCountVisuals(bool isInsufficient)
    {
        if (poolCountText == null) return;

        if (isInsufficient)
        {
            // === 状态：不足 ===
            // 1. 变红
            poolCountText.color = Color.red;

            // 2. 闪烁 (如果还没开始闪)
            if (poolCountBlinkTween == null || !poolCountBlinkTween.IsActive())
            {
                // 0.5秒内透明度降到 0.2，循环往复 (Yoyo)
                poolCountBlinkTween = poolCountText.DOFade(0.5f, 0.5f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetUpdate(true); // 确保在暂停时也能看到警示（可选）
            }
        }
        else
        {
            // === 状态：正常 ===
            // 1. 停止闪烁
            if (poolCountBlinkTween != null)
            {
                poolCountBlinkTween.Kill();
                poolCountBlinkTween = null;
            }

            // 2. 恢复白色和不透明
            poolCountText.color = Color.white;
            poolCountText.DOFade(1f, 0.1f); // 快速恢复 Alpha 到 1
        }
    }

    public void SetEndlessModeLabelActive(bool isActive)
    {
        if (endlessModeLabel)
        {
            endlessModeLabel.SetActive(isActive);
        }
    }
    public bool IsPoolViewerActive()
    {
        return poolViewerRoot != null && poolViewerRoot.activeInHierarchy;
    }
    public void ShowPoolViewer()
    {
        if (poolViewerRoot == null || poolViewerContainer == null) return;
        PlayPopupShow(poolViewerRoot, poolViewerContainer);
        RefreshPoolDisplay();
    }
    public void HidePoolViewer()
    {
        // 【使用通用方法替换】
        PlayPopupHide(poolViewerRoot, poolViewerContainer, () =>
        {
            ClearActiveTileUIs();
        });
    }
    private void ClearActiveTileUIs()
    {
        // 清理并回收所有生成的麻将 UI
        foreach (var go in activeTileUIs)
        {
            if (go != null) Destroy(go);
        }
        activeTileUIs.Clear();
    }
    public void RefreshPoolDisplay()
    {
        // 1. 清理
        ClearActiveTileUIs();
        dotsContainer.DestroyChildren();
        bamboosContainer.DestroyChildren();
        charactersContainer.DestroyChildren();

        // 增加对 GameManager.Instance.BlockPool 的判空，更安全
        if (GameManager.Instance == null || GameManager.Instance.BlockPool == null) return;

        // 2. 获取数据
        // 【修改】使用 GameManager 中的 BlockPool，而不是本地可能为空的变量
        var targetPool = GameManager.Instance.BlockPool;
        var allIDs = targetPool.GetRemainingTileIDs();

        // 3. 统计
        int[] tileCounts = new int[27];
        foreach (int id in allIDs)
        {
            int normalizedId = id % 27;
            if (normalizedId >= 0 && normalizedId < 27) tileCounts[normalizedId]++;
        }

        // 4. 容器映射
        var containers = new Dictionary<int, Transform>()
        {
            { 0, dotsContainer }, { 1, bamboosContainer }, { 2, charactersContainer }
        };

        // 5. 生成 27 个格子
        for (int i = 0; i < 27; i++)
        {
            int suit = i / 9;
            if (containers.TryGetValue(suit, out var parentTransform))
            {
                GameObject go = Instantiate(uiBlockPrefab, parentTransform);

                var bu = go.GetComponent<BlockUnit>();
                if (bu != null)
                {
                    // 【关键修复】这里传入 targetPool (即 GameManager.Instance.BlockPool)
                    bu.InitializeForPoolViewer(i, tileCounts[i], targetPool);
                }

                activeTileUIs.Add(go);
            }
        }
    }
    public bool IsHuPopupActive()
    {
        return huPopupRoot != null && huPopupRoot.activeInHierarchy;
    }
    public bool IsGameOverPanelActive()
    {
        return gameOverPanel != null && gameOverPanel.activeInHierarchy;
    }
    public bool IsPatternViewerActive()
    {
        return patternViewerRoot != null && patternViewerRoot.activeInHierarchy;
    }
    public void ShowPatternViewer()
    {
        if (patternViewerRoot == null || patternViewerContainer == null) return;
        if (IsPoolViewerActive()) HidePoolViewer();
        PlayPopupShow(patternViewerRoot, patternViewerContainer);
    }
    public void HidePatternViewer()
    {
        PlayPopupHide(patternViewerRoot, patternViewerContainer);
    }
    public void SetMistActive(bool isActive)
    {
        if (mistOverlayRoot == null) return;

        mistOverlayRoot.SetActive(isActive);

        if (isActive)
        {
            if (mistCoroutine != null) StopCoroutine(mistCoroutine);
            mistCoroutine = StartCoroutine(AnimateMist());
        }
        else
        {
            if (mistCoroutine != null) StopCoroutine(mistCoroutine);
        }
    }
    private System.Collections.IEnumerator AnimateMist()
    {
        // 为每张图片随机一个初始目标透明度
        float[] targetAlphas = new float[mistImages.Length];
        for (int i = 0; i < mistImages.Length; i++)
        {
            targetAlphas[i] = UnityEngine.Random.Range(mistAlphaRange.x, mistAlphaRange.y);
        }

        while (true)
        {
            for (int i = 0; i < mistImages.Length; i++)
            {
                if (mistImages[i] == null) continue;

                // 1. 获取当前颜色
                Color color = mistImages[i].color;
                float currentAlpha = color.a;

                // 2. 向目标透明度平滑过渡
                float newAlpha = Mathf.MoveTowards(currentAlpha, targetAlphas[i], mistChangeSpeed * Time.unscaledDeltaTime);
                mistImages[i].color = new Color(color.r, color.g, color.b, newAlpha);

                // 3. 如果达到了目标，随即寻找下一个目标
                if (Mathf.Abs(newAlpha - targetAlphas[i]) < 0.01f)
                {
                    targetAlphas[i] = UnityEngine.Random.Range(mistAlphaRange.x, mistAlphaRange.y);
                }
            }
            yield return null;
        }
    }
    private void HighlightBars(bool highlight)
    {
        // 1. 条约栏
        HighlightTarget(protocolBarPanel != null ? protocolBarPanel : (protocolContainer != null ? protocolContainer.gameObject : null), highlight);

        // 2. 道具栏
        HighlightTarget(itemBarPanel != null ? itemBarPanel : (itemContainer != null ? itemContainer.gameObject : null), highlight);
    }
    private void HighlightTarget(GameObject target, bool highlight)
    {
        if (target == null) return;

        if (highlight)
        {
            Canvas canvas = target.GetComponent<Canvas>();
            if (canvas == null) canvas = target.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = 3000; // 提升层级

            if (target.GetComponent<GraphicRaycaster>() == null) target.AddComponent<GraphicRaycaster>();
        }
        else
        {
            GraphicRaycaster gr = target.GetComponent<GraphicRaycaster>();
            if (gr != null) Destroy(gr);
            Canvas canvas = target.GetComponent<Canvas>();
            if (canvas != null) Destroy(canvas);
        }
    }
    public void OnItemSlotClicked(ItemSlotUI clickedSlot)
    {
        bool wasActive = clickedSlot.IsDeleteButtonActive();

        // 隐藏所有的删除按钮 (条约的 + 道具的)
        HideAllDeleteButtons();

        // Toggle
        if (!wasActive)
        {
            clickedSlot.ShowDeleteButton();
        }
    }
    public void ShowTutorialPanel(bool show)
    {
        if (show)
        {
            PlayPopupShow(tutorialRoot, tutorialContainer);
        }
        else
        {
            PlayPopupHide(tutorialRoot, tutorialContainer);
        }
    }
    private void PlayPopupShow(GameObject root, RectTransform container)
    {
        if (root == null) return;

        // 1. 激活根节点 (遮罩)
        root.SetActive(true);

        if (container != null)
        {
            // 2. 杀掉该容器上正在运行的所有动画 (防止快速开关导致冲突)
            container.DOKill();

            // 3. 重置位置到屏幕下方
            container.anchoredPosition = new Vector2(0, POPUP_HIDDEN_Y);

            // 4. 执行动画
            container.DOAnchorPosY(POPUP_SHOWN_Y, POPUP_SLIDE_DURATION)
                .SetEase(Ease.OutBack) // 您的最爱：弹性进入
                .SetUpdate(true);      // 忽略 Time.timeScale (暂停时也能播)
        }
    }
    private void PlayPopupHide(GameObject root, RectTransform container, Action onComplete = null)
    {
        // 如果容器存在，且根节点是开着的，才播放动画
        if (container != null && root != null && root.activeSelf)
        {
            container.DOKill();

            // 退出动画稍微快一点点 (0.5s)，使用 InBack (先回缩再飞走)
            container.DOAnchorPosY(POPUP_HIDDEN_Y, 0.5f)
                .SetEase(Ease.InBack)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    root.SetActive(false);
                    onComplete?.Invoke();
                });
        }
        else
        {
            // 如果面板本来就没开，直接执行关闭逻辑
            if (root) root.SetActive(false);
            onComplete?.Invoke();
        }
    }
    public bool IsTutorialActive()
    {
        return tutorialRoot != null && tutorialRoot.activeSelf;
    }
    private void OnRefreshClicked()
    {
        if (GameManager.Instance.TrySpendRefreshCost())
        {
            // 锁定逻辑：只要列表不为空，就视为需要保留
            bool keepBlocks = _selectedBlockIndices.Count > 0;
            bool keepItems = _selectedItemIndices.Count > 0;
            bool keepProtocols = _selectedProtocolIndices.Count > 0;

            _currentRewardPackage = GameManager.Instance.RefreshRewardPackage(
                _currentRewardPackage,
                keepBlocks,
                keepItems,
                keepProtocols,
                _isLastHuAdvanced
            );

            PopulateRewardSlots(_currentRewardPackage); // 这里的列表依然保留着旧的索引

            // 【重要】刷新后，需要清理掉越界的索引 (防止刷新后数量变少导致报错)
            // 但通常刷新数量不变，或者保留了旧的选项，所以索引依然有效。
            // 如果刷新逻辑是“保留旧物体”，那么索引对应的内容没变。

            RestoreSelections();
            UpdateRefreshCostUI();
        }
        else
        {
            if (AudioManager.Instance) AudioManager.Instance.PlayBuyFailSound();
            string msg = LocalizationManager.Instance ? LocalizationManager.Instance.GetText("HU_LACK_GOLD") : "金币不足！";
            ShowToast(msg);
        }
    }
    // 【新增】更新刷新按钮状态 (价格、颜色)
    private void UpdateRefreshUI()
    {
        if (refreshRoot == null || !refreshRoot.activeSelf) return;

        int cost = GameManager.Instance.GetCurrentRefreshCost();
        int playerGold = GameSession.Instance.CurrentGold;

        if (refreshCostText)
        {
            refreshCostText.text = $"{cost} G";
            // 金币不足变红
            refreshCostText.color = (playerGold >= cost) ? Color.white : Color.red;
        }

        // 如果金币不足，也可以选择让按钮不可点击 interactable = false
        // refreshButton.interactable = (playerGold >= cost);
    }
    private void UpdateRefreshCostUI()
    {
        if (!refreshCostText) return;

        int cost = GameManager.Instance.GetCurrentRefreshCost();
        // 获取当前持有金币 (增加空值检查防止报错)
        int playerGold = GameSession.Instance != null ? GameSession.Instance.CurrentGold : 0;

        // 【修改】格式改为： 消耗 / 持有 (例如 "100 / 5000")
        refreshCostText.text = $"{cost}/{playerGold}";

        // 颜色逻辑保持不变：钱不够变红
        refreshCostText.color = (playerGold >= cost) ? new Color32(0, 95, 115, 255) : Color.red;
    }
    private void PopulateRewardSlots(HuRewardPackage rewards, bool isBerserker = false,
                                     List<int> autoBlockIndices = null,
                                     List<int> autoItemIndices = null,
                                     List<int> autoProtocolIndices = null)
    {
        // 1. 生成方块栏
        if (currentBlockContainer != null)
        {
            GenerateColumn(currentBlockContainer, rewards.BlockChoices.Count, (obj, i) => {
                var ui = obj.GetComponent<RewardOptionUI>();
                if (ui) ui.Setup(rewards.BlockChoices[i], null); // 传入 null，由 SetupSelectionClick 接管

                SetupSelectionClick(obj, i, 0);

                if (isBerserker && ui)
                {
                    ui.SetInteractable(false);
                    // 检查是否在列表中
                    if (autoBlockIndices != null && autoBlockIndices.Contains(i)) ui.SetSelected(true);
                }
            });
        }

        // 2. 生成道具栏
        if (currentItemContainer != null)
        {
            GenerateColumn(currentItemContainer, rewards.ItemChoices.Count, (obj, i) => {
                var ui = obj.GetComponent<RewardOptionUI>();
                if (ui) ui.Setup(rewards.ItemChoices[i], null);

                SetupSelectionClick(obj, i, 1);

                if (isBerserker && ui)
                {
                    ui.SetInteractable(false);
                    if (autoItemIndices != null && autoItemIndices.Contains(i)) ui.SetSelected(true);
                }
            });
        }

        // 3. 生成条约栏
        if (currentProtocolContainer != null)
        {
            GenerateColumn(currentProtocolContainer, rewards.ProtocolChoices.Count, (obj, i) => {
                var ui = obj.GetComponent<RewardOptionUI>();
                if (ui) ui.Setup(rewards.ProtocolChoices[i], null);

                SetupSelectionClick(obj, i, 2);

                if (isBerserker && ui)
                {
                    ui.SetInteractable(false);
                    if (autoProtocolIndices != null && autoProtocolIndices.Contains(i)) ui.SetSelected(true);
                }
            });
        }

        // 刷新高亮状态 (仅在非狂战士模式下需要，狂战士模式下上面已经设置了 Selected)
        if (!isBerserker) RestoreSelections();
    }

    private void RestoreSelections()
    {
        HighlightContainer(currentBlockContainer, _selectedBlockIndices);
        HighlightContainer(currentItemContainer, _selectedItemIndices);
        HighlightContainer(currentProtocolContainer, _selectedProtocolIndices);
    }

    private void GenerateColumn(Transform parent, int count, Action<GameObject, int> onInit)
    {
        if (!parent || !rewardOptionPrefab) return;
        List<GameObject> oldChildren = new List<GameObject>();
        foreach (Transform child in parent) oldChildren.Add(child.gameObject);

        foreach (GameObject go in oldChildren)
        {
            if (go != null)
            {
                go.transform.SetParent(null); // 立即断开父子关系
                Destroy(go);                  // 稍后销毁
            }
        }

        // 生成新物体
        for (int i = 0; i < count; i++)
        {
            GameObject go = Instantiate(rewardOptionPrefab, parent);
            onInit(go, i);
        }
    }

    private void SetupSelectionClick(GameObject obj, int index, int typeId)
    {
        Button btn = obj.GetComponent<Button>();
        if (!btn) return;

        btn.onClick.RemoveAllListeners();

        btn.onClick.AddListener(() => {

            // 1. 判断意图：是“选中”还是“取消选中”？
            // (如果点击的是当前已经选中的项，说明玩家想取消，这种情况下不需要检查容量)
            List<int> targetList = null;
            if (typeId == 0) targetList = _selectedBlockIndices;
            else if (typeId == 1) targetList = _selectedItemIndices;
            else if (typeId == 2) targetList = _selectedProtocolIndices;

            if (targetList == null) return;

            // 1. 判断意图：是“选中”还是“取消选中”？
            bool isSelecting = !targetList.Contains(index);

            // 2. 如果是“选中”操作，执行容量检查
            if (isSelecting)
            {
                // 获取当前允许的选择上限 (SSSVIP=2, 默认=1)
                int selectionLimit = GameManager.Instance.GetCurrentRewardSelectionLimit();
                bool willIncreaseCount = targetList.Count < selectionLimit;
                // --- 容量检查 (仅在列表为空，即选择第一个时检查，防止溢出) ---
                // 如果已选了一个想选第二个，这里暂不做严格容量检查(视作玩家自己管理)
                if (willIncreaseCount)
                {
                    if (typeId == 1) // 道具
                    {
                        int emptySlots = GameManager.Instance.GetRemainingItemSpace();
                        // 逻辑：当前已选数量 >= 剩余空位，说明没地儿放新的了
                        if (targetList.Count >= emptySlots)
                        {
                            if (AudioManager.Instance) AudioManager.Instance.PlayBuyFailSound();
                            string msg = LocalizationManager.Instance ? LocalizationManager.Instance.GetText("HU_FULL_ITEM") : "背包空间不足！";
                            ShowToast(msg);
                            return;
                        }
                    }
                    else if (typeId == 2) // 条约
                    {
                        int emptySlots = GameManager.Instance.GetRemainingProtocolSpace();
                        if (targetList.Count >= emptySlots)
                        {
                            if (AudioManager.Instance) AudioManager.Instance.PlayBuyFailSound();
                            string msg = LocalizationManager.Instance ? LocalizationManager.Instance.GetText("HU_FULL_PROTOCOL") : "条约栏空间不足！";
                            ShowToast(msg);
                            return;
                        }
                    }
                }

                // --- 正常的 FIFO 替换逻辑 ---
                if (targetList.Count >= selectionLimit)
                {
                    targetList.RemoveAt(0); // 移除最早选的
                }

                targetList.Add(index);
            }
            else
            {
                // 取消选中
                targetList.Remove(index);
            }

            // 3. 通过检查，播放正常的点击音效
            if (AudioManager.Instance) AudioManager.Instance.PlayButtonClickSound();

            // 5. 刷新 UI 显示
            RestoreSelections();
        });
    }
    private void DistributeSelectedRewards()
    {
        // 1. 方块
        foreach (int idx in _selectedBlockIndices)
        {
            if (idx >= 0 && idx < _currentRewardPackage.BlockChoices.Count)
            {
                var block = _currentRewardPackage.BlockChoices[idx];
                int count = 1;
                // 幸运瓶盖 (简单起见，每次触发都消耗)
                if (GameManager.Instance.luckyCapStack > 0)
                {
                    count += GameManager.Instance.luckyCapStack;
                    GameManager.Instance.ConsumeLuckyCap();
                }
                for (int k = 0; k < count; k++) GameManager.Instance.Spawner.AddTetrominoToPool(block);
            }
        }

        // 2. 道具
        foreach (int idx in _selectedItemIndices)
        {
            if (idx >= 0 && idx < _currentRewardPackage.ItemChoices.Count)
            {
                var item = _currentRewardPackage.ItemChoices[idx];
                GameManager.Instance.Inventory.AddItem(item);
            }
        }

        // 3. 条约
        foreach (int idx in _selectedProtocolIndices)
        {
            if (idx >= 0 && idx < _currentRewardPackage.ProtocolChoices.Count)
            {
                var proto = _currentRewardPackage.ProtocolChoices[idx];
                GameManager.Instance.AddProtocol(proto);
            }
        }
    }
    private void HighlightContainer(Transform container, List<int> selectedIndices)
    {
        if (container == null) return;

        for (int i = 0; i < container.childCount; i++)
        {
            var ui = container.GetChild(i).GetComponent<RewardOptionUI>();
            if (ui)
            {
                // 只要在列表中，就设为选中
                bool isSelected = selectedIndices.Contains(i);
                ui.SetSelected(isSelected);
                ui.SetInteractable(true);
            }
        }
    }
    private void TryUseItemByIndex(int index)
    {
        // 确保 activeItemSlotUIs 列表已初始化且索引有效
        if (activeItemSlotUIs != null && index >= 0 && index < activeItemSlotUIs.Count)
        {
            var slot = activeItemSlotUIs[index];
            if (slot != null)
            {
                // 调用 Slot 上的方法，这样就会播放音效并进行相同的逻辑检查
                slot.AttemptUseItem();
            }
        }
    }
}
