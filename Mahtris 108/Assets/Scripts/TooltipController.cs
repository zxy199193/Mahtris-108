using UnityEngine;
using UnityEngine.UI;

public class TooltipController : MonoBehaviour
{
    public static TooltipController Instance;

    [Header("核心配置")]
    // 允许直接拖入 GameSettings，防止主菜单没有 GameManager 时报错
    [SerializeField] private GameSettings inspectorSettings;

    [Header("UI 组件")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private Text titleText;
    [SerializeField] private Text descriptionText;
    [SerializeField] private GameObject legendaryIcon;

    [Header("类型标签组件")]
    [SerializeField] private GameObject typeLabelObj;
    [SerializeField] private Image typeLabelBackground;
    [SerializeField] private Text typeLabelText;

    [Header("配置")]
    [SerializeField] private Vector3 offset = new Vector3(0, 100, 0);

    void Awake()
    {
        Instance = this;
        Hide();
    }

    public void Show(string title, string desc, Sprite icon, Sprite bgSprite, bool isLegendary, TooltipTriggerUI.TooltipType type, Transform target)
    {
        if (panel) panel.SetActive(true);

        // 尝试翻译 Title 和 Description
        // 如果传入的是 Key (如 "ITEM_BOMB")，GetLocalizedText 会返回翻译
        // 如果传入的是普通文本且找不到 Key，它会原样返回
        if (titleText)
        {
            titleText.text = GetLocalizedText(title, title);
        }

        if (descriptionText)
        {
            descriptionText.text = GetLocalizedText(desc, desc);
        }

        if (iconImage) iconImage.sprite = icon;
        if (backgroundImage && bgSprite) backgroundImage.sprite = bgSprite;
        if (legendaryIcon) legendaryIcon.SetActive(isLegendary);

        // 统一刷新字体
        if (LocalizationManager.Instance)
        {
            LocalizationManager.Instance.UpdateFont(titleText);
            LocalizationManager.Instance.UpdateFont(descriptionText);
            LocalizationManager.Instance.UpdateFont(typeLabelText);
        }

        UpdateTypeLabel(type);

        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }

    private void UpdateTypeLabel(TooltipTriggerUI.TooltipType type)
    {
        if (typeLabelBackground == null || typeLabelText == null) return;

        if (typeLabelObj) typeLabelObj.SetActive(true);

        // 获取配置
        GameSettings settings = inspectorSettings;
        if (settings == null && GameManager.Instance != null)
        {
            settings = GameManager.Instance.GetSettings();
        }

        if (settings == null) return;

        string localizedLabel = "";

        switch (type)
        {
            case TooltipTriggerUI.TooltipType.Advanced:
                typeLabelBackground.color = settings.labelColorAdvanced;
                localizedLabel = GetLocalizedText("TIP_ADVANCED_ITEM", "高级道具");
                break;

            case TooltipTriggerUI.TooltipType.Protocol:
                typeLabelBackground.color = settings.labelColorProtocol;
                localizedLabel = GetLocalizedText("TIP_PROTOCOL", "条约");
                break;

            case TooltipTriggerUI.TooltipType.Common:
            default:
                typeLabelBackground.color = settings.labelColorCommon;
                localizedLabel = GetLocalizedText("TIP_ITEM", "道具");
                break;
        }

        typeLabelText.text = localizedLabel;

        if (LocalizationManager.Instance)
        {
            LocalizationManager.Instance.UpdateFont(typeLabelText);
        }
    }

    public void Hide()
    {
        if (panel) panel.SetActive(false);
    }

    // 辅助方法：安全获取文本 (已移除 Debug Log)
    private string GetLocalizedText(string key, string defaultText)
    {
        if (LocalizationManager.Instance == null)
        {
            return defaultText;
        }

        return LocalizationManager.Instance.GetText(key, defaultText);
    }
}