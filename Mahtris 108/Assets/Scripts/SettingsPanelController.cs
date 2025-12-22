using UnityEngine;
using UnityEngine.UI;

public class SettingsPanelController : MonoBehaviour
{
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
            // 从存档读取状态并显示
            bool isFs = SaveManager.LoadFullscreenState();
            fullscreenToggle.isOn = isFs;

            // 绑定事件
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggleChanged);
        }
        // 2. 绑定事件监听
        if (bgmToggle)
        {
            bgmToggle.onValueChanged.AddListener(OnBgmToggleChanged);
        }

        if (sfxToggle)
        {
            sfxToggle.onValueChanged.AddListener(OnSfxToggleChanged);
        }

        if (closeButton)
        {
            closeButton.onClick.AddListener(ClosePanel);
        }
        InitLanguageDropdown();
    }

    // 当 BGM 开关被点击时
    private void OnBgmToggleChanged(bool isOn)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetBgmOn(isOn);

            // 可选：播放点击音效
            if (isOn) AudioManager.Instance.PlayButtonClickSound();
        }
    }

    // 当 音效 开关被点击时
    private void OnSfxToggleChanged(bool isOn)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSfxOn(isOn);

            // 可选：播放点击音效 (只有开启时才听得到反馈，这正好是测试)
            if (isOn) AudioManager.Instance.PlayButtonClickSound();
        }
    }
    private void OnFullscreenToggleChanged(bool isOn)
    {
        // 设置屏幕状态
        Screen.fullScreen = isOn;

        // 保存设置
        SaveManager.SaveFullscreenState(isOn);

        // 可选：如果不希望点击Toggle时有缩放声，这里不要手动播声音
        // 如果你的 Toggle 上挂了 UIButtonClickEffect，它会自动处理声音
    }
    public void ClosePanel()
    {
        // 隐藏自己
        gameObject.SetActive(false);
    }
    private void InitLanguageDropdown()
    {
        if (languageDropdown == null || LocalizationManager.Instance == null) return;

        languageDropdown.ClearOptions();
        // 按 Enum 顺序添加选项
        languageDropdown.AddOptions(new System.Collections.Generic.List<string> {
            "简体中文", "繁體中文", "English", "日本語"
        });

        // 设置当前选中的值
        languageDropdown.value = (int)LocalizationManager.Instance.CurrentLanguage;

        // 绑定事件
        languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
    }

    private void OnLanguageChanged(int index)
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.ChangeLanguage((Language)index);

            // 播放音效
            if (AudioManager.Instance) AudioManager.Instance.PlayButtonClickSound();
        }
    }
}