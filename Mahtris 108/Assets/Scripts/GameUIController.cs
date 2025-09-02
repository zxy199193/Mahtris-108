// FileName: GameUIController.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

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
    [SerializeField] private Text targetScoreText;
    [SerializeField] private Text speedText;
    [SerializeField] private Text blockMultiplierText;

    [Header("下一个方块预览")]
    [SerializeField] private Transform nextBlockPreviewArea;

    [Header("道具栏")]
    [SerializeField] private List<Button> itemSlotButtons;
    [SerializeField] private List<Image> itemSlotIcons;

    [Header("胡牌弹窗")]
    [SerializeField] private GameObject huPopupPanel;
    [SerializeField] private Transform huHandDisplayArea;
    [SerializeField] private Text patternNameText;
    [SerializeField] private Text formulaText;
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

    private GameObject currentPreviewObject;
    private InventoryManager inventoryManager;

    void Awake()
    {
        inventoryManager = FindObjectOfType<InventoryManager>();
        SetupButtonListeners();
    }

    void OnEnable()
    {
        GameEvents.OnNextBlockReady += UpdateNextBlockPreview;
        GameEvents.OnScoreChanged += UpdateScoreText;
        GameEvents.OnPoolCountChanged += UpdatePoolCountText;
        if (inventoryManager != null) inventoryManager.OnInventoryChanged += UpdateInventoryUI;
    }

    void OnDisable()
    {
        GameEvents.OnNextBlockReady -= UpdateNextBlockPreview;
        GameEvents.OnScoreChanged -= UpdateScoreText;
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
    }

    public void UpdateTimerText(float time) { if (timerText) timerText.text = $"时间: {Mathf.Max(0, time):F0}"; }
    public void UpdateTargetScoreText(string text) { if (targetScoreText) targetScoreText.text = $"目标: {text}"; }
    public void UpdateSpeedText(float percent) { if (speedText) speedText.text = $"速度: {percent:F0}%"; }
    public void UpdateBlockMultiplierText(float multiplier) { if (blockMultiplierText) blockMultiplierText.text = $"方块倍率: x{multiplier:F1}"; }
    private void UpdateScoreText(int newScore) { if (scoreText) scoreText.text = $"得分: {newScore}"; }
    private void UpdatePoolCountText(int count) { if (poolCountText) poolCountText.text = $"牌库剩余: {count}"; }

    private void UpdateInventoryUI(List<ItemData> items)
    {
        for (int i = 0; i < itemSlotIcons.Count; i++)
        {
            if (i < items.Count && items[i] != null)
            {
                itemSlotIcons[i].sprite = items[i].itemIcon;
                itemSlotIcons[i].enabled = true;
                itemSlotButtons[i].interactable = true;
            }
            else
            {
                itemSlotIcons[i].sprite = null;
                itemSlotIcons[i].enabled = false;
                itemSlotButtons[i].interactable = false;
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
            if (tetromino == null || tetromino.uiPrefab == null) continue;

            var itemGO = Instantiate(tetrominoListItemPrefab, tetrominoListContent);
            var listItemUI = itemGO.GetComponent<TetrominoListItemUI>();
            if (listItemUI == null) continue;

            if (listItemUI.multiplierText) listItemUI.multiplierText.text = $"x{tetromino.extraMultiplier:F1}";
            if (listItemUI.shapeContainer) Instantiate(tetromino.uiPrefab, listItemUI.shapeContainer);

            if (listItemUI.countText != null)
            {
                listItemUI.countText.gameObject.SetActive(count > 1);
                listItemUI.countText.text = $"x{count}";
            }
        }
        if (totalMultiplierText) totalMultiplierText.text = $"总倍率: x{totalMultiplier:F1}";
    }

    public void ShowHuPopup(List<List<int>> huHand, HandAnalysisResult analysis,
                            int baseScore, float blockMultiplier, float extraMultiplier, long finalScore,
                            HuRewardPackage rewards, bool isAdvanced)
    {
        if (huPopupPanel) huPopupPanel.SetActive(true);

        if (patternNameText) patternNameText.text = $"{analysis.PatternName} ({analysis.TotalFan}番)";
        if (formulaText) formulaText.text = $"{baseScore} × 2^{analysis.TotalFan} × {blockMultiplier:F1} × {extraMultiplier:F1} = {finalScore}";
        if (huCycleText) huCycleText.text = isAdvanced ? "4/4" : $"{FindObjectOfType<ScoreManager>().GetHuCountInCycle()}/4";

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
                rewardUI.Initialize(tetromino.shapeUISprite, $"方块: {blockPrefab.name}", $"倍率: x{tetromino.extraMultiplier}",
                (clickedUI) => {
                    FindObjectOfType<Spawner>().AddTetrominoToPool(blockPrefab);
                    DisableOtherOptions(container, clickedUI);
                }, ShowTooltip, HideTooltip);
            }
            else if (choice is ItemData itemData)
            {
                rewardUI.Initialize(itemData.itemIcon, $"道具: {itemData.itemName}", itemData.itemDescription,
                (clickedUI) => {
                    FindObjectOfType<InventoryManager>().AddItem(itemData);
                    DisableOtherOptions(container, clickedUI);
                }, ShowTooltip, HideTooltip);
            }
            else if (choice is ProtocolData protocolData)
            {
                rewardUI.Initialize(protocolData.protocolIcon, $"条约: {protocolData.protocolName}", protocolData.protocolDescription,
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
                rewardOption.SetInteractable(child.gameObject == selected.gameObject);
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
    public void ShowGameOverPanel() { if (gameOverPanel) gameOverPanel.SetActive(true); }

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

        currentPreviewObject = Instantiate(prefab, nextBlockPreviewArea);
        currentPreviewObject.transform.localPosition = Vector3.zero;

        if (currentPreviewObject.GetComponent<Tetromino>())
            currentPreviewObject.GetComponent<Tetromino>().enabled = false;

        var blockUnits = currentPreviewObject.GetComponentsInChildren<BlockUnit>();
        for (int i = 0; i < blockUnits.Length && i < ids.Count; i++)
        {
            blockUnits[i].Initialize(ids[i], blockPool);
        }
    }

    private void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenuScene");
    }
}
