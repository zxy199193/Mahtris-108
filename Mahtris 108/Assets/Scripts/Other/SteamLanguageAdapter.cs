using UnityEngine;
using Steamworks;

public class SteamLanguageAdapter : MonoBehaviour
{
    // 这里设置一个标志位，防止重复检测（可选）
    private bool hasChecked = false;

    void Start()
    {
        if (hasChecked) return;
        hasChecked = true;

        // 1. 检查 Steam 是否初始化
        if (!SteamManager.Initialized) return;

        // 2. 检查是否已经是“老玩家”
        // 逻辑：如果 SaveManager 里已经存了语言设置，说明玩家之前玩过或手动改过语言
        // 这时候我们要尊重玩家的选择，不要用 Steam 语言覆盖它
        string savedLang = SaveManager.LoadLanguage();
        if (!string.IsNullOrEmpty(savedLang))
        {
            // Debug.Log("[SteamAdapter] 检测到已有语言存档，跳过 Steam 自动匹配。");
            return;
        }

        // 3. 获取 Steam 客户端语言 (返回全小写字符串)
        string steamLang = SteamApps.GetCurrentGameLanguage();
        Debug.Log($"[SteamAdapter] 首次运行，Steam 客户端语言为: {steamLang}");

        // 4. 进行映射匹配
        Language targetLang = Language.en_US; // 默认兜底为英文
        bool isSupported = false;

        switch (steamLang)
        {
            case "schinese": // 简体中文
                targetLang = Language.zh_CN;
                isSupported = true;
                break;
            case "tchinese": // 繁体中文
                targetLang = Language.zh_TW;
                isSupported = true;
                break;
            case "japanese": // 日语
                targetLang = Language.ja_JP;
                isSupported = true;
                break;
            case "english":  // 英语
                targetLang = Language.en_US;
                isSupported = true;
                break;
            default:
                // 如果是法语、德语等您没做的语言，就保持 en_US
                targetLang = Language.en_US;
                break;
        }

        // 5. 应用语言
        if (isSupported)
        {
            // 调用您的管理器切换语言
            // 注意：您的 ChangeLanguage 方法里包含了 SaveManager.SaveLanguage
            // 所以一旦切换成功，下次运行就会直接读取存档，不再触发这里的逻辑
            LocalizationManager.Instance.ChangeLanguage(targetLang);
            Debug.Log($"[SteamAdapter] 已自动切换游戏语言至: {targetLang}");
        }
    }
}