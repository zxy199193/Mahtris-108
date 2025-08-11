using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HuPopup : MonoBehaviour
{
    public static HuPopup Instance { get; private set; }

    [Header("UI 引用（Inspector 必填）")]
    public GameObject panel;
    public Transform contentParent;
    public GameObject tilePrefab;    // UI Image prefab 或 带 BlockUnit 的 prefab
    public Button confirmButton;

    private List<List<int>> currentSets = new List<List<int>>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (panel != null) panel.SetActive(false);
    }

    // 显示胡牌弹窗（sets 每项为三张 blockId）
    public void ShowHu(List<List<int>> huSets)
    {
        if (panel == null || contentParent == null || tilePrefab == null || confirmButton == null)
        {
            Debug.LogWarning("[HuPopup] UI 引用未完整设置 (panel/contentParent/tilePrefab/confirmButton)。直接 finalize 以避免卡死。");
            TetrisGrid.PerformHuFinalize();
            return;
        }

        // 缓存 sets
        currentSets = new List<List<int>>();
        if (huSets != null)
        {
            foreach (var s in huSets) currentSets.Add(new List<int>(s));
        }

        // 清空旧显示
        foreach (Transform c in contentParent) Destroy(c.gameObject);

        // 尝试准备 sprite 源（BlockPool 或 Resources 备选）
        bool hasPool = BlockPool.Instance != null;
        Sprite[] fallback = null;
        if (!hasPool) fallback = Resources.LoadAll<Sprite>("MahjongTiles");

        // 为每组创建容器并显示三张牌
        float xOffset = 0f;
        float setSpacing = 120f;
        foreach (var set in currentSets)
        {
            GameObject setContainer = new GameObject("SetContainer", typeof(RectTransform));
            setContainer.transform.SetParent(contentParent, false);
            RectTransform rt = setContainer.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(xOffset, 0);
            xOffset += setSpacing;

            for (int i = 0; i < set.Count; i++)
            {
                int id = set[i];
                GameObject tile = Instantiate(tilePrefab, setContainer.transform);

                // 优先处理带 BlockUnit 的 prefab
                var bu = tile.GetComponent<BlockUnit>();
                if (bu != null)
                {
                    bu.SetAsDisplay(id);
                    tile.transform.localPosition = new Vector3(i * 1.2f, 0f, 0f);
                    continue;
                }

                // UI Image 处理
                var img = tile.GetComponent<Image>();
                if (img != null)
                {
                    Sprite s = null;
                    if (hasPool) s = BlockPool.Instance.GetSpriteForBlock(id);
                    else if (fallback != null && fallback.Length > 0) s = fallback[id % fallback.Length];

                    if (s == null)
                    {
                        Debug.LogWarning($"[HuPopup] tile sprite null for id={id}");
                        img.enabled = false;
                    }
                    else
                    {
                        img.sprite = s;
                        img.enabled = true;
                    }

                    tile.GetComponent<RectTransform>().anchoredPosition = new Vector2(i * 90f, 0f);
                    continue;
                }

                tile.name = "Tile_" + id;
            }
        }

        // 显示并暂停
        panel.SetActive(true);
        Time.timeScale = 0f;
        TetrisGrid.huPending = true;

        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(OnConfirmClicked);

        Debug.Log("[HuPopup] 显示胡牌弹窗 setsCount=" + currentSets.Count);
    }

    private void OnConfirmClicked()
    {
        confirmButton.onClick.RemoveAllListeners();

        Time.timeScale = 1f;

        TetrisGrid.PerformHuFinalize();

        panel.SetActive(false);

        currentSets.Clear();
        Debug.Log("[HuPopup] 玩家确认，已调用 PerformHuFinalize");
    }
}
