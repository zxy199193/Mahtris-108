using UnityEngine;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions; // 必须引入正则库

// =========================================================
// 【关键修复】 Enum 定义必须放在这里 (类外面)，或者单独放在一个文件里
// 之前报错就是因为缺少了这行代码
// =========================================================
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
}
public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    public event Action OnLanguageChanged;

    // 这里使用上面定义的 Language 枚举
    public Language CurrentLanguage { get; private set; } = Language.zh_CN;

    private Dictionary<string, string[]> localizedData = new Dictionary<string, string[]>();

    [Header("字体设置")]
    [SerializeField] private Font defaultFont;
    [Tooltip("在此列表中配置特定语言对应的特定字体")]
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
            // 尝试解析保存的字符串为 Language 枚举
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

        // 1. 处理换行符 (兼容 Windows/Mac)
        string fileContent = csvFile.text.Replace("\r\n", "\n").Replace("\r", "\n");
        string[] lines = fileContent.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

        // 2. 正则表达式：匹配逗号，但忽略双引号内的逗号 (Excel CSV 标准格式)
        Regex csvParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");

        localizedData.Clear();

        // 从第1行开始（跳过表头）
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line)) continue;

            // 使用正则分割
            string[] parts = csvParser.Split(line);

            if (parts.Length >= 5)
            {
                // Trim() 去掉 Key 前后可能存在的空格
                string key = parts[0].Trim();

                string[] values = new string[4];
                for (int j = 0; j < 4; j++)
                {
                    string content = parts[j + 1];

                    // 处理 Excel 的转义字符
                    content = content.Trim('\"'); // 去掉包裹的引号
                    content = content.Replace("\"\"", "\""); // 处理双引号转义
                    content = content.Replace("\\n", "\n"); // 处理手动换行

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

        key = key.Trim(); // 安全措施

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

    public Font GetGlobalFont()
    {
        // 遍历列表查找当前语言的配置
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

        // 如果没找到特定配置，返回默认字体
        return defaultFont;
    }
    public void UpdateFont(UnityEngine.UI.Text targetText)
    {
        if (targetText == null) return;

        Font currentFont = GetGlobalFont();
        if (currentFont != null)
        {
            targetText.font = currentFont;
        }
    }
}