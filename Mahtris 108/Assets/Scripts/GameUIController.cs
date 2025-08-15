// FileName: GameUIController.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class GameUIController : MonoBehaviour
{
    // --- �����ֶ��������ֲ��� ---
    #region Fields
    [Header("ͨ��UIԪ��")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text poolCountText;

    [Header("��һ������Ԥ��")]
    [SerializeField] private Transform nextBlockPreviewArea;

    [Header("���Ƶ���")]
    [SerializeField] private GameObject huPopupPanel;
    [SerializeField] private Transform huHandDisplayArea;
    [SerializeField] private Text patternNameText;
    [SerializeField] private Text formulaText;
    [SerializeField] private Button continueButton;
    [SerializeField] private List<Button> levelButtons;
    [SerializeField] private Transform chosenTetrominoArea;

    [Header("Tetromino�б�")]
    [SerializeField] private Transform tetrominoListContent;
    [SerializeField] private Text totalMultiplierText;

    [Header("��Ϸ����")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button restartButton;

    [Header("UIԤ�Ƽ�")]
    [Tooltip("���ڶ�̬ƴ�Ӻ������͵ġ�����UI�齫�ơ�Ԥ�Ƽ�")]
    [SerializeField] private GameObject uiBlockPrefab;
    [Tooltip("�������б�����ʾ������Tetromino����UI��Ԥ�Ƽ� (Ӧ����TetrominoListItemUI�ű�)")]
    [SerializeField] private GameObject tetrominoListItemPrefab;

    [Header("ģ������")]
    [SerializeField] private BlockPool blockPool;

    private GameObject currentPreviewObject;
    #endregion

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

    // ---���ش�������---
    // UpdateTetrominoList ��Ψһ��Ҫ�޸ĵķ���
    public void UpdateTetrominoList(IEnumerable<GameObject> prefabs, float totalMultiplier)
    {
        foreach (Transform child in tetrominoListContent) Destroy(child.gameObject);

        if (tetrominoListItemPrefab == null)
        {
            Debug.LogError("����GameUIController��TetrominoListItemPrefab�ֶ�δ��Inspector�и�ֵ��");
            return;
        }

        foreach (var prefab in prefabs)
        {
            // 1. ʵ�����б����UIԤ�Ƽ�
            var itemGO = Instantiate(tetrominoListItemPrefab, tetrominoListContent);

            // 2. ��ȡ�����ĸ����ű����
            var listItemUI = itemGO.GetComponent<TetrominoListItemUI>();
            if (listItemUI == null)
            {
                Debug.LogError("���󣺷����TetrominoListItemPrefab��ȱ��TetrominoListItemUI�ű���");
                continue;
            }

            var tetromino = prefab.GetComponent<Tetromino>();
            if (tetromino != null && tetromino.uiPrefab != null)
            {
                // 3. ͨ�������ű���ֱ����������ȫ�ط��������������ʹ��transform.Find()��
                listItemUI.multiplierText.text = $"x{tetromino.extraMultiplier:F1}";
                Instantiate(tetromino.uiPrefab, listItemUI.shapeContainer);
            }
        }
        totalMultiplierText.text = $"�ܱ���: x{totalMultiplier:F1}";
    }

    // --- �������з������� version 2.6 ����һ�� ---
    #region Unchanged Methods
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

    private void UpdateScoreText(int newScore) => scoreText.text = $"�÷�: {newScore}";
    private void UpdatePoolCountText(int count) => poolCountText.text = $"�ƿ�ʣ��: {count}";

    public void ShowHuPopup(List<List<int>> huHand, HandAnalysisResult analysis, int baseScore, float multiplier, long finalScore)
    {
        huPopupPanel.SetActive(true);
        foreach (var btn in levelButtons) btn.interactable = true;
        foreach (Transform child in chosenTetrominoArea) Destroy(child.gameObject);

        patternNameText.text = $"{analysis.PatternName} ({analysis.TotalFan}��)";
        formulaText.text = $"{baseScore} �� (2^{analysis.TotalFan}) �� {multiplier:F1} = {finalScore}";

        BuildUIHand(huHandDisplayArea, huHand);
    }

    public void DisplayChosenTetrominoAndLockButtons(GameObject chosenPrefab)
    {
        foreach (var btn in levelButtons) btn.interactable = false;
        foreach (Transform child in chosenTetrominoArea) Destroy(child.gameObject);

        var tetromino = chosenPrefab.GetComponent<Tetromino>();
        if (tetromino != null && tetromino.uiPrefab != null)
        {
            Instantiate(tetromino.uiPrefab, chosenTetrominoArea);
        }
    }

    private void BuildUIHand(Transform container, List<List<int>> hand)
    {
        foreach (Transform child in container) Destroy(child.gameObject);

        var layoutGroup = container.GetComponent<LayoutGroup>();
        if (layoutGroup == null) Debug.LogWarning("HuHandDisplayArea ��ù���һ�� HorizontalLayoutGroup ���Զ����֡�");

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







