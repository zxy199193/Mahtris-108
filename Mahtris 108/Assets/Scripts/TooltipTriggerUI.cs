// FileName: TooltipTriggerUI.cs
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTriggerUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private string title;
    private string description;
    private bool hasData = false;

    /// <summary>
    /// ���´˴�������ʾ����ʾ��Ϣ
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
        // ���������ݣ��ǿղ�λ��ʱ��ʾ��ʾ
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

    // �����屻����ʱ�������λ��գ���Ҳ������ʾ
    void OnDisable()
    {
        if (TooltipSystem.Instance != null)
        {
            TooltipSystem.Instance.Hide();
        }
    }
}