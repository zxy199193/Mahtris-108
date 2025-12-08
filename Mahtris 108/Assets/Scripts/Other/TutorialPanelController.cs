using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPanelController : MonoBehaviour
{
    [Header("UI References")]
    public Image tutorialImage;
    public Text descriptionText;
    public Text pageText;      // 新增：页码显示

    public Button prevButton;
    public Button nextButton;

    [Header("Pages Data")]
    public List<TutorialPage> pages = new List<TutorialPage>();

    private int currentPage = 0;

    void Start()
    {
        UpdatePage();

        prevButton.onClick.AddListener(ShowPrevPage);
        nextButton.onClick.AddListener(ShowNextPage);
    }

    void UpdatePage()
    {
        if (pages.Count == 0) return;

        tutorialImage.sprite = pages[currentPage].image;
        descriptionText.text = pages[currentPage].description;

        // 更新页码
        pageText.text = $"{currentPage + 1} / {pages.Count}";

        // 按钮状态
        prevButton.interactable = currentPage > 0;
        nextButton.interactable = currentPage < pages.Count - 1;
    }

    void ShowPrevPage()
    {
        if (currentPage <= 0) return;
        currentPage--;
        UpdatePage();
    }

    void ShowNextPage()
    {
        if (currentPage >= pages.Count - 1) return;
        currentPage++;
        UpdatePage();
    }
}
