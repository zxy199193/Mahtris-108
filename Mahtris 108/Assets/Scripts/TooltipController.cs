using UnityEngine;
using UnityEngine.UI;

public class TooltipController : MonoBehaviour
{
    public static TooltipController Instance;

    [Header("UI ×é¼þ")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private Text titleText;
    [SerializeField] private Text descriptionText;
    [SerializeField] private GameObject legendaryIcon;

    [Header("ÅäÖÃ")]
    [SerializeField] private Vector3 offset = new Vector3(0, 100, 0);

    void Awake()
    {
        Instance = this;
        Hide();
    }

    public void Show(string title, string desc, Sprite icon, Sprite bgSprite, bool isLegendary, Transform target)
    {
        if (panel) panel.SetActive(true);
        if (titleText) titleText.text = title;
        if (descriptionText) descriptionText.text = desc;
        if (iconImage) iconImage.sprite = icon;
        if (backgroundImage && bgSprite) backgroundImage.sprite = bgSprite;
        if (legendaryIcon) legendaryIcon.SetActive(isLegendary);

        if (target != null)
        {
            transform.position = target.position + offset;
            //RectTransform targetRect = target as RectTransform;
            //RectTransform tooltipRect = transform as RectTransform;

            //if (targetRect != null && tooltipRect != null)
            //{
            //    tooltipRect.pivot = new Vector2(0.5f, 0.5f);
            //    Vector2 centerPos = targetRect.anchoredPosition;
            //    Vector2 finalPos = centerPos + (Vector2)offset;
            //    tooltipRect.anchoredPosition = finalPos;
            //}
        }
    }

    public void Hide()
    {
        if (panel) panel.SetActive(false);
    }
}