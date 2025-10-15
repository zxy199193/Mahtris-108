// FileName: GameUIController.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    void Awake()
    {
        inventoryManager = FindObjectOfType<InventoryManager>();
        scoreManager = FindObjectOfType<ScoreManager>();
        SetupButtonListeners();
        InitializeTooltipTriggers();
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
    public void UpdateSpeedText(float percent) { if (speedText) speedText.text = $"{percent:F0}%"; }
    public void UpdateBlockMultiplierText(float multiplier) { if (blockMultiplierText) blockMultiplierText.text = $"{multiplier:F0}"; }
    public void UpdateBaseScoreText(int score) { if (baseScoreText) baseScoreText.text = $"{score}"; }
    public void UpdateExtraMultiplierText(float multiplier) { if (extraMultiplierText) extraMultiplierText.text = $"{multiplier:F0}"; }
    private void UpdateScoreText(int newScore) { if (scoreText) scoreText.text = $"{newScore}"; }
    private void UpdatePoolCountText(int count) { if (poolCountText) poolCountText.text = $"{count}"; }

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
                    itemTooltipTriggers[i].SetData(items[i].itemName, items[i].itemDescription);
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
                    protocolTooltipTriggers[i].SetData(protocols[i].protocolName, protocols[i].protocolDescription);
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

    public void UpdateTetrominoList(IEnumerable<GameObject> prefabs, float totalMultiplier)
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

            listItemUI.InitializeForPrefab(tetromino.uiPrefab, $"x{tetromino.extraMultiplier:F0}");

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
                            HuRewardPackage rewards, bool isAdvanced)
    {
        if (huPopupPanel) huPopupPanel.SetActive(true);

        if (patternNameText) patternNameText.text = $"{analysis.PatternName} ({analysis.TotalFan}番)";
        if (formulaBaseScoreText) formulaBaseScoreText.text = $"{baseScore}";
        if (formulaFanBaseText) formulaFanBaseText.text = "2"; // (注：如果"高端局"条约实装，这里需要改成动态的)
        if (formulaFanExpText) formulaFanExpText.text = $"{analysis.TotalFan}";
        if (formulaBlockMultText) formulaBlockMultText.text = $"{blockMultiplier:F0}";
        if (formulaExtraMultText) formulaExtraMultText.text = $"{extraMultiplier:F0}";
        if (formulaFinalScoreText) formulaFinalScoreText.text = $"{finalScore}";
        if (huCycleText) huCycleText.text = isAdvanced ? "4/4" : $"{scoreManager.GetHuCountInCycle()}/4";

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

            if (choice is GameObject blockPrefab)
            {
                var tetromino = blockPrefab.GetComponent<Tetromino>();
                rewardUI.InitializeForPrefab(
                 tetromino.uiPrefab,$"x{tetromino.extraMultiplier:F0}", $"{blockPrefab.name}",$"将该方块加入方块列表，方块倍率 +{tetromino.extraMultiplier:F0}。", (clickedUI) =>
                 { 
                 FindObjectOfType<Spawner>().AddTetrominoToPool(blockPrefab);
                 DisableOtherOptions(container, clickedUI);
                 },ShowTooltip,HideTooltip); 
            }
            else if (choice is ItemData itemData)
            {
                rewardUI.InitializeForSprite(itemData.itemIcon, $"{itemData.itemName}", itemData.itemDescription,
                (clickedUI) => {
                    FindObjectOfType<InventoryManager>().AddItem(itemData);
                    DisableOtherOptions(container, clickedUI);
                }, ShowTooltip, HideTooltip);
            }
            else if (choice is ProtocolData protocolData)
            {
                rewardUI.InitializeForSprite(protocolData.protocolIcon, $"{protocolData.protocolName}", protocolData.protocolDescription,
                (clickedUI) => {
                    GameManager.Instance.AddProtocol(protocolData);
                    DisableOtherOptions(container, clickedUI);
                }, ShowTooltip, HideTooltip);
            }
        }
    }

    private void DisableOtherOptions(Transform container, RewardOptionUI selected)
    {
        foreach (Transform child in container)
        {
            var rewardOption = child.GetComponent<RewardOptionUI>();
            if (rewardOption != null)
            {
                rewardOption.SetInteractable(false);
            }
        }
    }

    private void ShowTooltip(string title, string desc) { if (TooltipSystem.Instance) TooltipSystem.Instance.Show(title, desc); }
    private void HideTooltip() { if (TooltipSystem.Instance) TooltipSystem.Instance.Hide(); }

    public void HideAllPanels()
    {
        if (huPopupPanel) huPopupPanel.SetActive(false);
        if (gameOverPanel) gameOverPanel.SetActive(false);
    }

    public void HideHuPopup() { if (huPopupPanel) huPopupPanel.SetActive(false); }
    public void ShowGameEndPanel(bool isWin, int finalScore, bool isNewHighScore)
    {
        if (gameOverPanel) gameOverPanel.SetActive(true);
        if (gameOverTitleText) gameOverTitleText.text = isWin ? "恭喜过关！" : "游戏结束";
        if (finalScoreText) finalScoreText.text = $"最终得分: {finalScore}";
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
        if (goldRewardText) goldRewardText.text = $"奖励: {reward}金";
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
                currentScoreForBarText.text = $"{currentScore} / {targetProgressBar.maxValue}";
            }
        }
    }
}
