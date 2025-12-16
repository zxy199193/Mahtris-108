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
    [SerializeField] private float columnSpacing = 4.0f;
    [SerializeField] private float tileSpacing = 1.1f;
    [SerializeField] private float kongOffsetY = 0.3f;

    private List<List<int>> huPaiSets = new List<List<int>>();

    public void AddSets(List<List<int>> sets)
    {
        huPaiSets.AddRange(sets);
        RefreshDisplay();
        if (GameManager.Instance != null)
        {
            foreach (var set in sets)
            {
                foreach (var id in set)
                {
                    GameManager.Instance.OnHuPaiTileAdded(id);
                }
            }
        }
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

        for (int i = 0; i < huPaiSets.Count; i++)
        {
            var set = huPaiSets[i];

            // 计算当前面子在哪一列 (0 or 1) 和哪一行 (0, 1, 2...)
            int columnIndex = i % 2;
            int rowIndex = i / 2;

            // 根据行列计算出这一组牌的起始位置
            float startX = columnIndex * columnSpacing;
            float yPos = -rowIndex * rowSpacing;

            // 排列组内的每一张牌
            for (int tileIndex = 0; tileIndex < set.Count; tileIndex++)
            {
                int blockId = set[tileIndex];

                // 【核心修改】位置计算逻辑
                float xPos;
                float currentYPos = yPos;

                // 判断是否为杠牌的第4张 (索引为3)
                if (set.Count == 4 && tileIndex == 3)
                {
                    // === 放在第2张牌 (索引1) 的上方 ===
                    // X轴：与索引1的位置相同
                    xPos = startX + (1 * tileSpacing);

                    // Y轴：在当前行高基础上，向上偏移
                    currentYPos += kongOffsetY;
                }
                else
                {
                    // === 正常排列 (索引0, 1, 2) ===
                    xPos = startX + (tileIndex * tileSpacing);
                }

                // 实例化并设置位置
                GameObject go = Instantiate(blockPrefab, displayParent);
                RectTransform rectTransform = go.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = new Vector2(xPos, currentYPos);
                }

                var bu = go.GetComponent<BlockUnit>();
                if (bu != null)
                {
                    bu.Initialize(blockId, blockPool);
                }
            }
        }
    }

    public bool UpgradePungToKong(int pungTileValue, int fourthTileId)
    {
        // 查找匹配的刻子
        var pungSet = huPaiSets.FirstOrDefault(set => set.Count == 3 && (set[0] % 27) == (pungTileValue % 27));

        if (pungSet != null)
        {
            pungSet.Add(fourthTileId);
            RefreshDisplay();
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnHuPaiTileAdded(fourthTileId);
            }
            return true;
        }
        return false;
    }
}