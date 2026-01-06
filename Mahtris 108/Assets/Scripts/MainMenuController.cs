// FileName: MainMenuController.cs
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic; // 必须引用 List
using System.Linq; // 必须引用 Linq

public class MainMenuController : MonoBehaviour
{
    [Header("UI引用")]
    [SerializeField] private Text goldText;
    [SerializeField] private GameObject goldBarPanel;
    [SerializeField] private Text highScoreText;
    [SerializeField] private GameObject difficultyPopupPanel;
    [SerializeField] private RectTransform difficultyPopupWindow;
    [SerializeField] private Text currentDifficultyText;
    [SerializeField] private GameObject normalLockMask;
    [SerializeField] private GameObject hardLockMask;

    [Header("难度选择 - 按钮")]
    [SerializeField] private Button easyButton;
    [SerializeField] private Button normalButton;
    [SerializeField] private Button hardButton;

    [Header("难度选择 - 样式")]
    [SerializeField] private Color selectedColor = Color.green;
    [SerializeField] private Color unselectedColor = Color.white;

    [Header("商店")]
    [SerializeField] private Button openStoreButton;
    [SerializeField] private StorePanelController storePanel;

    // =========================================================
    // 【新增】商店提示配置
    // =========================================================
    [Header("商店提示 (可购买提醒)")]
    [SerializeField] private GameObject storeNotificationObj; // 请在 Inspector 中拖入提示用的红点或图标
    [SerializeField] private float notificationScaleDuration = 0.6f; // 缩放动画单次时间
    private Tween notificationTween; // 缓存动画，用于关闭时杀掉

    [Header("刷新功能解锁提示")]
    [SerializeField] private GameObject refreshUnlockTipRoot;       // 父节点 (黑色遮罩)
    [SerializeField] private RectTransform refreshUnlockTipContainer; // 弹窗本体
    [SerializeField] private Button refreshUnlockTipConfirmButton;  // 确认按钮
    private const string PREF_REFRESH_TIP_SHOWN = "RefreshTipShown"; // 存档 Key

    [Header("成就系统")]
    [SerializeField] private Button achievementButton;
    [SerializeField] private AchievementUIController achievementPopup;
    private string gameSceneName = "GameScene";

    public IntroPanelController introPanel;
    public Button openIntroButton;

