using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPanelController : MonoBehaviour
{
    [Header("容器引用")]
    [Tooltip("用于挂载所有教学页预制体的父节点")]
    public Transform pageContainer;

    [Header("UI 引用")]
    public Text pageText;      // 页码显示 (1/5)
    public Button prevButton;
    public Button nextButton;

    [Header("数据配置")]
    [Tooltip("请按顺序拖入做好的教学页预制体")]
    public List<GameObject> pagePrefabs = new List<GameObject>();

    // 运行时实例化的页面列表
    private List<GameObject> _instantiatedPages = new List<GameObject>();
    private int currentPage = 0;

    void Start()
    {
        InitializePages();
        UpdatePage();

        if (prevButton) prevButton.onClick.AddListener(ShowPrevPage);
        if (nextButton) nextButton.onClick.AddListener(ShowNextPage);
    }

    // 初始化：生成所有页面，但默认全部隐藏
    void InitializePages()
    {
        if (pageContainer == null)
        {
            Debug.LogError("请在 Inspector 中赋值 Page Container！");
            return;
        }

        // 1. 清理容器中可能存在的旧物体（比如编辑器里留下的占位符）
        foreach (Transform child in pageContainer)
        {
            Destroy(child.gameObject);
        }
        _instantiatedPages.Clear();

        // 2. 实例化所有预制体
        foreach (var prefab in pagePrefabs)
        {
            if (prefab != null)
            {
                GameObject page = Instantiate(prefab, pageContainer);

                // 确保位置归零，大小匹配
                RectTransform rt = page.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchoredPosition = Vector2.zero;
                    rt.localScale = Vector3.one;
                }

                page.SetActive(false); // 默认隐藏
                _instantiatedPages.Add(page);
            }
        }
    }

    void UpdatePage()
    {
        if (_instantiatedPages.Count == 0) return;

        // 1. 控制页面的显隐：只显示当前页，其他隐藏
        for (int i = 0; i < _instantiatedPages.Count; i++)
        {
            _instantiatedPages[i].SetActive(i == currentPage);
        }

        // 2. 更新页码
        if (pageText) pageText.text = $"{currentPage + 1} / {_instantiatedPages.Count}";

        // 3. 按钮显隐逻辑
        if (prevButton)
        {
            prevButton.gameObject.SetActive(currentPage > 0);
        }

        if (nextButton)
        {
            nextButton.gameObject.SetActive(currentPage < _instantiatedPages.Count - 1);
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
        if (currentPage >= _instantiatedPages.Count - 1) return;
        currentPage++;
        UpdatePage();
    }
}