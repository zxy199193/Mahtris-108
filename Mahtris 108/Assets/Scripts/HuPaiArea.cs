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

    // 正式显示的牌组列表
    private List<List<int>> huPaiSets = new List<List<int>>();

    // 【新增】暂存数据列表：存储那些“正在路上”还未正式显示的牌组
    // 用于确保 GetAllSets() 能立即获取到完整数据，而不用等动画结束
    private List<List<int>> _pendingSetsData = new List<List<int>>();

    // 挂起计数
    private int _pendingSetCount = 0;

    public void AddSets(List<List<int>> sets, float delay = 0f)
    {
        if (sets == null || sets.Count == 0) return;

        // 1. 立刻占位计数
        _pendingSetCount += sets.Count;

        // 2. 【关键】立刻保存数据到暂存列表
        // 这样即使视觉上还没显示，数据逻辑（如胡牌检测）也能立刻读到它
        _pendingSetsData.AddRange(sets);

        // 定义核心执行逻辑
        System.Action executeLogic = () =>
        {
            if (this == null || gameObject == null) return;

            // 3. 延迟结束后：从暂存区移除，加入正式区
            // 注意：这里需要移除对应的引用
            foreach (var set in sets)
            {
                _pendingSetsData.Remove(set);
            }

            // 还原计数
            _pendingSetCount -= sets.Count;

            // 加入正式列表并刷新 UI
            huPaiSets.AddRange(sets);
            RefreshDisplay();

            // 播放音效
            if (AudioManager.Instance != null && AudioManager.Instance.SoundLibrary != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.SoundLibrary.addSetToHuArea);
            }

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

        if (delay > 0f)
        {
            DOVirtual.DelayedCall(delay, () => executeLogic()).SetTarget(this);
        }
        else
        {
            executeLogic();
        }
    }

    public bool RemoveLastSet()
    {
        // 移除逻辑通常只针对已显示的牌
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

    public int GetSetCount() => huPaiSets.Count + _pendingSetCount;

    // 【核心修复】获取所有牌组时，合并“正式列表”和“暂存列表”
    // 这样 GameManager 在胡牌弹窗读取数据时，即使牌还在飞，也能拿到完整的数据！
    public List<List<int>> GetAllSets()
    {
        var allSets = new List<List<int>>(huPaiSets);
        if (_pendingSetsData.Count > 0)
        {
            allSets.AddRange(_pendingSetsData);
        }
        return allSets;
    }

    public void ClearAll()
    {
        // 1. 杀掉延迟动画
        DOTween.Kill(this);

        // 2. 清理所有数据
        _pendingSetCount = 0;
        _pendingSetsData.Clear(); // 清空暂存区
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

            int columnIndex = i % 2;
            int rowIndex = i / 2;

            float startX = columnIndex * columnSpacing;
            float yPos = -rowIndex * rowSpacing;

            for (int tileIndex = 0; tileIndex < set.Count; tileIndex++)
            {
                int blockId = set[tileIndex];
                float xPos;
                float currentYPos = yPos;

                if (set.Count == 4 && tileIndex == 3)
                {
                    xPos = startX + (1 * tileSpacing);
                    currentYPos += kongOffsetY;
                }
                else
                {
                    xPos = startX + (tileIndex * tileSpacing);
                }

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
        // 只能升级已经存在的牌，Pending 中的牌还不能被升级
        var pungSet = huPaiSets.FirstOrDefault(set => set.Count == 3 && (set[0] % 27) == (pungTileValue % 27));

        if (pungSet != null)
        {
            pungSet.Add(fourthTileId);
            RefreshDisplay();

            if (AudioManager.Instance != null && AudioManager.Instance.SoundLibrary != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.SoundLibrary.addSetToHuArea);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnHuPaiTileAdded(fourthTileId);
            }
            return true;
        }
        return false;
    }
}