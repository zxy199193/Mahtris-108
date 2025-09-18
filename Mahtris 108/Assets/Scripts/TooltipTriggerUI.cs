// FileName: TooltipTriggerUI.cs
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTriggerUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private string title;
    private string description;
    private bool hasData = false;

    /// <summary>
    /// 更新此触发器显示的提示信息
    /// </summary>
    public void SetData(string newTitle, string newDescription)
    {
        if (!string.IsNullOrEmpty(newTitle))
        {
            this.title = newTitle;
            this.description = newDescription;
            this.hasData = true;
        }
        else
        {
            this.hasData = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 仅在有数据（非空槽位）时显示提示
        if (hasData && TooltipSystem.Instance != null)
        {
            TooltipSystem.Instance.Show(title, description);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipSystem.Instance != null)
        {
            TooltipSystem.Instance.Hide();
        }
    }

    // 当物体被禁用时（例如槽位变空），也隐藏提示
    void OnDisable()
    {
        if (TooltipSystem.Instance != null)
        {
            TooltipSystem.Instance.Hide();
        }
    }
}