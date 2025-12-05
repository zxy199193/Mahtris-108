// FileName: GameUIController.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;



// 新增：用于在UI和逻辑间传递奖励数据的结构体
public class HuRewardPackage
{
    public List<GameObject> BlockChoices = new List<GameObject>();
    public List<ItemData> ItemChoices = new List<ItemData>();
    public List<ProtocolData> ProtocolChoices = new List<ProtocolData>();
}

public class GameUIController : MonoBehaviour
{
    [Header("文本显示")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text poolCountText;
    [SerializeField] private Text timerText;
    [SerializeField] private Text speedText;
    [SerializeField] private Text blockMultiplierText;
    [SerializeField] private Text baseScoreText; // 新增
    [SerializeField] private Text extraMultiplierText; // 新增
    [SerializeField] private Text goldText;
    [SerializeField] private Text loopProgressText;

    [Header("目标进度条")]
    [SerializeField] private Slider targetProgressBar;
    [SerializeField] private Text currentScoreForBarText; // (可选)显示 "1200 / 5000"
    [SerializeField] private Text goldRewardText;

    [Header("下一个方块预览")]
    [SerializeField] private Transform nextBlockPreviewArea;

    [Header("道具栏")]
    [SerializeField] private List<Button> itemSlotButtons;
    [SerializeField] private List<Image> itemSlotIcons;

    [Header("条约栏")]
    [SerializeField] private List<Image> protocolIconSlots; // 新增

    [Header("胡牌弹窗")]
    [SerializeField] private GameObject huPopupPanel;
    [SerializeField] private Transform huHandDisplayArea;
    [SerializeField] private Text patternNameText;
    [Header("胡牌得分公式 (拆分)")]
    [SerializeField] private Text formulaBaseScoreText;
    [SerializeField] private Text formulaFanBaseText;
    [SerializeField] private Text formulaFanExpText;
    [SerializeField] private Text formulaBlockMultText;
    [SerializeField] private Text formulaExtraMultText;
    [SerializeField] private Text formulaFinalScoreText;
    [SerializeField] private Button continueButton;
    [SerializeField] private Text huCycleText;

    [Header("胡牌奖励区域")]
    [SerializeField] private GameObject commonRewardPanel;
    [SerializeField] private Transform commonRewardBlockArea;
    [SerializeField] private Transform commonRewardItemArea;
    [SerializeField] private GameObject advancedRewardPanel;
    [SerializeField] private Transform advancedRewardBlockArea;
    [SerializeField] private Transform advancedRewardItemArea;
    [SerializeField] private Transform advancedRewardProtocolArea;

    [Header("游戏结束")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button restartButton;

    [Header("Tetromino列表")]
    [SerializeField] private Transform tetrominoListContent;
    [SerializeField] private Text totalMultiplierText;

    [Header("UI预制件")]
    [SerializeField] private GameObject rewardOptionPrefab;
    [SerializeField] private GameObject uiBlockPrefab;
    [SerializeField] private GameObject tetrominoListItemPrefab;

    [Header("模块引用")]
    [SerializeField] private BlockPool blockPool;

    [Header("游戏结束面板")]
    [SerializeField] private Text gameOverTitleText;
    [SerializeField] private Text finalScoreText;
    [SerializeField] private GameObject newHighScoreIndicator;
    [SerializeField] private Button endlessModeButton;

    private GameObject currentPreviewObject;
    private InventoryManager inventoryManager;
    private List<ItemData> currentItems;
    private List<ProtocolData> currentProtocols;
    private ScoreManager scoreManager;

    private List<TooltipTriggerUI> itemTooltipTriggers = new List<TooltipTriggerUI>();
    private List<TooltipTriggerUI> protocolTooltipTriggers = new List<TooltipTriggerUI>();

    [Header("暂停功能")]
    [SerializeField] private Button pauseButton;
    [SerializeField] private Image pauseButtonIcon;
    [SerializeField] private Text pauseCountText;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Sprite playIcon; // 在Inspector中拖入“播放”图标
    [SerializeField] private Sprite pauseIcon; // 在Inspector中拖入“暂停”图标
    private Dictionary<Transform, int> selectionCounts = new Dictionary<Transform, int>();
    void Awake()
    {
        inventoryManager = FindObjectOfType<InventoryManager>();
        scoreManager = FindObjectOfType<ScoreManager>();
        SetupButtonListeners();
        InitializeTooltipTriggers();
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
        if (Input.GetKeyDown(KeyCode.Alpha1)) inventoryManager.UseItem(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) inventoryManager.UseItem(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) inventoryManager.UseItem(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) inventoryManager.UseItem(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) inventoryManager.UseItem(4);
    }

    private void SetupButtonListeners()
    {
        if (continueButton) continueButton.onClick.AddListener(() => { if (AudioManager.Instance) AudioManager.Instance.PlayButtonClickSound(); GameManager.Instance.ContinueAfterHu(); });
        if (restartButton) restartButton.onClick.AddListener(() => { if (AudioManager.Instance) AudioManager.Instance.PlayButtonClickSound(); ReturnToMainMenu(); });

        for (int i = 0; i < itemSlotButtons.Count; i++)
        {
            int slotIndex = i;
            itemSlotButtons[i].onClick.AddListener(() => { if (AudioManager.Instance) AudioManager.Instance.PlayButtonClickSound(); if (inventoryManager) inventoryManager.UseItem(slotIndex); });
        }
        if (pauseButton) pauseButton.onClick.AddListener(() => GameManager.Instance.TogglePause());
        if (endlessModeButton) endlessModeButton.onClick.AddListener(() => GameManager.Instance.StartEndlessMode());
    }

    public void UpdateTimerText(float time) { if (timerText) timerText.text = $"{Mathf.Max(0, time):F0}"; }
    public void UpdateSpeedText(int speedValue)
    {
        if (speedText) speedText.text = $"{speedValue}";
    }
    public void UpdateBlockMultiplierText(float multiplier) { if (blockMultiplierText) blockMultiplierText.text = $"{multiplier:F0}"; }
    public void UpdateBaseScoreText(int score) { if (baseScoreText) baseScoreText.text = $"{score}"; }
    public void UpdateExtraMultiplierText(float multiplier) { if (extraMultiplierText) extraMultiplierText.text = $"{multiplier:F0}"; }
    private void UpdateScoreText(int newScore) { if (scoreText) scoreText.text = $"{newScore}"; }
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
        currentItems = items;
        for (int i = 0; i < itemSlotIcons.Count; i++)
        {
            if (i < items.Count && items[i] != null)
            {
                itemSlotIcons[i].sprite = items[i].itemIcon;
                itemSlotIcons[i].enabled = true;
                itemSlotButtons[i].interactable = true;

                if (itemTooltipTriggers.Count > i && itemTooltipTriggers[i] != null)
                {
                    // 【修改】传递图标和传奇标签信息给 TooltipTrigger
                    itemTooltipTriggers[i].SetData(
                        items[i].itemName,
                        items[i].itemDescription,
                        items[i].itemIcon,
                        items[i].isLegendary
                    );
                }
            }
            else
            {
                itemSlotIcons[i].sprite = null;
                itemSlotIcons[i].enabled = false;
                itemSlotButtons[i].interactable = false;

                if (itemTooltipTriggers.Count > i && itemTooltipTriggers[i] != null)
                {
                    itemTooltipTriggers[i].SetData(null, null); // 清除数据
                }
            }
        }
    }

    public void UpdateProtocolUI(List<ProtocolData> protocols)
    {
        currentProtocols = protocols;
        for (int i = 0; i < protocolIconSlots.Count; i++)
        {
            if (i < protocols.Count && protocols[i] != null)
            {
                protocolIconSlots[i].sprite = protocols[i].protocolIcon;
                protocolIconSlots[i].enabled = true;

                if (protocolTooltipTriggers.Count > i && protocolTooltipTriggers[i] != null)
                {
                    // 【修改】传递图标和传奇标签信息给 TooltipTrigger
                    protocolTooltipTriggers[i].SetData(
                        protocols[i].protocolName,
                        protocols[i].protocolDescription,
                        protocols[i].protocolIcon,
                        protocols[i].isLegendary
                    );
                }
            }
            else
            {
                protocolIconSlots[i].sprite = null;
                protocolIconSlots[i].enabled = false;

                if (protocolTooltipTriggers.Count > i && protocolTooltipTriggers[i] != null)
                {
                    protocolTooltipTriggers[i].SetData(null, null); // 清除数据
                }
            }
        }
    }

    // 【修改】增加 overrideMultiplier 参数
    public void UpdateTetrominoList(IEnumerable<GameObject> prefabs, float totalMultiplier, float overrideMultiplier = -1f)
    {
        if (tetrominoListContent == null) return;
        foreach (Transform child in tetrominoListContent) Destroy(child.gameObject);
        if (tetrominoListItemPrefab == null) return;

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

            // 【修改】传递 overrideMultiplier
            listItemUI.InitializeForPrefab(representativePrefab.GetComponent<Tetromino>().uiPrefab, $"{tetromino.extraMultiplier:F0}", overrideMultiplier);

            if (listItemUI.countText != null)
            {
                listItemUI.countText.gameObject.SetActive(count > 1);
                listItemUI.countText.text = $"x{count}";
            }
        }
        if (totalMultiplierText) totalMultiplierText.text = $"{totalMultiplier:F0}";
    }

