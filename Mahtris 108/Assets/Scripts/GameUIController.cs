using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameUIController : MonoBehaviour
{
    [Header("UIԪ������")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text poolCountText;
    [SerializeField] private Transform nextBlockPreviewArea;
    [SerializeField] private GameObject huPopupPanel;
    [SerializeField] private Transform huHandDisplayArea; // ���ؼ���������ʾ�������͵�����
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private GameObject blockDisplayPrefab; // ���ؼ���������ʾ���������Ԥ�Ƽ�
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
        if (scoreText) scoreText.text = $"�÷�: {newScore}";
    }

    private void UpdatePoolCountText(int count)
    {
        if (poolCountText) poolCountText.text = $"�ƿ�ʣ��: {count}";
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

    // ---���ش������㡿---
    // ��������д��� ShowHuPopup ����
    public void ShowHuPopup(List<List<int>> huHand)
    {
        if (huPopupPanel) huPopupPanel.SetActive(true);
        if (huHandDisplayArea == null)
        {
            Debug.LogError("HuPopup �� huHandDisplayArea ����δ����!");
            return;
        }

        // 1. �����һ�ε�������ʾ
        foreach (Transform child in huHandDisplayArea)
        {
            Destroy(child.gameObject);
        }

        float tileSpacing = 1.1f;   // ������֮��ļ��
        float groupSpacing = 0.5f;  // ������֮��Ķ�����
        float currentX = 0f;        // ��ǰ��Ӧ�ðڷŵ�X����

        // 2. ���������е�ÿһ���ƣ��������ӡ�˳�ӡ����Ӻͽ��ƣ�
        foreach (var set in huHand)
        {
            // 3. ����һ�����е�ÿһ����
            foreach (var blockId in set)
            {
                // 4. ʵ����������ʾ��������
                GameObject tileGO = Instantiate(blockDisplayPrefab, huHandDisplayArea);
                tileGO.transform.localPosition = new Vector3(currentX, 0, 0);

                // 5. ��ʼ��������ͼ
                var bu = tileGO.GetComponent<BlockUnit>();
                if (bu != null)
                {
                    bu.Initialize(blockId, blockPool);
                }

                // 6. ������һ���Ƶ�X����
                currentX += tileSpacing;
            }
            // 7. ÿһ������ʾ������Ӷ���ļ��
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
