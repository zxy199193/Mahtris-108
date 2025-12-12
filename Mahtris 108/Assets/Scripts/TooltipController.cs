// FileName: TooltipController.cs
using UnityEngine;
using UnityEngine.UI;

public class TooltipController : MonoBehaviour
{
    public static TooltipController Instance;

    [Header("核心配置")]
    // 【新增】允许直接拖入 GameSettings，防止主菜单没有 GameManager 时报错
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
        if (titleText) titleText.text = title;
        if (descriptionText) descriptionText.text = desc;
        if (iconImage) iconImage.sprite = icon;
        if (backgroundImage && bgSprite) backgroundImage.sprite = bgSprite;
        if (legendaryIcon) legendaryIcon.SetActive(isLegendary);

        // 更新标签
        UpdateTypeLabel(type);

        if (target != null)
        {
            transform.position = target.position + offset;

            // 简单的防出界逻辑（可选）
            // Vector3 pos = target.position + offset;
            // transform.position = pos;
        }
    }

    private void UpdateTypeLabel(TooltipTriggerUI.TooltipType type)
    {
        if (typeLabelBackground == null || typeLabelText == null) return;

        if (typeLabelObj) typeLabelObj.SetActive(true);

        // 【核心修复】优先使用 Inspector 拖拽的设置，如果没有才找 GameManager
        GameSettings settings = inspectorSettings;
        if (settings == null && GameManager.Instance != null)
        {
            settings = GameManager.Instance.GetSettings();
        }

        // 如果 settings 还是空的（说明既没拖也没 GameManager），直接返回防止报错
        if (settings == null) return;

        switch (type)
        {
            case TooltipTriggerUI.TooltipType.Advanced:
                typeLabelBackground.color = settings.labelColorAdvanced;
                typeLabelText.text = "高级道具";
                break;

            case TooltipTriggerUI.TooltipType.Protocol:
                typeLabelBackground.color = settings.labelColorProtocol;
                typeLabelText.text = "条约";
                break;

            case TooltipTriggerUI.TooltipType.Common:
            default:
                typeLabelBackground.color = settings.labelColorCommon;
                typeLabelText.text = "道具";
                break;
        }
    }

    public void Hide()
    {
        if (panel) panel.SetActive(false);
    }
}