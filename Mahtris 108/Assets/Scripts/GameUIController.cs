using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameUIController : MonoBehaviour
{
    [Header("UI元素引用")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text poolCountText;
    [SerializeField] private Transform nextBlockPreviewArea;
    [SerializeField] private GameObject huPopupPanel;
    [SerializeField] private Transform huHandDisplayArea; // 【关键】用于显示胡牌牌型的容器
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private GameObject blockDisplayPrefab; // 【关键】用于显示单个牌面的预制件
    [SerializeField] private BlockPool blockPool;

    private GameObject currentPreviewObject;

    void Awake()
    {
        if (continueButton) continueButton.onClick.AddListener(() => GameManager.Instance.ContinueAfterHu());
        if (restartButton) restartButton.onClick.AddListener(() => GameManager.Instance.StartNewGame());
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

    private void UpdateScoreText(int newScore)
    {
        if (scoreText) scoreText.text = $"得分: {newScore}";
    }

    private void UpdatePoolCountText(int count)
    {
        if (poolCountText) poolCountText.text = $"牌库剩余: {count}";
    }

    private void UpdateNextBlockPreview(GameObject prefab, List<int> ids)
    {
        if (currentPreviewObject != null) Destroy(currentPreviewObject);
        if (nextBlockPreviewArea == null) return;

        currentPreviewObject = Instantiate(prefab, nextBlockPreviewArea);
        currentPreviewObject.transform.localPosition = Vector3.zero;

        var tetromino = currentPreviewObject.GetComponent<Tetromino>();
        if (tetromino) tetromino.enabled = false;

        var blockUnits = currentPreviewObject.GetComponentsInChildren<BlockUnit>();
        for (int i = 0; i < blockUnits.Length && i < ids.Count; i++)
        {
            blockUnits[i].Initialize(ids[i], blockPool);
        }
    }

    // ---【重大修正点】---
    // 以下是重写后的 ShowHuPopup 方法
    public void ShowHuPopup(List<List<int>> huHand)
    {
        if (huPopupPanel) huPopupPanel.SetActive(true);
        if (huHandDisplayArea == null)
        {
            Debug.LogError("HuPopup 的 huHandDisplayArea 引用未设置!");
            return;
        }

        // 1. 清空上一次的牌型显示
        foreach (Transform child in huHandDisplayArea)
        {
            Destroy(child.gameObject);
        }

        float tileSpacing = 1.1f;   // 牌与牌之间的间距
        float groupSpacing = 0.5f;  // 组与组之间的额外间距
        float currentX = 0f;        // 当前牌应该摆放的X坐标

        // 2. 遍历手牌中的每一组牌（包括刻子、顺子、杠子和将牌）
        foreach (var set in huHand)
        {
            // 3. 遍历一组牌中的每一张牌
            foreach (var blockId in set)
            {
                // 4. 实例化用于显示的牌物体
                GameObject tileGO = Instantiate(blockDisplayPrefab, huHandDisplayArea);
                tileGO.transform.localPosition = new Vector3(currentX, 0, 0);

                // 5. 初始化牌面贴图
                var bu = tileGO.GetComponent<BlockUnit>();
                if (bu != null)
                {
                    bu.Initialize(blockId, blockPool);
                }

                // 6. 更新下一个牌的X坐标
                currentX += tileSpacing;
            }
            // 7. 每一组牌显示完后，增加额外的间距
            currentX += groupSpacing;
        }
    }

    public void HideHuPopup()
    {
        if (huPopupPanel) huPopupPanel.SetActive(false);
    }

    public void ShowGameOverPanel()
    {
        if (gameOverPanel) gameOverPanel.SetActive(true);
    }

    public void HideGameOverPanel()
    {
        if (gameOverPanel) gameOverPanel.SetActive(false);
    }
}