    void Start()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMainMenuBGM();
        }
        UpdateGoldText();
        if (DifficultyManager.Instance != null)
        {
            InitDifficultyUI();
        }
        UpdateHighScoreText();
        bool savedFullscreenState = SaveManager.LoadFullscreenState();
        if (Screen.fullScreen != savedFullscreenState)
        {
            Screen.fullScreen = savedFullscreenState;
        }

        GameSession.OnGoldChanged += UpdateGoldText;
        InitDifficultyUI();
        UpdateDifficultyText(DifficultyManager.Instance.CurrentDifficulty);

        if (openStoreButton != null && storePanel != null)
        {
            openStoreButton.onClick.AddListener(() =>
            {
                HighlightTarget(goldBarPanel, true);
                storePanel.OpenStore();
            });

            storePanel.OnStoreClosed += () =>
            {
                HighlightTarget(goldBarPanel, false);
                // 【新增】商店关闭回来后，再次检查是否还要显示提示
                CheckStoreNotification();
                CheckAndShowRefreshUnlockTip();
            };
        }

        openIntroButton.onClick.AddListener(() =>
        {
            introPanel.Open();
        });

        if (achievementButton) achievementButton.onClick.AddListener(() => achievementPopup.ShowPopup());

        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged += OnLanguageChanged;
        }
        if (refreshUnlockTipConfirmButton != null)
        {
            refreshUnlockTipConfirmButton.onClick.AddListener(CloseRefreshUnlockTip);
        }
        // 【新增】初始化时检查一次提示
        CheckStoreNotification();
        CheckAndShowRefreshUnlockTip();
        if (AchievementManager.Instance != null && storePanel != null)
        {
            AchievementManager.Instance.CheckAllUnlockProgress(storePanel.GetSettings());
        }
    }

    void OnDestroy()
    {
        GameSession.OnGoldChanged -= UpdateGoldText;
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged -= OnLanguageChanged;
        }

        // 【新增】清理动画
        if (notificationTween != null) notificationTween.Kill();
    }

    private void UpdateGoldText(int newGoldAmount)
    {
        if (goldText != null)
        {
            goldText.text = $"{newGoldAmount}";
        }
        // 【新增】金币变化时检查提示 (防止玩家通关回来钱多了，可以买新东西了)
        CheckStoreNotification();
    }

    private void UpdateGoldText()
    {
        if (GameSession.Instance != null && goldText != null)
        {
            goldText.text = $"{GameSession.Instance.CurrentGold}";
        }
        else if (goldText != null)
        {
            goldText.text = "0";
        }
    }

    // =========================================================
    // 【新增】商店可购买提示逻辑
    // =========================================================
    private void CheckStoreNotification()
    {
        // 如果没有配置提示物体，或者没有引用商店/设置，直接返回
        if (storeNotificationObj == null || storePanel == null) return;

        GameSettings settings = storePanel.GetSettings();
        if (settings == null) return;

        bool canBuyAnything = false;
        int currentGold = GameSession.Instance != null ? GameSession.Instance.CurrentGold : 0;

        // -----------------------
        // 1. 检查道具 (Item)
        // -----------------------
        List<ItemData> allItems = new List<ItemData>();
        if (settings.commonItemPool != null) allItems.AddRange(settings.commonItemPool);
        if (settings.advancedItemPool != null) allItems.AddRange(settings.advancedItemPool);

        // 计算当前已解锁的道具数量 (用于判断 Legendary 是否显示)
        int unlockedItemsCount = allItems.Count(i => SaveManager.IsItemUnlocked(i.itemName, i.isInitial));

        foreach (var item in allItems)
        {
            bool isUnlocked = SaveManager.IsItemUnlocked(item.itemName, item.isInitial);

            // 只有 未解锁 且 不隐藏 的物品才参与检查
            // 隐藏条件：是传奇物品 且 解锁数量不足
            bool isHidden = item.isLegendary && unlockedItemsCount < item.unlockConditionCount;

            if (!isUnlocked && !isHidden)
            {
                // 如果买得起
                if (currentGold >= item.price)
                {
                    canBuyAnything = true;
                    break; // 只要有一个能买，就显示提示，跳出循环
                }
            }
        }

        // -----------------------
        // 2. 检查条约 (Protocol) (如果道具没检测到能买的，才检测条约)
        // -----------------------
        if (!canBuyAnything && settings.protocolPool != null)
        {
            int unlockedProtocolsCount = settings.protocolPool.Count(p => SaveManager.IsProtocolUnlocked(p.protocolName, p.isInitial));

            foreach (var proto in settings.protocolPool)
            {
                bool isUnlocked = SaveManager.IsProtocolUnlocked(proto.protocolName, proto.isInitial);
                bool isHidden = proto.isLegendary && unlockedProtocolsCount < proto.unlockConditionCount;

                if (!isUnlocked && !isHidden)
                {
                    if (currentGold >= proto.price)
                    {
                        canBuyAnything = true;
                        break;
                    }
                }
            }
        }

        // -----------------------
        // 3. 更新 UI 状态
        // -----------------------
        if (canBuyAnything)
        {
            // 如果物体之前是隐藏的，现在激活并开始动画
            if (!storeNotificationObj.activeSelf)
            {
                storeNotificationObj.SetActive(true);

                // 开始循环呼吸动画
                if (notificationTween != null) notificationTween.Kill();
                storeNotificationObj.transform.localScale = Vector3.one; // 重置大小

                notificationTween = storeNotificationObj.transform
                    .DOScale(1.1f, notificationScaleDuration) // 放大到 1.1 倍
                    .SetLoops(-1, LoopType.Yoyo) // 无限循环，悠悠球模式 (大->小->大)
                    .SetEase(Ease.InOutSine)     // 平滑曲线
                    .SetUpdate(true);            // 忽略 TimeScale
            }
        }
        else
        {
            // 如果不能购买，隐藏并停止动画
            if (storeNotificationObj.activeSelf)
            {
                storeNotificationObj.SetActive(false);
                if (notificationTween != null) notificationTween.Kill();
            }
        }
    }

    // ... [以下为原有代码，保持不变] ...

    private void UpdateHighScoreText()
    {
        if (highScoreText != null)
        {
            int highScore = SaveManager.LoadHighScore();
            highScoreText.text = $"{highScore}";
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
    private void InitDifficultyUI()
    {
        Difficulty savedDiff = DifficultyManager.Instance.CurrentDifficulty;
        UpdateDifficultyText(savedDiff);

        int unlockedLevel = DifficultyManager.Instance.MaxUnlockedLevel;

        if (normalLockMask) normalLockMask.SetActive(unlockedLevel < 1);
        if (hardLockMask) hardLockMask.SetActive(unlockedLevel < 2);

        if (normalButton) normalButton.interactable = (unlockedLevel >= 1);
        if (hardButton) hardButton.interactable = (unlockedLevel >= 2);

        UpdateButtonVisuals(savedDiff);
    }

    public void SelectEasy()
    {
        DifficultyManager.Instance.SetDifficulty(Difficulty.Easy);
        UpdateDifficultyText(Difficulty.Easy);
        UpdateButtonVisuals(Difficulty.Easy);
    }

    public void SelectNormal()
    {
        if (DifficultyManager.Instance.MaxUnlockedLevel >= 1)
        {
            DifficultyManager.Instance.SetDifficulty(Difficulty.Normal);
            UpdateDifficultyText(Difficulty.Normal);
            UpdateButtonVisuals(Difficulty.Normal);
        }
    }

    public void SelectHard()
    {
        if (DifficultyManager.Instance.MaxUnlockedLevel >= 2)
        {
            DifficultyManager.Instance.SetDifficulty(Difficulty.Hard);
            UpdateDifficultyText(Difficulty.Hard);
            UpdateButtonVisuals(Difficulty.Hard);
        }
    }

    public void OpenDifficultyPopup()
    {
        if (difficultyPopupPanel != null)
        {
            difficultyPopupPanel.SetActive(true);
            if (difficultyPopupWindow != null)
            {
                difficultyPopupWindow.anchoredPosition = new Vector2(0, -1200);
                difficultyPopupWindow.DOLocalMove(Vector2.zero, 0.4f).SetEase(Ease.OutBack).SetUpdate(true);
            }
        }
    }

    public void CloseDifficultyPopup()
    {
        if (difficultyPopupWindow != null)
        {
            difficultyPopupWindow.DOLocalMove(new Vector2(0, -1200), 0.4f).SetEase(Ease.InBack).SetUpdate(true)
                .OnComplete(() =>
                {
                    if (difficultyPopupPanel != null)
                    {
                        difficultyPopupPanel.SetActive(false);
                    }
                });
        }
        else
        {
            if (difficultyPopupPanel != null) difficultyPopupPanel.SetActive(false);
        }
    }
    private void UpdateDifficultyText(Difficulty difficulty)
    {
        if (currentDifficultyText != null)
        {
            string key = "";
            switch (difficulty)
            {
                case Difficulty.Easy:
                    key = "DIFFICULTY_EASY";
                    break;
                case Difficulty.Hard:
                    key = "DIFFICULTY_HARD";
                    break;
                case Difficulty.Normal:
                default:
                    key = "DIFFICULTY_NORMAL";
                    break;
            }
            if (LocalizationManager.Instance)
            {
                currentDifficultyText.text = LocalizationManager.Instance.GetText(key);
                LocalizationManager.Instance.UpdateFont(currentDifficultyText);
            }
        }

    }
    private void UpdateButtonVisuals(Difficulty selected)
    {
        UpdateButtonState(easyButton, selected == Difficulty.Easy);
        UpdateButtonState(normalButton, selected == Difficulty.Normal);
        UpdateButtonState(hardButton, selected == Difficulty.Hard);
    }

    private void UpdateButtonState(Button btn, bool isSelected)
    {
        if (btn != null)
        {
            Image img = btn.GetComponent<Image>();
            if (img != null)
            {
                img.color = isSelected ? selectedColor : unselectedColor;
            }

            Text btnText = btn.GetComponentInChildren<Text>();
            if (btnText != null)
            {
                LocalizedText locText = btnText.GetComponent<LocalizedText>();
                string targetKey = isSelected ? "DIFFICULTY_SELECTED" : "DIFFICULTY_SELECT";

                if (locText != null)
                {
                    locText.SetKey(targetKey);
                }
                else
                {
                    if (LocalizationManager.Instance)
                    {
                        btnText.text = LocalizationManager.Instance.GetText(targetKey);
                        LocalizationManager.Instance.UpdateFont(btnText);
                    }
                }
            }
        }
    }
    private void HighlightTarget(GameObject target, bool highlight)
    {
        if (target == null) return;

        if (highlight)
        {
            Canvas canvas = target.GetComponent<Canvas>();
            if (canvas == null) canvas = target.AddComponent<Canvas>();

            canvas.overrideSorting = true;
            canvas.sortingOrder = 3000;

            if (target.GetComponent<GraphicRaycaster>() == null) target.AddComponent<GraphicRaycaster>();
        }
        else
        {
            GraphicRaycaster gr = target.GetComponent<GraphicRaycaster>();
            if (gr != null) Destroy(gr);

            Canvas canvas = target.GetComponent<Canvas>();
            if (canvas != null) Destroy(canvas);
        }
    }
    private void OnLanguageChanged()
    {
        if (DifficultyManager.Instance != null)
        {
            UpdateButtonVisuals(DifficultyManager.Instance.CurrentDifficulty);
            UpdateDifficultyText(DifficultyManager.Instance.CurrentDifficulty);
        }
        StartCoroutine(ForceRefreshAllLayouts());
    }
    private IEnumerator ForceRefreshAllLayouts()
    {
        yield return null;
        if (difficultyPopupPanel != null)
        {
            LayoutGroup[] groups = difficultyPopupPanel.GetComponentsInChildren<LayoutGroup>(true);
            foreach (var group in groups)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(group.GetComponent<RectTransform>());
            }
        }
    }
    private void CheckAndShowRefreshUnlockTip()
    {
        // 1. 如果已经显示过（存了档），就不再显示
        if (PlayerPrefs.GetInt(PREF_REFRESH_TIP_SHOWN, 0) == 1) return;

        // 2. 检查是否达成条件 (所有道具和条约都已解锁)
        if (IsAllContentUnlocked())
        {
            ShowRefreshUnlockTip();
        }
    }
    private bool IsAllContentUnlocked()
    {
        if (storePanel == null) return false;
        GameSettings settings = storePanel.GetSettings(); // 直接从商店面板获取配置
        if (settings == null) return false;

        // 检查道具 (Common + Advanced)
        List<ItemData> allItems = new List<ItemData>();
        if (settings.commonItemPool != null) allItems.AddRange(settings.commonItemPool);
        if (settings.advancedItemPool != null) allItems.AddRange(settings.advancedItemPool);

        foreach (var item in allItems)
        {
            if (!SaveManager.IsItemUnlocked(item.itemName, item.isInitial)) return false;
        }

        // 检查条约
        if (settings.protocolPool != null)
        {
            foreach (var proto in settings.protocolPool)
            {
                if (!SaveManager.IsProtocolUnlocked(proto.protocolName, proto.isInitial)) return false;
            }
        }

        return true;
    }
    private void ShowRefreshUnlockTip()
    {
        if (refreshUnlockTipRoot == null || refreshUnlockTipContainer == null) return;

        // 1. 激活遮罩
        refreshUnlockTipRoot.SetActive(true);

        // 2. 准备动画：先移到屏幕下方
        refreshUnlockTipContainer.anchoredPosition = new Vector2(0, -1200);

        // 3. 播放滑入动画 (参考了 StorePanel 的动画参数)
        refreshUnlockTipContainer.DOKill();
        refreshUnlockTipContainer.DOLocalMove(Vector2.zero, 0.4f)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);
    }
    private void CloseRefreshUnlockTip()
    {
        if (refreshUnlockTipContainer == null) return;

        // 1. 记录已显示状态，以后不再显示
        PlayerPrefs.SetInt(PREF_REFRESH_TIP_SHOWN, 1);
        PlayerPrefs.Save();

        // 2. 播放滑出动画
        refreshUnlockTipContainer.DOKill();
        refreshUnlockTipContainer.DOLocalMove(new Vector2(0, -1200), 0.4f)
            .SetEase(Ease.InBack)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                if (refreshUnlockTipRoot) refreshUnlockTipRoot.SetActive(false);
            });
    }
}