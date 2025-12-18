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

    // 场景 A: 每次胡牌时调用
    public void CheckOnHu(int fan, List<string> patterns)
    {
        // 1. 增加累计胡牌次数
        AddProgress(AchievementType.AccumulateHu, 1);

        // 2. 检查单次胡牌相关成就
        foreach (var ach in allAchievements)
        {
            if (IsUnlocked(ach)) continue;

            bool passed = false;
            switch (ach.type)
            {
                case AchievementType.FirstHu:
                    passed = true;
                    break;
                case AchievementType.HuFanCount:
                    if (fan >= ach.targetValue) passed = true;
                    break;
                case AchievementType.HuPattern:
                    // 只要胡牌牌型里包含了目标字符串 (如 "清一色")
                    if (patterns != null && patterns.Any(p => p.Contains(ach.targetString))) passed = true;
                    break;
            }

            if (passed) UnlockAchievement(ach);
        }
    }

    // 场景 B: 游戏胜利/结束时调用
    // difficulty: 0=Easy, 1=Normal, 2=Hard
    public void CheckGameWin(bool isWin, int difficulty, int finalSpeed, float remainTime, int remainGold, int score)
    {
        if (!isWin) return; // 大部分成就要求"完成"(胜利)

        foreach (var ach in allAchievements)
        {
            if (IsUnlocked(ach)) continue;

            bool passed = false;
            switch (ach.type)
            {
                case AchievementType.WinGame:
                    // 检查难度
                    if (difficulty == ach.targetValue) passed = true;
                    break;
                case AchievementType.GameEndSpeed:
                    if (finalSpeed >= ach.targetValue) passed = true;
                    break;
                case AchievementType.GameEndTime:
                    if (remainTime >= ach.targetValue) passed = true;
                    break;
                case AchievementType.GameEndGold:
                    if (remainGold >= ach.targetValue) passed = true;
                    break;
                case AchievementType.SingleGameScore:
                    if (score >= ach.targetValue) passed = true;
                    break;
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
        if (GameSession.Instance) GameSession.Instance.AddGold(data.rewardGold);

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
        // ... (原有的 FirstHu, HuFanCount, HuPattern 逻辑保持不变)

        // 新增逻辑
        foreach (var ach in allAchievements)
        {
            if (IsUnlocked(ach)) continue;
            if (ach.type == AchievementType.SingleGameLoopCount && loopCount >= ach.targetValue) UnlockAchievement(ach);
            if (ach.type == AchievementType.SingleGameActiveProtocol && activeProtocols >= ach.targetValue) UnlockAchievement(ach);
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
}