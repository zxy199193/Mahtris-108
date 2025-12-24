using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // 引入 DOTween

public class SettingsPanelController : MonoBehaviour
{
    [Header("动画容器")]
    [SerializeField] private RectTransform popupWindow; // 【新增】请拖入实际显示内容的子物体

    [Header("UI 组件")]
    [SerializeField] private Toggle bgmToggle;
    [SerializeField] private Toggle sfxToggle;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Button closeButton;
    [Header("多语言")]
    [SerializeField] private Dropdown languageDropdown;

    void Start()
    {
        // 1. 初始化 Toggle 的显示状态 (勾选 or 不勾选)
        if (AudioManager.Instance != null)
        {
            if (bgmToggle) bgmToggle.isOn = AudioManager.Instance.IsBgmOn;
            if (sfxToggle) sfxToggle.isOn = AudioManager.Instance.IsSfxOn;
        }

        if (fullscreenToggle)
        {
            bool isFs = SaveManager.LoadFullscreenState();
            fullscreenToggle.isOn = isFs;
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggleChanged);
        }

        // 2. 绑定事件监听
        if (bgmToggle) bgmToggle.onValueChanged.AddListener(OnBgmToggleChanged);
        if (sfxToggle) sfxToggle.onValueChanged.AddListener(OnSfxToggleChanged);

        // 注意：这里改成了调用 ClosePanel，里面包含了动画逻辑
        if (closeButton) closeButton.onClick.AddListener(ClosePanel);

        InitLanguageDropdown();
    }

    // 【新增】打开面板的方法 (请确保按钮点击时调用的是这个方法，而不是直接 SetActive)

    private void OnEnable()
    {
        if (popupWindow != null)
        {
            popupWindow.DOKill(); // 杀掉之前的动画
            popupWindow.anchoredPosition = new Vector2(0, -1200); // 1. 先把位置重置到屏幕外

            // 2. 再滑进来
            popupWindow.DOLocalMove(Vector2.zero, 0.4f)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);
        }
    }
    public void Open()
    {
        gameObject.SetActive(true);
    }

    // 当 BGM 开关被点击时
    private void OnBgmToggleChanged(bool isOn)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetBgmOn(isOn);
            if (isOn) AudioManager.Instance.PlayButtonClickSound();
        }
    }

    // 当 音效 开关被点击时
    private void OnSfxToggleChanged(bool isOn)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSfxOn(isOn);
            if (isOn) AudioManager.Instance.PlayButtonClickSound();
        }
    }

    private void OnFullscreenToggleChanged(bool isOn)
    {
        Screen.fullScreen = isOn;
        SaveManager.SaveFullscreenState(isOn);
    }

    public void ClosePanel()
    {
        if (popupWindow != null)
        {
            popupWindow.DOKill();
            popupWindow.DOLocalMove(new Vector2(0, -1200), 0.4f)
                .SetEase(Ease.InBack)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    gameObject.SetActive(false);
                });
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void InitLanguageDropdown()
    {
        if (languageDropdown == null || LocalizationManager.Instance == null) return;

        languageDropdown.ClearOptions();
        languageDropdown.AddOptions(new System.Collections.Generic.List<string> {
            "简体中文", "繁體中文", "English", "日本語"
        });

        languageDropdown.value = (int)LocalizationManager.Instance.CurrentLanguage;
        languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
    }

    private void OnLanguageChanged(int index)
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.ChangeLanguage((Language)index);
            if (AudioManager.Instance) AudioManager.Instance.PlayButtonClickSound();
        }
    }
}