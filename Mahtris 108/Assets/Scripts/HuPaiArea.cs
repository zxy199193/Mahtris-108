// FileName: HuPaiArea.cs
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class HuPaiArea : MonoBehaviour
{
    [Header("核心引用")]
    [SerializeField] private Transform displayParent;
    [SerializeField] private GameObject blockPrefab;
    [SerializeField] private BlockPool blockPool;

    [Header("布局设置")]
    [SerializeField] private float rowSpacing = 1.2f;
    [SerializeField] private float columnSpacing = 4.0f; // 新增：用于控制两列之间的间距
    [SerializeField] private float tileSpacing = 1.1f;

    private List<List<int>> huPaiSets = new List<List<int>>();

    public void AddSets(List<List<int>> sets)
    {
        huPaiSets.AddRange(sets);
        RefreshDisplay();
    }

    public bool RemoveLastSet()
    {
        if (huPaiSets.Count > 0)
        {
            var lastSet = huPaiSets.Last();
            blockPool.ReturnBlockIds(lastSet);
            huPaiSets.RemoveAt(huPaiSets.Count - 1);
            RefreshDisplay();
            return true;
        }
        return false;
    }

    public int GetSetCount() => huPaiSets.Count;

    public List<List<int>> GetAllSets() => new List<List<int>>(huPaiSets);

    public void ClearAll()
    {
        huPaiSets.Clear();
        if (displayParent != null)
        {
            foreach (Transform child in displayParent) Destroy(child.gameObject);
        }
    }

    private void RefreshDisplay()
    {
        if (displayParent != null)
        {
            foreach (Transform child in displayParent) Destroy(child.gameObject);
        }
        else
        {
            Debug.LogError("HuPaiArea 的 displayParent 引用未设置!");
            return;
        }

        if (blockPrefab == null || blockPool == null)
        {
            Debug.LogError("HuPaiArea 的 blockPrefab 或 blockPool 引用未设置!");
            return;
        }

        // 【修正点】重写了布局逻辑以实现双列网格排列
        for (int i = 0; i < huPaiSets.Count; i++)
        {
            var set = huPaiSets[i];

            // 计算当前面子在哪一列 (0 or 1) 和哪一行 (0, 1, 2...)
            int columnIndex = i % 2;
            int rowIndex = i / 2;

            // 根据行列计算出这一组牌的起始位置
            float startX = columnIndex * columnSpacing;
            float yPos = -rowIndex * rowSpacing;

            // 在起始位置的基础上，排列组内的每一张牌
            for (int tileIndex = 0; tileIndex < set.Count; tileIndex++)
            {
                int blockId = set[tileIndex];
                float xPos = startX + (tileIndex * tileSpacing);

                GameObject go = Instantiate(blockPrefab, displayParent);
                RectTransform rectTransform = go.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = new Vector2(xPos, yPos);
                }

                var bu = go.GetComponent<BlockUnit>();
                if (bu != null)
                {
                    bu.Initialize(blockId, blockPool);
                }
            }
        }
    }
    // 【HuPaiArea.cs 新增方法】
    public bool UpgradePungToKong(int pungTileValue, int fourthTileId)
    {
        // 查找匹配的刻子（拥有3张牌，且牌值相同）
        // 注意：这里使用 % 27 来匹配牌面数值，忽略ID差异
        var pungSet = huPaiSets.FirstOrDefault(set => set.Count == 3 && (set[0] % 27) == (pungTileValue % 27));

        if (pungSet != null)
        {
            pungSet.Add(fourthTileId); // 将第4张牌加入该组
            RefreshDisplay();          // 刷新UI显示
            return true;
        }
        return false;
    }
}
