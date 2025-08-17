// FileName: GameUIController.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class GameUIController : MonoBehaviour
{
    // (字段声明和Awake/Event Subscription/Text Updates等方法与上一版相同)
    #region Unchanged Code
    [Header("通用UI元素")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text poolCountText;

    [Header("下一个方块预览")]
    [SerializeField] private Transform nextBlockPreviewArea;

    [Header("胡牌弹窗")]
    [SerializeField] private GameObject huPopupPanel;
    [SerializeField] private Transform huHandDisplayArea;
    [SerializeField] private Text patternNameText;
    [SerializeField] private Text formulaText;
    [SerializeField] private Button continueButton;
    [SerializeField] private List<Button> levelButtons;
    [SerializeField] private Transform chosenTetrominoArea;

    [Header("Tetromino列表")]
    [SerializeField] private Transform tetrominoListContent;
    [SerializeField] private Text totalMultiplierText;

    [Header("游戏结束")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button restartButton;

    [Header("UI预制件")]
    [Tooltip("用于动态拼接胡牌牌型的【单个UI麻将牌】预制件")]
    [SerializeField] private GameObject uiBlockPrefab;
    [Tooltip("用于在列表中显示【单个Tetromino】的UI项预制件 (应挂载TetrominoListItemUI脚本)")]
    [SerializeField] private GameObject tetrominoListItemPrefab;

    [Header("模块引用")]
    [SerializeField] private BlockPool blockPool;

    private GameObject currentPreviewObject;

    void Awake()
    {
        if (continueButton) continueButton.onClick.AddListener(() => GameManager.Instance.ContinueAfterHu());
        if (restartButton) restartButton.onClick.AddListener(() => GameManager.Instance.StartNewGame());

        for (int i = 0; i < levelButtons.Count; i++)
        {
            int levelIndex = i;
            levelButtons[i].onClick.AddListener(() => GameManager.Instance.OnLevelButtonClicked(levelIndex));
        }
    }

    void OnEnable()
    {
        GameEvents.OnNextBlockReady += UpdateNextBlockPreview;
        GameEvents.OnScoreChanged += UpdateScoreText;
        GameEvents.OnPoolCountChanged += UpdatePoolCountText;
    }

    void OnDisable()
    {
        GameEvents.OnNextBlockReady -= UpdateNextBlockPreview;
        GameEvents.OnScoreChanged -= UpdateScoreText;
        GameEvents.OnPoolCountChanged -= UpdatePoolCountText;
    }

    private void UpdateScoreText(int newScore) => scoreText.text = $"{newScore}";
    private void UpdatePoolCountText(int count) => poolCountText.text = $"牌库剩余: {count}";
    #endregion

    // --- 【重大修正】---
    // 更新列表的逻辑被完全重写，采用更稳定可靠的字典统计法
    public void UpdateTetrominoList(IEnumerable<GameObject> prefabs, float totalMultiplier)
    {
        foreach (Transform child in tetrominoListContent) Destroy(child.gameObject);

        if (tetrominoListItemPrefab == null)
        {
            Debug.LogError("错误：TetrominoListItemPrefab 未在 GameUIController 中赋值！");
            return;
        }

        // 1. 创建一个字典来统计每种 Prefab 的数量
        var prefabCounts = new Dictionary<GameObject, int>();
        foreach (var prefab in prefabs)
        {
            if (!prefabCounts.ContainsKey(prefab))
            {
                prefabCounts[prefab] = 0;
            }
            prefabCounts[prefab]++;
        }

        // 2. 遍历统计完成的字典，为每种【不同】的 Tetromino 创建一个列表项
        foreach (var entry in prefabCounts)
        {
            var representativePrefab = entry.Key;
            int count = entry.Value;

            var tetromino = representativePrefab.GetComponent < Tetromino > ();
            if (tetromino == null || tetromino.uiPrefab == null) continue;

            // 3. 实例化列表项UI
            var itemGO = Instantiate(tetrominoListItemPrefab, tetrominoListContent);
            var listItemUI = itemGO.GetComponent<TetrominoListItemUI>();
            if (listItemUI == null)
            {
                Debug.LogError("列表项预制件上缺少 TetrominoListItemUI 脚本！", itemGO);
                continue;
            }
            if (listItemUI.multiplierText == null || listItemUI.shapeContainer == null)
            {
                Debug.LogError("列表项预制件的 TetrominoListItemUI 脚本中，有字段未赋值！", itemGO);
                continue;
            }

            // 4. 设置倍率和形状
            listItemUI.multiplierText.text = $"x{tetromino.extraMultiplier:F1}";
            Instantiate(tetromino.uiPrefab, listItemUI.shapeContainer);

            // 5. 处理堆叠数量显示
            if (listItemUI.countText != null)
            {
                if (count > 1)
                {
                    listItemUI.countText.gameObject.SetActive(true);
                    listItemUI.countText.text = $"x{count}";
                }
                else
                {
                    listItemUI.countText.gameObject.SetActive(false);
                }
            }
        }

        totalMultiplierText.text = $"x{totalMultiplier:F1}";
    }

    // (其余方法与上一版完全相同)
    #region Unchanged Code
    public void DisplayChosenTetrominoAndLockButtons(GameObject chosenPrefab)
    {
        foreach (var btn in levelButtons) btn.interactable = false;
        foreach (Transform child in chosenTetrominoArea) Destroy(child.gameObject);

        var tetromino = chosenPrefab.GetComponent<Tetromino>();
        if (tetromino != null && tetromino.uiPrefab != null)
        {
            var itemGO = Instantiate(tetrominoListItemPrefab, chosenTetrominoArea);
            var listItemUI = itemGO.GetComponent<TetrominoListItemUI>();

            if (listItemUI != null)
            {
                if (listItemUI.multiplierText)
                    listItemUI.multiplierText.text = $"x{tetromino.extraMultiplier:F1}";

                if (listItemUI.shapeContainer)
                    Instantiate(tetromino.uiPrefab, listItemUI.shapeContainer);

                if (listItemUI.countText != null)
                    listItemUI.countText.gameObject.SetActive(false);
            }
        }
    }

    public void ShowHuPopup(List<List<int>> huHand, HandAnalysisResult analysis, int baseScore, float multiplier, long finalScore)
    {
        huPopupPanel.SetActive(true);
        foreach (var btn in levelButtons) btn.interactable = true;
        foreach (Transform child in chosenTetrominoArea) Destroy(child.gameObject);

        patternNameText.text = $"{analysis.PatternName} ({analysis.TotalFan}番)";
        formulaText.text = $"{baseScore} × (2^{analysis.TotalFan}) × {multiplier:F1} = {finalScore}";

        BuildUIHand(huHandDisplayArea, huHand);
    }

    private void BuildUIHand(Transform container, List<List<int>> hand)
    {
        foreach (Transform child in container) Destroy(child.gameObject);

        var layoutGroup = container.GetComponent<LayoutGroup>();
        if (layoutGroup == null) Debug.LogWarning("HuHandDisplayArea 最好挂载一个 HorizontalLayoutGroup 以自动布局。");

        foreach (var set in hand)
        {
            foreach (var blockId in set)
            {
                var uiBlock = Instantiate(uiBlockPrefab, container);
                uiBlock.GetComponent<Image>().sprite = blockPool.GetSpriteForBlock(blockId);
            }

            if (layoutGroup == null && set != hand.Last())
            {
                var spacer = new GameObject("Spacer", typeof(RectTransform));
                spacer.transform.SetParent(container, false);
                spacer.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 20f);
            }
        }
    }

    public void HideHuPopup() => huPopupPanel.SetActive(false);
    public void ShowGameOverPanel() => gameOverPanel.SetActive(true);
    public void HideGameOverPanel() => gameOverPanel.SetActive(false);

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
    #endregion
}