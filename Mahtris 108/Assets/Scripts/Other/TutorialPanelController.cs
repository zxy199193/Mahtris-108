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

        // 1. 更新内容
        if (tutorialImage) tutorialImage.sprite = pages[currentPage].image;
        if (descriptionText) descriptionText.text = pages[currentPage].description;

        // 2. 更新页码
        if (pageText) pageText.text = $"{currentPage + 1} / {pages.Count}";

        // 3. 【修改】按钮显隐逻辑
        // 第一页时，隐藏"上一页"按钮
        if (prevButton)
        {
            prevButton.gameObject.SetActive(currentPage > 0);
        }

        // 最后一页时，隐藏"下一页"按钮
        if (nextButton)
        {
            nextButton.gameObject.SetActive(currentPage < pages.Count - 1);
        }
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
