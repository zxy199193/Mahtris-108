// FileName: LocalizationManager.cs
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;

// 语言枚举
public enum Language
{
    zh_CN,
    zh_TW,
    en_US,
    ja_JP
}

[System.Serializable]
public struct LanguageFontMapping
{
    public Language language;
    public Font font;

    // 如果不设置（为0），代码会自动视为 1.0
    [Tooltip("行间距倍率 (例如: 中文1.2, 英文1.0)")]
    [Range(0.5f, 3.0f)]
    public float lineSpacing;
}

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    public event Action OnLanguageChanged;

    public Language CurrentLanguage { get; private set; } = Language.zh_CN;

    private Dictionary<string, string[]> localizedData = new Dictionary<string, string[]>();

    [Header("字体设置")]
    [SerializeField] private Font defaultFont;
    [Tooltip("在此列表中配置特定语言对应的特定字体和行间距")]
    [SerializeField] private List<LanguageFontMapping> fontMappings;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadCSV();

            // 初始化语言
            string savedLang = SaveManager.LoadLanguage();
            if (Enum.TryParse(savedLang, out Language parsedLang))
            {
                CurrentLanguage = parsedLang;
            }
            else
            {
                // 默认语言逻辑
                if (Application.systemLanguage == SystemLanguage.ChineseTraditional) CurrentLanguage = Language.zh_TW;
                else if (Application.systemLanguage == SystemLanguage.Japanese) CurrentLanguage = Language.ja_JP;
                else if (Application.systemLanguage == SystemLanguage.English) CurrentLanguage = Language.en_US;
                else CurrentLanguage = Language.zh_CN;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadCSV()
    {
        TextAsset csvFile = Resources.Load<TextAsset>("Localization");
        if (csvFile == null)
        {
            Debug.LogError("【严重】未找到 Resources/Localization.csv 文件！");
            return;
        }

        string fileContent = csvFile.text.Replace("\r\n", "\n").Replace("\r", "\n");
        string[] lines = fileContent.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        Regex csvParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");

        localizedData.Clear();

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] parts = csvParser.Split(line);

            if (parts.Length >= 5)
            {
                string key = parts[0].Trim();
                string[] values = new string[4];
                for (int j = 0; j < 4; j++)
                {
                    string content = parts[j + 1];
                    content = content.Trim('\"');
                    content = content.Replace("\"\"", "\"");
                    content = content.Replace("\\n", "\n");
                    values[j] = content;
                }

                if (!localizedData.ContainsKey(key))
                {
                    localizedData.Add(key, values);
                }
            }
        }
        Debug.Log($"【多语言】加载成功，共 {localizedData.Count} 条数据。");
    }

    public string GetText(string key, string defaultText = "")
    {
        if (string.IsNullOrEmpty(key)) return defaultText;
        key = key.Trim();

        if (localizedData.TryGetValue(key, out string[] values))
        {
            int langIndex = (int)CurrentLanguage;
            if (langIndex < values.Length)
            {
                return values[langIndex];
            }
        }
        return string.IsNullOrEmpty(defaultText) ? key : defaultText;
    }

    public void ChangeLanguage(Language newLang)
    {
        if (CurrentLanguage == newLang) return;
        CurrentLanguage = newLang;
        SaveManager.SaveLanguage(newLang.ToString());
        OnLanguageChanged?.Invoke();
    }

    // 获取全局字体 (保留以兼容旧调用，主要逻辑在 UpdateFont)
    public Font GetGlobalFont()
    {
        if (fontMappings != null)
        {
            foreach (var mapping in fontMappings)
            {
                if (mapping.language == CurrentLanguage && mapping.font != null)
                {
                    return mapping.font;
                }
            }
        }
        return defaultFont;
    }

    public void UpdateFont(UnityEngine.UI.Text targetText)
    {
        if (targetText == null) return;

        // 1. 查找当前语言的配置
        LanguageFontMapping currentMapping = default;
        bool found = false;

        if (fontMappings != null)
        {
            foreach (var mapping in fontMappings)
            {
                if (mapping.language == CurrentLanguage)
                {
                    currentMapping = mapping;
                    found = true;
                    break;
                }
            }
        }

        // 2. 应用字体
        if (found && currentMapping.font != null)
        {
            targetText.font = currentMapping.font;
        }
        else
        {
            targetText.font = defaultFont;
        }

        // 3. 应用行间距
        // 逻辑：如果配置了 > 0.1 的值，就用配置值；否则默认 1.0 (防止没配置时字叠在一起)
        float lineSpacing = (found && currentMapping.lineSpacing > 0.1f) ? currentMapping.lineSpacing : 1.0f;

        targetText.lineSpacing = lineSpacing;
    }
}