    public void ShowHuPopup(List<List<int>> huHand, HandAnalysisResult analysis,
                            int baseScore, float blockMultiplier, float extraMultiplier, long finalScore,
                            HuRewardPackage rewards, bool isAdvanced, bool isBerserkerActive)
    {
        selectionCounts.Clear();
        if (huPopupPanel)
        {
            // 1. 立即激活面板
            huPopupPanel.SetActive(true);

            // 确保 RectTransform 锚点居中（Inspector 设置）
            RectTransform rectTransform = huPopupPanel.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f); // 屏幕中心
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f); // 轴心居中

            // 设置初始位置（本地坐标）
            rectTransform.anchoredPosition = new Vector2(0, 1200); // 使用 anchoredPosition 而非 position

            // 执行本地移动动画
            rectTransform.DOLocalMove(new Vector2(0, 0), 0.8f)
                .SetEase(Ease.OutBounce)
                .SetUpdate(true);
        }

        if (patternNameText) patternNameText.text = $"{analysis.PatternName} ({analysis.TotalFan}番)";
        if (formulaBaseScoreText) formulaBaseScoreText.text = $"{baseScore}";
        if (formulaFanBaseText) formulaFanBaseText.text = "2"; // (注：如果"高端局"条约实装，这里需要改成动态的)
        if (formulaFanExpText) formulaFanExpText.text = $"{analysis.TotalFan}";
        if (formulaBlockMultText) formulaBlockMultText.text = $"{blockMultiplier:F0}";
        if (formulaExtraMultText) formulaExtraMultText.text = $"{extraMultiplier:F0}";
        if (formulaFinalScoreText) formulaFinalScoreText.text = $"{finalScore}";
        if (huCycleText) huCycleText.text = isAdvanced ? "4/4" : $"第X圈 第{scoreManager.GetHuCountInCycle()}轮";

        BuildUIHand(huHandDisplayArea, huHand);

        if (commonRewardPanel) commonRewardPanel.SetActive(!isAdvanced);
        if (advancedRewardPanel) advancedRewardPanel.SetActive(isAdvanced);

        if (isAdvanced)
        {
            PopulateRewardOptions(advancedRewardBlockArea, rewards.BlockChoices);
            PopulateRewardOptions(advancedRewardItemArea, rewards.ItemChoices);
            PopulateRewardOptions(advancedRewardProtocolArea, rewards.ProtocolChoices);
        }
        else
        {
            PopulateRewardOptions(commonRewardBlockArea, rewards.BlockChoices);
            PopulateRewardOptions(commonRewardItemArea, rewards.ItemChoices);
        }
    }

    private void PopulateRewardOptions<T>(Transform container, List<T> choices) where T : class
    {
        if (container == null) return;
        foreach (Transform child in container) Destroy(child.gameObject);
        if (choices == null) return;

        foreach (var choice in choices)
        {
            var optionGO = Instantiate(rewardOptionPrefab, container);
            var rewardUI = optionGO.GetComponent<RewardOptionUI>();
            if (rewardUI == null) continue;

            // 使用重载的 Setup 方法
            if (choice is GameObject blockPrefab)
            {
                rewardUI.Setup(blockPrefab, (clickedUI) => {

                    // 【新增】幸运瓶盖逻辑
                    // 检查是否激活了幸运瓶盖
                    if (GameManager.Instance.isLuckyCapActive)
                    {
                        // 效果：获得 2 个相同的方块
                        FindObjectOfType<Spawner>().AddTetrominoToPool(blockPrefab);
                        FindObjectOfType<Spawner>().AddTetrominoToPool(blockPrefab);

                        // 消耗道具状态
                        GameManager.Instance.ConsumeLuckyCap();
                        Debug.Log("幸运瓶盖生效！获得双倍方块。");
                    }
                    else
                    {
                        // 正常逻辑：获得 1 个方块
                        FindObjectOfType<Spawner>().AddTetrominoToPool(blockPrefab);
                    }

                    DisableOtherOptions(container, clickedUI);
                });
            }
            else if (choice is ItemData itemData)
            {
                rewardUI.Setup(itemData, (clickedUI) => {
                    FindObjectOfType<InventoryManager>().AddItem(itemData);
                    DisableOtherOptions(container, clickedUI);
                });
            }
            else if (choice is ProtocolData protocolData)
            {
                rewardUI.Setup(protocolData, (clickedUI) => {
                    GameManager.Instance.AddProtocol(protocolData);
                    DisableOtherOptions(container, clickedUI);
                });
            }
        }
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
        if (huPopupPanel) huPopupPanel.SetActive(false);
        if (gameOverPanel) gameOverPanel.SetActive(false);
    }

    public void HideHuPopup() { if (huPopupPanel) huPopupPanel.SetActive(false); }
    public void ShowGameEndPanel(bool isWin, int finalScore, bool isNewHighScore)
    {
        if (gameOverPanel)
        {
            // 1. 立即激活面板
            gameOverPanel.SetActive(true);

            // 确保 RectTransform 锚点居中（Inspector 设置）
            RectTransform rectTransform = gameOverPanel.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f); // 屏幕中心
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f); // 轴心居中

            // 设置初始位置（本地坐标）
            rectTransform.anchoredPosition = new Vector2(0, 1200); // 使用 anchoredPosition 而非 position

            // 执行本地移动动画
            rectTransform.DOLocalMove(new Vector2(0, 0), 0.8f)
                .SetEase(Ease.OutBounce)
                .SetUpdate(true);
        }
        if (gameOverTitleText) gameOverTitleText.text = isWin ? "恭喜过关！" : "游戏结束";
        if (finalScoreText) finalScoreText.text = $"{finalScore}";
        if (newHighScoreIndicator) newHighScoreIndicator.SetActive(isNewHighScore);
        if (endlessModeButton) endlessModeButton.gameObject.SetActive(isWin);
    }

    private void BuildUIHand(Transform container, List<List<int>> hand)
    {
        if (container == null) return;
        foreach (Transform child in container) Destroy(child.gameObject);

        foreach (var set in hand)
        {
            foreach (var blockId in set)
            {
                var uiBlock = Instantiate(uiBlockPrefab, container);
                if (uiBlock) uiBlock.GetComponent<Image>().sprite = blockPool.GetSpriteForBlock(blockId);
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
    }

    private void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenuScene");
    }

    private void InitializeTooltipTriggers()
    {
        itemTooltipTriggers.Clear();
        foreach (var button in itemSlotButtons)
        {
            // 我们将触发器添加到按钮的GameObject上
            TooltipTriggerUI trigger = button.gameObject.GetComponent<TooltipTriggerUI>();
            if (trigger == null)
            {
                trigger = button.gameObject.AddComponent<TooltipTriggerUI>();
            }
            itemTooltipTriggers.Add(trigger);
        }

        protocolTooltipTriggers.Clear();
        foreach (var image in protocolIconSlots)
        {
            // 我们将触发器添加到图标的GameObject上
            TooltipTriggerUI trigger = image.gameObject.GetComponent<TooltipTriggerUI>();
            if (trigger == null)
            {
                trigger = image.gameObject.AddComponent<TooltipTriggerUI>();
            }
            protocolTooltipTriggers.Add(trigger);
        }
    }
    public void UpdatePauseUI(bool isCurrentlyPaused, int count)
    {
        if (pauseButtonIcon) pauseButtonIcon.sprite = isCurrentlyPaused ? playIcon : pauseIcon;
        if (pauseCountText) pauseCountText.text = count.ToString();
        // 只有在次数 > 0 或 已经暂停时，按钮才可交互
        if (pauseButton) pauseButton.interactable = (count > 0 || isCurrentlyPaused);
    }

    public void ShowPausePanel(bool show)
    {
        if (pausePanel) pausePanel.SetActive(show);
    }
    // 【新增】这个方法用于显示目标和进度条
    public void UpdateTargetScoreDisplay(int target, int reward)
    {
        if (targetProgressBar)
        {
            targetProgressBar.gameObject.SetActive(true);
            targetProgressBar.maxValue = target;
        }
        if (currentScoreForBarText) currentScoreForBarText.gameObject.SetActive(true);
        if (goldRewardText) goldRewardText.text = $"{reward}";
    }

    // 【新增】这个方法用于显示“无尽模式”
    public void UpdateTargetScoreDisplay(string endlessText)
    {
        if (targetProgressBar) targetProgressBar.gameObject.SetActive(false);
        if (currentScoreForBarText) currentScoreForBarText.gameObject.SetActive(false);
        if (goldRewardText) goldRewardText.text = endlessText;
    }
    public void UpdateScoreProgress(int currentScore)
    {
        if (targetProgressBar && targetProgressBar.gameObject.activeSelf)
        {
            targetProgressBar.value = currentScore;
            if (currentScoreForBarText)
            {
                currentScoreForBarText.text = $"{targetProgressBar.maxValue}";
            }
        }
    }
}
