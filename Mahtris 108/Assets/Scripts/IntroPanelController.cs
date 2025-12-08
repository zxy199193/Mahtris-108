using UnityEngine;
using UnityEngine.UI;

public class IntroPanelController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject tutorialPanel;   // 教学界面
    public GameObject huTypePanel;      // 其他界面

    [Header("Buttons")]
    public Button tabTutorialButton;
    public Button tabhuTypeButton;

    public Button closeButton;

    [Header("Tutorial Controller (optional)")]
    public TutorialPanelController tutorialPanelController;

    private void Start()
    {
        // 绑定按钮事件
        tabTutorialButton.onClick.AddListener(ShowTeachingPanel);
        tabhuTypeButton.onClick.AddListener(ShowOtherPanel);
        closeButton.onClick.AddListener(Close);

        // 默认显示教学界面
        ShowTeachingPanel();
    }

    public void Open()
    {
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void ShowTeachingPanel()
    {
        tutorialPanel.SetActive(true);
        huTypePanel.SetActive(false);

        // 高亮选中状态（可根据需要添加样式）
    }

    public void ShowOtherPanel()
    {
        tutorialPanel.SetActive(false);
        huTypePanel.SetActive(true);
    }
}
