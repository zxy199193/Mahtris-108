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
    private int _frameAddedCount = 0;
    private bool _isRefreshScheduled = false;

    public void AddSets(List<List<int>> sets, float delay = 0f)
    {
        if (sets == null || sets.Count == 0) return;

        // 1. 立刻占位计数与暂存
        _pendingSetCount += sets.Count;
        _pendingSetsData.AddRange(sets);

        // 核心执行逻辑
        System.Action executeLogic = () =>
        {
            if (this == null || gameObject == null) return;

            // 移除暂存
            foreach (var set in sets) _pendingSetsData.Remove(set);
            _pendingSetCount -= sets.Count;

            // 加入正式列表
            huPaiSets.AddRange(sets);

            // =========================================================
            // 【核心修复】合并刷新 (Batching)
            // 累加本次新增的组数，并预约在极短时间后统一刷新
            // =========================================================
            _frameAddedCount += sets.Count;

            if (!_isRefreshScheduled)
            {
                _isRefreshScheduled = true;

                // 延迟 0.02 秒执行统一刷新，把这一瞬间进来的所有牌一起动画
                DOVirtual.DelayedCall(0.02f, () =>
                {
                    if (this == null) return;

                    RefreshDisplay(_frameAddedCount, -1);

                    // 播放一次音效，避免多组牌同时加时声音过大或重叠
                    if (AudioManager.Instance != null && AudioManager.Instance.SoundLibrary != null)
                    {
                        AudioManager.Instance.PlaySFX(AudioManager.Instance.SoundLibrary.addSetToHuArea);
                    }

                    // 重置合并状态
                    _frameAddedCount = 0;
                    _isRefreshScheduled = false;

                }).SetUpdate(true); // 确保暂停时也能正常调度
            }

            // 触发游戏逻辑事件
            if (GameManager.Instance != null)
            {
                foreach (var set in sets)
                    foreach (var id in set)
                        GameManager.Instance.OnHuPaiTileAdded(id);
            }
        };

        if (delay > 0f) DOVirtual.DelayedCall(delay, () => executeLogic()).SetTarget(this);
        else executeLogic();
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

    private void RefreshDisplay(int newlyAddedSetCount = 0, int upgradedSetIndex = -1)
    {
        if (displayParent != null)
        {
            foreach (Transform child in displayParent) Destroy(child.gameObject);
        }
        else return;

        if (blockPrefab == null || blockPool == null) return;

        // 【关键】计算从哪一组开始是新加的。例如原来2组，新加3组，那么索引 >= 2 的全是新组
        int startAnimIndex = huPaiSets.Count - newlyAddedSetCount;

        for (int i = 0; i < huPaiSets.Count; i++)
        {
            var set = huPaiSets[i];

            int columnIndex = i % 2;
            int rowIndex = i / 2;

            float startX = columnIndex * columnSpacing;
            float yPos = -rowIndex * rowSpacing;

            // 只要索引大于等于起始点，就全是新组
            bool isNewSet = (i >= startAnimIndex);
            bool isUpgradedSet = (i == upgradedSetIndex);

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
                    // 判断是否需要动画
                    bool isGamePaused = Time.timeScale == 0f;
                    bool shouldAnimate = (isNewSet || (isUpgradedSet && tileIndex == 3)) && !isGamePaused;

                    if (shouldAnimate)
                    {
                        // 1. 设置出生点：空中
                        rectTransform.anchoredPosition = new Vector2(xPos, currentYPos + 50f);

                        // 2. 设置延迟：
                        // 如果是第4张牌（杠牌），稍微延迟 0.05s 让它跌在上面
                        // 其他所有牌（包括多组同时加进来的）延迟均为 0，完全同时下落
                        float delay = (tileIndex == 3) ? 0.05f : 0f;

                        rectTransform.DOAnchorPosY(currentYPos, 0.2f)
                                     .SetEase(Ease.OutBounce)
                                     .SetDelay(delay);
                    }
                    else
                    {
                        // 老牌直接定位，无动画
                        rectTransform.anchoredPosition = new Vector2(xPos, currentYPos);
                    }
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
            int setIndex = huPaiSets.IndexOf(pungSet);
            RefreshDisplay(0, setIndex);

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