// FileName: HuPaiArea.cs
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;

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

    public void AddSets(List<List<int>> sets, float delay = 0f)
    {
        if (sets == null || sets.Count == 0) return;

        // 定义核心执行逻辑 (闭包)
        System.Action executeLogic = () =>
        {
            // 安全检查：防止延迟期间场景切换或物体被销毁导致报错
            if (this == null || gameObject == null) return;

            // 1. 更新数据
            huPaiSets.AddRange(sets);

            // 2. 刷新画面 (此时才会显示牌)
            RefreshDisplay();

            // 3. 播放音效 (逻辑已经延迟了，所以这里立即播放即可)
            if (AudioManager.Instance != null && AudioManager.Instance.SoundLibrary != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.SoundLibrary.addSetToHuArea);
            }

            // 4. 通知 GameManager (应用被动效果，如加分)
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
        };

        // 根据 delay 参数决定执行时机
        if (delay > 0f)
        {
            // 使用 DOTween 进行延迟调用
            // 注意：这里不需要再对音效单独做 Delay 了，因为整个 executeLogic 都会被推迟
            DOVirtual.DelayedCall(delay, () => executeLogic());
        }
        else
        {
            // 立即执行 (用于道具直接添加等情况)
            executeLogic();
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