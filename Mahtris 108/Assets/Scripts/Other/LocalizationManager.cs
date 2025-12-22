using UnityEngine;
using System.Collections.Generic;
using System;

public enum Language { zh_CN, zh_TW, en_US, ja_JP }

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    // 事件：当语言改变时触发，通知所有 UI 刷新
    public event Action OnLanguageChanged;

    public Language CurrentLanguage { get; private set; } = Language.zh_CN;

    // 字典结构： Key -> [zh_CN内容, zh_TW内容, en_US内容, ja_JP内容]
    private Dictionary<string, string[]> localizedData = new Dictionary<string, string[]>();

    // 字体管理 (不同语言可能需要不同字体，或者统一用一个支持 CJK 的字体)
    [Header("字体设置")]
    [SerializeField] private Font defaultFont; // 拖入一个含中日韩字符的字体 (如 NotoSansCJK)

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
                // 默认根据系统语言判断
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
        // 读取 Resources/Localization.csv
        TextAsset csvFile = Resources.Load<TextAsset>("Localization");
        if (csvFile == null)
        {
            Debug.LogError("未找到 Resources/Localization.csv 文件！");
            return;
        }

        string[] lines = csvFile.text.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        // 从第1行开始（跳过表头 Key,zh_CN...）
        for (int i = 1; i < lines.Length; i++)
        {
            string[] parts = lines[i].Split(','); // 注意：如果文本里有逗号，需用更复杂的CSV解析，这里暂用简易版
            if (parts.Length >= 5)
            {
                string key = parts[0].Trim();
                string[] values = new string[4];
                values[0] = parts[1]; // zh_CN
                values[1] = parts[2]; // zh_TW
                values[2] = parts[3]; // en_US
                values[3] = parts[4]; // ja_JP

                // 处理换行符 (CSV里通常用 \n 代表换行)
                for (int j = 0; j < 4; j++) values[j] = values[j].Replace("\\n", "\n");

                if (!localizedData.ContainsKey(key))
                {
                    localizedData.Add(key, values);
                }
            }
        }
        Debug.Log($"多语言加载完毕，共 {localizedData.Count} 条数据。");
    }

    public string GetText(string key, string defaultText = "")
    {
        if (string.IsNullOrEmpty(key)) return defaultText;

        if (localizedData.TryGetValue(key, out string[] values))
        {
            return values[(int)CurrentLanguage];
        }

        return string.IsNullOrEmpty(defaultText) ? key : defaultText; // 找不到Key就返回Key本身或默认值
    }

    public void ChangeLanguage(Language newLang)
    {
        if (CurrentLanguage == newLang) return;

        CurrentLanguage = newLang;
        SaveManager.SaveLanguage(newLang.ToString());

        OnLanguageChanged?.Invoke();
        Debug.Log($"语言切换为: {newLang}");
    }

    public Font GetGlobalFont()
    {
        return defaultFont;
    }
}