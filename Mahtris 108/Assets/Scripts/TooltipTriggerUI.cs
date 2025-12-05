// FileName: TooltipTriggerUI.cs
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTriggerUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private string title;
    private string description;
    private Sprite icon;
    private bool isLegendary;

    // 供 GameUIController 动态设置数据用
    public void SetData(string title, string description, Sprite icon = null, bool isLegendary = false)
    {
        this.title = title;
        this.description = description;
        this.icon = icon;
        this.isLegendary = isLegendary;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (string.IsNullOrEmpty(title) && string.IsNullOrEmpty(description)) return;

        // 调用新的 TooltipController
        if (TooltipController.Instance != null)
        {
            // 获取设置中的背景图
            GameSettings settings = GameManager.Instance.GetSettings();
            Sprite bg = isLegendary ? settings.tooltipBgLegendary : settings.tooltipBgCommon; // 默认为普通背景，您可以根据需要扩展判断逻辑

            TooltipController.Instance.Show(title, description, icon, bg, isLegendary, this.transform);
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