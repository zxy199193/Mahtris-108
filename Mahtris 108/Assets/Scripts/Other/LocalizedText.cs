using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class LocalizedText : MonoBehaviour
{
    [Tooltip("对应 CSV 中的 Key")]
    public string key;

    private Text _textComponent;

    void Awake()
    {
        _textComponent = GetComponent<Text>();
    }

    void Start()
    {
        UpdateContent();
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged += UpdateContent;
        }
    }

    void OnDestroy()
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged -= UpdateContent;
        }
    }

    // 公开方法，允许代码动态修改 Key (例如动态生成的文本)
    public void SetKey(string newKey)
    {
        this.key = newKey;
        UpdateContent();
    }

    private void UpdateContent()
    {
        if (LocalizationManager.Instance == null || _textComponent == null) return;

        // 1. 获取翻译
        if (!string.IsNullOrEmpty(key))
        {
            _textComponent.text = LocalizationManager.Instance.GetText(key, _textComponent.text);
        }

        // 2. 更新字体 (可选，防止中文在英文字体下乱码)
        Font globalFont = LocalizationManager.Instance.GetGlobalFont();
        if (globalFont != null)
        {
            _textComponent.font = globalFont;
        }
    }
}