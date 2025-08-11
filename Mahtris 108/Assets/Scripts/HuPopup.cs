using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HuPopup : MonoBehaviour
{
    public static HuPopup Instance { get; private set; }

    [Header("UI ���ã�Inspector ���")]
    public GameObject panel;
    public Transform contentParent;
    public GameObject tilePrefab;    // UI Image prefab �� �� BlockUnit �� prefab
    public Button confirmButton;

    private List<List<int>> currentSets = new List<List<int>>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (panel != null) panel.SetActive(false);
    }

    // ��ʾ���Ƶ�����sets ÿ��Ϊ���� blockId��
    public void ShowHu(List<List<int>> huSets)
    {
        if (panel == null || contentParent == null || tilePrefab == null || confirmButton == null)
        {
            Debug.LogWarning("[HuPopup] UI ����δ�������� (panel/contentParent/tilePrefab/confirmButton)��ֱ�� finalize �Ա��⿨����");
            TetrisGrid.PerformHuFinalize();
            return;
        }

        // ���� sets
        currentSets = new List<List<int>>();
        if (huSets != null)
        {
            foreach (var s in huSets) currentSets.Add(new List<int>(s));
        }

        // ��վ���ʾ
        foreach (Transform c in contentParent) Destroy(c.gameObject);

        // ����׼�� sprite Դ��BlockPool �� Resources ��ѡ��
        bool hasPool = BlockPool.Instance != null;
        Sprite[] fallback = null;
        if (!hasPool) fallback = Resources.LoadAll<Sprite>("MahjongTiles");

        // Ϊÿ�鴴����������ʾ������
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

                // ���ȴ���� BlockUnit �� prefab
                var bu = tile.GetComponent<BlockUnit>();
                if (bu != null)
                {
                    bu.SetAsDisplay(id);
                    tile.transform.localPosition = new Vector3(i * 1.2f, 0f, 0f);
                    continue;
                }

                // UI Image ����
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

        // ��ʾ����ͣ
        panel.SetActive(true);
        Time.timeScale = 0f;
        TetrisGrid.huPending = true;

        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(OnConfirmClicked);

        Debug.Log("[HuPopup] ��ʾ���Ƶ��� setsCount=" + currentSets.Count);
    }

    private void OnConfirmClicked()
    {
        confirmButton.onClick.RemoveAllListeners();

        Time.timeScale = 1f;

        TetrisGrid.PerformHuFinalize();

        panel.SetActive(false);

        currentSets.Clear();
        Debug.Log("[HuPopup] ���ȷ�ϣ��ѵ��� PerformHuFinalize");
    }
}
