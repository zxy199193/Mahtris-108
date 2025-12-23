// FileName: MainMenuController.cs
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    [Header("UI引用")]
    [SerializeField] private Text goldText;
    [SerializeField] private GameObject goldBarPanel;
    [SerializeField] private Text highScoreText; // 新增
    [SerializeField] private GameObject difficultyPopupPanel; // 【新增】拖入你的难度选择弹窗面板
    [SerializeField] private RectTransform difficultyPopupWindow;
    [SerializeField] private Text currentDifficultyText;
    // 【新增】遮罩引用 (请在 Inspector 中拖入)
    [SerializeField] private GameObject normalLockMask; // 挡住普通按钮的遮罩
    [SerializeField] private GameObject hardLockMask;   // 挡住困难按钮的遮罩

    // 【新增】为了方便控制按钮不可点击，建议也引用按钮本身 (可选)
    [Header("难度选择 - 按钮")]
    [SerializeField] private Button easyButton;
    [SerializeField] private Button normalButton;
    [SerializeField] private Button hardButton;

    [Header("难度选择 - 样式")]
    [SerializeField] private Color selectedColor = Color.green;   // 【新增】选中时的颜色 (例如绿色)
    [SerializeField] private Color unselectedColor = Color.white;

    [Header("商店")]
    [SerializeField] private Button openStoreButton;
    [SerializeField] private StorePanelController storePanel;

    [Header("成就系统")]
    [SerializeField] private Button achievementButton;           // 主界面上的成就按钮
    [SerializeField] private AchievementUIController achievementPopup; // 成就弹窗控制器
    private string gameSceneName = "GameScene";

    public IntroPanelController introPanel;
    public Button openIntroButton;

    void Start()
    {
        // 游戏开始时，尝试找到GameSession并更新金币显示
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
        // 订阅金币变化事件，以便实时更新（例如从商店返回时）
        GameSession.OnGoldChanged += UpdateGoldText;
        InitDifficultyUI();
        UpdateDifficultyText(DifficultyManager.Instance.CurrentDifficulty);
        if (openStoreButton != null && storePanel != null)
        {
            openStoreButton.onClick.AddListener(() =>
            {
                HighlightTarget(goldBarPanel, true);
                // 如果你有点击音效，可以在这里播，或者让 UIButtonClickEffect 处理
                storePanel.OpenStore();
            });
            storePanel.OnStoreClosed += () =>
            {
                // 商店关完后，把金币栏放回去
                HighlightTarget(goldBarPanel, false);
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
    }

    void OnDestroy()
    {
        // 场景销毁时取消订阅，防止内存泄漏
        GameSession.OnGoldChanged -= UpdateGoldText;
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged -= OnLanguageChanged;
        }
    }

    private void UpdateGoldText(int newGoldAmount)
    {
        if (goldText != null)
        {
            goldText.text = $"{newGoldAmount}";
        }
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

    private void UpdateHighScoreText()
    {
        if (highScoreText != null)
        {
            // 从新的存档管理器读取最高分
            int highScore = SaveManager.LoadHighScore();
            highScoreText.text = $"{highScore}";
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    // 【新增方法】
    public void QuitGame()
    {
        Application.Quit();

        // 这行代码仅在Unity编辑器中运行时有效，用于方便测试
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
    private void InitDifficultyUI()
    {
        // 1. 获取当前选中的难度，更新文字
        Difficulty savedDiff = DifficultyManager.Instance.CurrentDifficulty;
        UpdateDifficultyText(savedDiff);

        // 2. 获取解锁进度，更新遮罩
        int unlockedLevel = DifficultyManager.Instance.MaxUnlockedLevel;

        // 逻辑：
        // 如果 unlockedLevel < 1 (只解锁Easy)，则 Normal 锁住
        // 如果 unlockedLevel < 2 (只解锁Easy/Normal)，则 Hard 锁住

        if (normalLockMask) normalLockMask.SetActive(unlockedLevel < 1);
        if (hardLockMask) hardLockMask.SetActive(unlockedLevel < 2);

        // 可选：同时禁用按钮交互，防止点穿遮罩
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
        // 双重保险：UI上锁了，逻辑上也检查一下
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
    // 【新增】用于打开弹窗
    public void OpenDifficultyPopup()
    {
        if (difficultyPopupPanel != null)
        {
            // 1. 激活根节点 (遮罩立刻显示)
            difficultyPopupPanel.SetActive(true);

            // 2. 执行窗口滑入动画
            if (difficultyPopupWindow != null)
            {
                // 初始位置：屏幕下方
                difficultyPopupWindow.anchoredPosition = new Vector2(0, -1200);

                // 动画：滑到中心
                difficultyPopupWindow.DOLocalMove(Vector2.zero, 0.4f).SetEase(Ease.OutBack).SetUpdate(true);
            }
        }
    }

    // 【新增】用于关闭弹窗
    public void CloseDifficultyPopup()
    {
        if (difficultyPopupWindow != null)
        {
            // 1. 动画：滑到屏幕下方
            difficultyPopupWindow.DOLocalMove(new Vector2(0, -1200), 0.4f).SetEase(Ease.InBack).SetUpdate(true)
                .OnComplete(() =>
                {
                    // 2. 动画结束后，关闭整个面板 (包括遮罩)
                    if (difficultyPopupPanel != null)
                    {
                        difficultyPopupPanel.SetActive(false);
                    }
                });
        }
        else
        {
            // 如果没配置窗口引用，直接关闭
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
        // 调用新的通用方法，同时处理颜色和文字
        UpdateButtonState(easyButton, selected == Difficulty.Easy);
        UpdateButtonState(normalButton, selected == Difficulty.Normal);
        UpdateButtonState(hardButton, selected == Difficulty.Hard);
    }

    // 【新增】设置单个按钮颜色的逻辑
    private void UpdateButtonState(Button btn, bool isSelected)
    {
        if (btn != null)
        {
            // 1. 修改背景颜色
            Image img = btn.GetComponent<Image>();
            if (img != null)
            {
                img.color = isSelected ? selectedColor : unselectedColor;
            }

            // 2. 修改按钮文本 (修复冲突)
            Text btnText = btn.GetComponentInChildren<Text>();
            if (btnText != null)
            {
                // 尝试获取挂在上面的自动多语言组件
                LocalizedText locText = btnText.GetComponent<LocalizedText>();

                // 决定要用的 Key
                string targetKey = isSelected ? "DIFFICULTY_SELECTED" : "DIFFICULTY_SELECT";

                if (locText != null)
                {
                    // 【关键】必须用 SetKey！
                    // 这样 LocalizedText 就会记住这个 Key，
                    // 下次切语言时，它会自动用这个 Key 去翻译，而不是用 Inspector 里的默认值。
                    locText.SetKey(targetKey);
                }
                else
                {
                    // 没挂组件的兜底逻辑
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
            // 动态添加 Canvas
            Canvas canvas = target.GetComponent<Canvas>();
            if (canvas == null) canvas = target.AddComponent<Canvas>();

            canvas.overrideSorting = true;
            // 设置一个很高的层级，确保在商店遮罩之上 (商店遮罩通常在默认层级)
            canvas.sortingOrder = 3000;

            // 需要 Raycaster 才能响应点击（虽然金币栏通常不需要点击，但加上比较保险）
            if (target.GetComponent<GraphicRaycaster>() == null) target.AddComponent<GraphicRaycaster>();
        }
        else
        {
            // 移除组件，恢复原状
            GraphicRaycaster gr = target.GetComponent<GraphicRaycaster>();
            if (gr != null) Destroy(gr);

            Canvas canvas = target.GetComponent<Canvas>();
            if (canvas != null) Destroy(canvas);
        }
    }
    private void OnLanguageChanged()
    {
        // 重新刷新一下难度按钮的显示状态（为了更新 选择/已选择 的文本）
        if (DifficultyManager.Instance != null)
        {
            UpdateButtonVisuals(DifficultyManager.Instance.CurrentDifficulty);
            UpdateDifficultyText(DifficultyManager.Instance.CurrentDifficulty);
        }

        // 强制刷新整个界面的布局（解决重叠问题）
        StartCoroutine(ForceRefreshAllLayouts());
    }
    private IEnumerator ForceRefreshAllLayouts()
    {
        // 等待一帧，让文字先更新完
        yield return null;

        // 刷新难度弹窗的布局
        if (difficultyPopupPanel != null)
        {
            // 递归刷新弹窗下的所有 LayoutGroup
            LayoutGroup[] groups = difficultyPopupPanel.GetComponentsInChildren<LayoutGroup>(true);
            foreach (var group in groups)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(group.GetComponent<RectTransform>());
            }
        }

        // 如果还有其他这就重叠的地方，也可以在这里加
        // LayoutRebuilder.ForceRebuildLayoutImmediate(someOtherRect);
    }
}