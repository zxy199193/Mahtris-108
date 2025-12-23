using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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

    void OnEnable()
    {
        StartCoroutine(InitRoutine());
    }

    void OnDisable()
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged -= UpdateContent;
        }
    }

    private IEnumerator InitRoutine()
    {
        while (LocalizationManager.Instance == null)
        {
            yield return null;
        }

        LocalizationManager.Instance.OnLanguageChanged -= UpdateContent;
        LocalizationManager.Instance.OnLanguageChanged += UpdateContent;
        UpdateContent();
    }

    public void SetKey(string newKey)
    {
        this.key = newKey;
        UpdateContent();
    }

    private void UpdateContent()
    {
        if (LocalizationManager.Instance == null || _textComponent == null) return;

        // 1. 更新文本
        if (!string.IsNullOrEmpty(key))
        {
            _textComponent.text = LocalizationManager.Instance.GetText(key, _textComponent.text);
        }

        // 2. 更新字体
        Font globalFont = LocalizationManager.Instance.GetGlobalFont();
        if (globalFont != null)
        {
            _textComponent.font = globalFont;
        }

        // =========================================================
        // 【核心修复】强制刷新布局 (解决文字变长导致的重叠问题)
        // =========================================================
        // 方案：让这一帧结束后，或者强制立即重建父级布局
        // 这里使用 Coroutine 等待一帧是因为某些嵌套布局(Nested Layout)无法立即响应
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(RefreshLayoutNextFrame());
        }
    }

    private IEnumerator RefreshLayoutNextFrame()
    {
        // 等待当前帧的 UI 渲染结束，让 Text 计算出新的 Preferred Width
        yield return null;

        // 查找父物体上的 LayoutGroup (Horizontal/Vertical/Grid)
        LayoutGroup group = GetComponentInParent<LayoutGroup>();
        if (group != null)
        {
            // 强制标记布局为脏，并立即重建
            LayoutRebuilder.ForceRebuildLayoutImmediate(group.GetComponent<RectTransform>());

            // 如果布局嵌套很深（比如 爷爷节点 也是 Layout），可能需要再重建一层
            // LayoutRebuilder.ForceRebuildLayoutImmediate(group.transform.parent as RectTransform);
        }
    }
}