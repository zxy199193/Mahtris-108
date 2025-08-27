// FileName: GameUIController.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class GameUIController : MonoBehaviour
{
    // (�����ֶ���������һ����ͬ)
    #region Fields
    [Header("�ı���ʾ")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text poolCountText;
    [SerializeField] private Text timerText;
    [SerializeField] private Text targetScoreText;
    [SerializeField] private Text speedText;

    [Header("��һ������Ԥ��")]
    [SerializeField] private Transform nextBlockPreviewArea;

    [Header("������")]
    [SerializeField] private List<Button> itemSlotButtons;
    [SerializeField] private List<Image> itemSlotIcons;

    [Header("���Ƶ���")]
    [SerializeField] private GameObject huPopupPanel;
    [SerializeField] private Transform huHandDisplayArea;
    [SerializeField] private Text patternNameText;
    [SerializeField] private Text formulaText;
    [SerializeField] private Button continueButton;
    [SerializeField] private List<Button> levelButtons;
    [SerializeField] private Transform chosenTetrominoArea;
    [SerializeField] private Button grantItemButton;

    [Header("��Ϸ����")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button restartButton;

    [Header("Tetromino�б�")]
    [SerializeField] private Transform tetrominoListContent;
    [SerializeField] private Text totalMultiplierText;

    [Header("UIԤ�Ƽ�")]
    [SerializeField] private GameObject uiBlockPrefab;
    [SerializeField] private GameObject tetrominoListItemPrefab;

    [Header("ģ������")]
    [SerializeField] private BlockPool blockPool;

    private GameObject currentPreviewObject;
    #endregion

    private InventoryManager inventoryManager;

    void Awake()
    {
        inventoryManager = FindObjectOfType<InventoryManager>();
        SetupButtonListeners();
    }

    private void SetupButtonListeners()
    {
        if (continueButton) continueButton.onClick.AddListener(() => { if (AudioManager.Instance) AudioManager.Instance.PlayButtonClickSound(); GameManager.Instance.ContinueAfterHu(); });
        if (restartButton) restartButton.onClick.AddListener(() => { if (AudioManager.Instance) AudioManager.Instance.PlayButtonClickSound(); ReturnToMainMenu(); });
        if (grantItemButton) grantItemButton.onClick.AddListener(() => { if (AudioManager.Instance) AudioManager.Instance.PlayButtonClickSound(); GameManager.Instance.GrantRandomItem(); });

        for (int i = 0; i < itemSlotButtons.Count; i++)
        {
            int slotIndex = i;
            itemSlotButtons[i].onClick.AddListener(() => { if (AudioManager.Instance) AudioManager.Instance.PlayButtonClickSound(); if (inventoryManager) inventoryManager.UseItem(slotIndex); });
        }

        // --- ��������---
        // ���Ӷ� Level Buttons �б�����ü��
        if (levelButtons == null || levelButtons.Count == 0)
        {
            Debug.LogWarning("GameUIController: 'Level Buttons' �б�Ϊ�գ����ƽ������ܽ��޷�ʹ�á�����Inspector��Ϊ���б�ֵ��");
        }
        else
        {
            for (int i = 0; i < levelButtons.Count; i++)
            {
                int levelIndex = i;
                levelButtons[i].onClick.AddListener(() => {
                    if (AudioManager.Instance) AudioManager.Instance.PlayButtonClickSound();
                    GameManager.Instance.OnLevelButtonClicked(levelIndex);
                });
            }
        }
    }

    // (�������з�������һ����ȫ��ͬ)
    #region Unchanged Code
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

    public void UpdateTimerText(float time) { if (timerText) timerText.text = $"ʱ��: {Mathf.Max(0, time):F0}"; }
    public void UpdateTargetScoreText(string text) { if (targetScoreText) targetScoreText.text = $"Ŀ��: {text}"; }
    public void UpdateSpeedText(float percent) { if (speedText) speedText.text = $"�ٶ�: {percent:F0}%"; }

    private void UpdateScoreText(int newScore) { if (scoreText) scoreText.text = $"�÷�: {newScore}"; }
    private void UpdatePoolCountText(int count) { if (poolCountText) poolCountText.text = $"�ƿ�ʣ��: {count}"; }

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
        if (totalMultiplierText) totalMultiplierText.text = $"�ܱ���: x{totalMultiplier:F1}";
    }

    public void ShowHuPopup(List<List<int>> huHand, HandAnalysisResult analysis, int baseScore, float multiplier, long finalScore)
    {
        if (huPopupPanel) huPopupPanel.SetActive(true);
        foreach (var btn in levelButtons) btn.interactable = true;
        if (chosenTetrominoArea) foreach (Transform child in chosenTetrominoArea) Destroy(child.gameObject);
        SetGrantItemButtonInteractable(true);
        if (patternNameText) patternNameText.text = $"{analysis.PatternName} ({analysis.TotalFan}��)";
        if (formulaText) formulaText.text = $"{baseScore} �� (2^{analysis.TotalFan}) �� {multiplier:F1} = {finalScore}";
        BuildUIHand(huHandDisplayArea, huHand);
    }

    public void DisplayChosenTetrominoAndLockButtons(GameObject chosenPrefab)
    {
        foreach (var btn in levelButtons) btn.interactable = false;
        if (chosenTetrominoArea == null) return;
        foreach (Transform child in chosenTetrominoArea) Destroy(child.gameObject);
        if (chosenPrefab == null) return;

        var tetromino = chosenPrefab.GetComponent<Tetromino>();
        if (tetromino != null && tetromino.uiPrefab != null)
        {
            var itemGO = Instantiate(tetrominoListItemPrefab, chosenTetrominoArea);
            var listItemUI = itemGO.GetComponent<TetrominoListItemUI>();
            if (listItemUI != null)
            {
                if (listItemUI.multiplierText) listItemUI.multiplierText.text = $"x{tetromino.extraMultiplier:F1}";
                if (listItemUI.shapeContainer) Instantiate(tetromino.uiPrefab, listItemUI.shapeContainer);
                if (listItemUI.countText != null) listItemUI.countText.gameObject.SetActive(false);
            }
        }
    }

    public void SetGrantItemButtonInteractable(bool interactable)
    {
        if (grantItemButton) grantItemButton.interactable = interactable;
    }

    public void HideAllPanels()
    {
        if (huPopupPanel) huPopupPanel.SetActive(false);
        if (gameOverPanel) gameOverPanel.SetActive(false);
    }
    public void HideHuPopup() { if (huPopupPanel) huPopupPanel.SetActive(false); }
    public void ShowGameOverPanel() { if (gameOverPanel) gameOverPanel.SetActive(true); }
    public void HideGameOverPanel() { if (gameOverPanel) gameOverPanel.SetActive(false); }
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
    #endregion
}
