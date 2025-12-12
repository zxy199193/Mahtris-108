// FileName: TooltipTriggerUI.cs
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTriggerUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // 定义浮窗内容的类型
    public enum TooltipType
    {
        Common,     // 普通道具
        Advanced,   // 高级道具
        Protocol    // 条约
    }

    private string title;
    private string description;
    private Sprite icon;
    private bool isLegendary;
    private TooltipType type; // 存储类型

    // 【关键修改】这里必须有 5 个参数，且使用了默认值
    public void SetData(string title, string description, Sprite icon = null, bool isLegendary = false, TooltipType type = TooltipType.Common)
    {
        this.title = title;
        this.description = description;
        this.icon = icon;
        this.isLegendary = isLegendary;

        // 【修复警告】加上 this. 前缀，防止 "type = type" 导致的自我赋值警告
        this.type = type;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (string.IsNullOrEmpty(title) && string.IsNullOrEmpty(description)) return;

        if (TooltipController.Instance != null)
        {
            // 获取配置（优先 Inspector，其次 GameManager）
            // 注意：这里为了方便，我们假设 TooltipController 里已经有 InspectorSettings 了
            // 为了稳健，还是走 GameManager
            GameSettings settings = GameManager.Instance.GetSettings();
            if (settings == null) return;

            Sprite bg = null;

            // 1. 传奇背景优先
            if (isLegendary)
            {
                bg = settings.tooltipBgLegendary;
            }
            else
            {
                // 2. 根据存储的 type 选择背景
                switch (this.type) // 使用成员变量 this.type
                {
                    case TooltipType.Advanced:
                        bg = settings.tooltipBgAdvanced;
                        break;
                    case TooltipType.Protocol:
                        bg = settings.tooltipBgProtocol;
                        break;
                    case TooltipType.Common:
                    default:
                        bg = settings.tooltipBgCommon;
                        break;
                }
                // 保底
                if (bg == null) bg = settings.tooltipBgCommon;
            }

            // 【关键】将 this.type 传给 TooltipController
            TooltipController.Instance.Show(title, description, icon, bg, isLegendary, this.type, this.transform);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipController.Instance != null)
        {
            TooltipController.Instance.Hide();
        }
    }
}