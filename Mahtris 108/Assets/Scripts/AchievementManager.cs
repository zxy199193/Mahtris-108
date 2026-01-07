using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    [Header("资源配置")]
    public List<AchievementData> allAchievements; // 请在 Inspector 中把所有成就数据拖进去

    [Header("UI引用")]
    [SerializeField] private GameObject notificationPrefab;

    // 运行时缓存已解锁的ID
    private HashSet<string> unlockedAchievementIds = new HashSet<string>();
    private Queue<AchievementData> notificationQueue = new Queue<AchievementData>();
    private bool isShowingNotification = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadProgress();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ========================================================================
    // 1. 外部调用接口 (埋点用)
    // ========================================================================

    // 场景 B: 游戏胜利/结束时调用
    // difficulty: 0=Easy, 1=Normal, 2=Hard
    public void CheckGameWin(bool isWin, int difficulty, int finalSpeed, float remainTime, int remainGold, long score,
                                 int itemsUsed, int protocolsObtained, int finalBlockCount, bool isEndlessMode = false)
    {
        // 1. 检查最高分 (无论输赢都可以查)
        foreach (var ach in allAchievements)
        {
            if (ach.type == AchievementType.HighScore && score >= ach.targetValue && !IsUnlocked(ach)) UnlockAchievement(ach);
        }

        // 【核心修改】逻辑变更
        // 原逻辑: if (!isWin) return;
        // 新逻辑: 如果既没赢，也不是无尽模式结算，才直接返回。
        // 意思是：如果是无尽模式失败(isEndlessMode=true)，我们也继续往下检查属性成就。
        if (!isWin && !isEndlessMode) return;

        foreach (var ach in allAchievements)
        {
            if (IsUnlocked(ach)) continue;
            bool passed = false;
            switch (ach.type)
            {
                case AchievementType.WinGame:
                    // “通关游戏”成就比较特殊，通常只在真正的胜利时刻结算
                    // 如果你在无尽模式死了，虽然算“完成一局”，但不能算“再次通关”
                    // 所以这里我们保留 && isWin 的限制
                    if (isWin && difficulty == ach.targetValue) passed = true;
                    break;

                // 下面这些属性类的成就，在无尽模式失败时也可以触发
                case AchievementType.GameEndSpeed: if (finalSpeed >= ach.targetValue) passed = true; break;
                case AchievementType.GameEndTime: if (remainTime >= ach.targetValue) passed = true; break;
                case AchievementType.GameEndGold: if (remainGold >= ach.targetValue) passed = true; break; // 修复：增加金币成就判断

                case AchievementType.SingleGameItemUse: if (itemsUsed >= ach.targetValue) passed = true; break;
                case AchievementType.SingleGameTotalProtocol: if (protocolsObtained >= ach.targetValue) passed = true; break;
                case AchievementType.SingleGameScore: if (score >= ach.targetValue) passed = true; break;

                // 特殊挑战类
                case AchievementType.WinNoItem: if (itemsUsed == 0) passed = true; break;
                case AchievementType.WinNoProtocol: if (protocolsObtained == 0) passed = true; break;
                case AchievementType.WinMinBlocks: if (finalBlockCount <= ach.targetValue) passed = true; break;
            }
            if (passed) UnlockAchievement(ach);
        }
    }

    // 场景 C: 通用累积 (消除行、使用道具、获得金币)
    // type: 累积类型
    // amount: 增加的量
    public void AddProgress(AchievementType type, int amount)
    {
        string keyPrefix = $"Progress_{type}_";
        int current = PlayerPrefs.GetInt(keyPrefix, 0);
        current += amount;
        PlayerPrefs.SetInt(keyPrefix, current);
        PlayerPrefs.Save();

        // 检查是否达成
        foreach (var ach in allAchievements)
        {
            if (ach.type == type && !IsUnlocked(ach))
            {
                if (current >= ach.targetValue) UnlockAchievement(ach);
            }
        }
    }

    // ========================================================================
    // 2. 内部逻辑
    // ========================================================================

    public bool IsUnlocked(AchievementData data)
    {
        return unlockedAchievementIds.Contains(data.id);
    }

    private void UnlockAchievement(AchievementData data)
    {
        if (IsUnlocked(data)) return;

        // 1. 逻辑处理 (存档 & 发奖) - 立即生效
        unlockedAchievementIds.Add(data.id);
        PlayerPrefs.SetInt($"Ach_Unlocked_{data.id}", 1);
        PlayerPrefs.Save();
        if (GameSession.Instance)
        {
            GameSession.Instance.AddGold(data.rewardGold);
            AddProgress(AchievementType.AccumulateGold, data.rewardGold);
        }

        Debug.Log($"成就达成: {data.title}");

        // 2. UI 表现 - 加入队列，而非直接显示
        notificationQueue.Enqueue(data);

        // 3. 尝试处理队列
        ProcessNotificationQueue();
    }

    private void LoadProgress()
    {
        unlockedAchievementIds.Clear();
        if (allAchievements == null) return;

        foreach (var ach in allAchievements)
        {
            if (PlayerPrefs.GetInt($"Ach_Unlocked_{ach.id}", 0) == 1)
            {
                unlockedAchievementIds.Add(ach.id);
            }
        }
    }

    // 获取当前进度数值 (给 UI 显示用)
    public int GetCurrentProgress(AchievementType type)
    {
        return PlayerPrefs.GetInt($"Progress_{type}_", 0);
    }
    private void ProcessNotificationQueue()
    {
        // 如果当前正在显示某个飘窗，或者队列空了，就什么都不做
        if (isShowingNotification || notificationQueue.Count == 0) return;

        // 取出下一个成就数据
        AchievementData nextData = notificationQueue.Dequeue();

        // 标记为忙碌状态
        isShowingNotification = true;

        // 执行生成逻辑
        ShowNotificationInstance(nextData);
    }
    private void ShowNotificationInstance(AchievementData data)
    {
        if (notificationPrefab != null)
        {
            // 查找 Canvas (复用之前的智能查找逻辑)
            Transform parentTransform = null;
            GameObject mainCanvasObj = GameObject.Find("Canvas");
            if (mainCanvasObj != null)
            {
                parentTransform = mainCanvasObj.transform;
            }
            else
            {
                var gameUI = FindObjectOfType<GameUIController>();
                if (gameUI != null)
                {
                    Canvas uiCanvas = gameUI.GetComponentInParent<Canvas>();
                    if (uiCanvas != null) parentTransform = uiCanvas.transform;
                }
            }

            if (parentTransform == null)
            {
                Canvas anyCanvas = FindObjectOfType<Canvas>();
                if (anyCanvas != null) parentTransform = anyCanvas.transform;
            }

            if (parentTransform != null)
            {
                GameObject notifyObj = Instantiate(notificationPrefab, parentTransform);

                // 重置 Scale (防止父节点 Scale 影响)
                // 建议加上这个，防止有时候 Canvas Scale 不同导致飘窗变形
                notifyObj.transform.localScale = Vector3.one;

                AchievementNotificationUI ui = notifyObj.GetComponent<AchievementNotificationUI>();
                if (ui != null)
                {
                    // 【关键】调用 Show，并传入回调函数
                    ui.Show(data, () =>
                    {
                        // 回调逻辑：
                        // 1. 标记当前空闲
                        isShowingNotification = false;

                        // 2. 递归调用，检查队列里还有没有下一个
                        ProcessNotificationQueue();
                    });
                }
                else
                {
                    // 如果脚本缺失，防止队列卡死，直接释放锁
                    isShowingNotification = false;
                    ProcessNotificationQueue();
                }
            }
            else
            {
                // 如果找不到 Canvas (极少情况)，也得释放锁，否则后面都出不来了
                Debug.LogWarning("未找到 Canvas，跳过成就显示: " + data.title);
                isShowingNotification = false;
                ProcessNotificationQueue();
            }
        }
    }
    // 【新增】实时数值检查 (在 Update 或数值变化时调用)
    // baseScore: 基础分, blockMult: 方块倍率, extraMult: 额外倍率, blockCount: 场上方块数
    public void CheckRealtimeStats(int baseScore, float blockMult, float extraMult, int blockCount)
    {
        foreach (var ach in allAchievements)
        {
            if (IsUnlocked(ach)) continue;
            bool passed = false;
            switch (ach.type)
            {
                case AchievementType.StatBaseScore: if (baseScore >= ach.targetValue) passed = true; break;
                case AchievementType.StatBlockMult: if (blockMult >= ach.targetValue) passed = true; break;
                case AchievementType.StatExtraMult: if (extraMult >= ach.targetValue) passed = true; break;
                case AchievementType.MaxBlocksOnBoard: if (blockCount >= ach.targetValue) passed = true; break;
            }
            if (passed) UnlockAchievement(ach);
        }
    }
    // 【新增】胡牌时检查 (扩展)
    // loopCount: 当前圈数, activeProtocols: 当前激活条约数
    public void CheckOnHu(int fan, List<string> patterns, int loopCount, int activeProtocols)
    {
        // 1. 增加累计胡牌次数 (来自旧逻辑)
        AddProgress(AchievementType.AccumulateHu, 1);

        // 2. 遍历所有成就
        foreach (var ach in allAchievements)
        {
            if (IsUnlocked(ach)) continue;

            bool passed = false;
            switch (ach.type)
            {
                // --- 旧逻辑部分 (搬运过来的) ---
                case AchievementType.FirstHu:
                    passed = true; // 只要胡牌就触发
                    break;
                case AchievementType.HuFanCount:
                    if (fan >= ach.targetValue) passed = true;
                    break;
                case AchievementType.HuPattern:
                    if (patterns != null && patterns.Any(p => p.Contains(ach.targetString))) passed = true;
                    break;

                // --- 新逻辑部分 (圈数 & 条约) ---
                case AchievementType.SingleGameLoopCount:
                    if (loopCount >= ach.targetValue) passed = true;
                    break;
                case AchievementType.SingleGameActiveProtocol:
                    if (activeProtocols >= ach.targetValue) passed = true;
                    break;
            }

            if (passed) UnlockAchievement(ach);
        }
    }
    // 【新增】胜利结算检查 (大幅扩展)
    public void CheckGameWin(bool isWin, int difficulty, int finalSpeed, float remainTime, int remainGold, int score,
                             int itemsUsed, int protocolsObtained, int finalBlockCount)
    {
        // 1. 检查最高分 (无论输赢都可以查)
        foreach (var ach in allAchievements)
        {
            if (ach.type == AchievementType.HighScore && score >= ach.targetValue && !IsUnlocked(ach)) UnlockAchievement(ach);
        }

        if (!isWin) return;

        foreach (var ach in allAchievements)
        {
            if (IsUnlocked(ach)) continue;
            bool passed = false;
            switch (ach.type)
            {
                // ... (原有的 WinGame, Speed, Time 逻辑)
                case AchievementType.WinGame: if (difficulty == ach.targetValue) passed = true; break;
                case AchievementType.GameEndSpeed: if (finalSpeed >= ach.targetValue) passed = true; break;
                case AchievementType.GameEndTime: if (remainTime >= ach.targetValue) passed = true; break;

                // 新增逻辑
                case AchievementType.SingleGameItemUse: if (itemsUsed >= ach.targetValue) passed = true; break;
                case AchievementType.SingleGameTotalProtocol: if (protocolsObtained >= ach.targetValue) passed = true; break;
                case AchievementType.WinNoItem: if (itemsUsed == 0) passed = true; break;
                case AchievementType.WinNoProtocol: if (protocolsObtained == 0) passed = true; break;
                case AchievementType.WinMinBlocks: if (finalBlockCount <= ach.targetValue) passed = true; break;
            }
            if (passed) UnlockAchievement(ach);
        }
    }
    // 【新增】通用解锁检查 (在 Start 或 SaveManager 加载时调用)
    // 传入：解锁的道具数, 解锁的条约数
    public void CheckUnlocks(int unlockedItemCount, int unlockedProtocolCount)
    {
        foreach (var ach in allAchievements)
        {
            if (IsUnlocked(ach)) continue;
            if (ach.type == AchievementType.UnlockItemCount && unlockedItemCount >= ach.targetValue) UnlockAchievement(ach);
            if (ach.type == AchievementType.UnlockProtocolCount && unlockedProtocolCount >= ach.targetValue) UnlockAchievement(ach);
        }
    }
    public void CheckAllUnlockProgress(GameSettings settings)
    {
        if (settings == null) return;

        // 1. 统计已解锁的道具数量 (普通 + 高级)
        int unlockedItems = 0;
        foreach (var item in settings.commonItemPool)
        {
            if (SaveManager.IsItemUnlocked(item.itemName, item.isInitial)) unlockedItems++;
        }
        foreach (var item in settings.advancedItemPool)
        {
            if (SaveManager.IsItemUnlocked(item.itemName, item.isInitial)) unlockedItems++;
        }

        // 2. 统计已解锁的条约数量
        int unlockedProtocols = 0;
        foreach (var proto in settings.protocolPool)
        {
            if (SaveManager.IsProtocolUnlocked(proto.protocolName, proto.isInitial)) unlockedProtocols++;
        }

        // 3. 调用现有的检查逻辑
        CheckUnlocks(unlockedItems, unlockedProtocols);

        // Debug.Log($"[成就检查] 已解锁道具: {unlockedItems}, 已解锁条约: {unlockedProtocols}");
    }
    public void CheckSingleGameRealtimeStats(int itemsUsed, int protocolsObtained)
    {
        foreach (var ach in allAchievements)
        {
            if (IsUnlocked(ach)) continue;

            bool passed = false;
            switch (ach.type)
            {
                case AchievementType.SingleGameItemUse:
                    if (itemsUsed >= ach.targetValue) passed = true;
                    break;
                case AchievementType.SingleGameTotalProtocol:
                    if (protocolsObtained >= ach.targetValue) passed = true;
                    break;
            }

            if (passed) UnlockAchievement(ach);
        }
    }
}