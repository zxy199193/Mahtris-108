// FileName: TooltipSystem.cs
using UnityEngine;
using UnityEngine.UI;

public class TooltipSystem : MonoBehaviour
{
    public static TooltipSystem Instance { get; private set; }

    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private Text titleText;
    [SerializeField] private Text descriptionText;
    [SerializeField] private RectTransform tooltipRect;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (tooltipPanel) tooltipPanel.SetActive(false);
    }

    public void Show(string title, string description)
    {
        if (tooltipPanel == null) return;

        if (titleText) titleText.text = title;
        if (descriptionText) descriptionText.text = description;
        tooltipPanel.SetActive(true);
    }

    public void Hide()
    {
        if (tooltipPanel) tooltipPanel.SetActive(false);
    }

    void Update()
    {
        if (tooltipPanel != null && tooltipPanel.activeSelf)
        {
            // »√Ã· æøÚ∏˙ÀÊ Û±Í
            if (tooltipRect != null)
            {
                tooltipRect.position = Input.mousePosition;
            }
        }
    }
}