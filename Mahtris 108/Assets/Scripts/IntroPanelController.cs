using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // 引入 DOTween

public class IntroPanelController : MonoBehaviour
{
    [Header("动画容器")]
    [SerializeField] private RectTransform popupWindow; // 【新增】请拖入实际显示内容的子物体 (Container)

    [Header("Panels")]
    public GameObject tutorialPanel;   // 教学界面
    public GameObject huTypePanel;      // 其他界面

    [Header("Buttons")]
    public Button tabTutorialButton;
    public Button tabhuTypeButton;
    public Button closeButton;

    [Header("Tab Indicators")]
    [SerializeField] private GameObject tabTutorialArrow;
    [SerializeField] private GameObject tabHuTypeArrow;

    [Header("Tutorial Controller (optional)")]
    public TutorialPanelController tutorialPanelController;

    private void Start()
    {
        // 绑定按钮事件
        if (tabTutorialButton) tabTutorialButton.onClick.AddListener(ShowTeachingPanel);
        if (tabhuTypeButton) tabhuTypeButton.onClick.AddListener(ShowOtherPanel);
        if (closeButton) closeButton.onClick.AddListener(Close);

        // 默认显示教学界面
        ShowTeachingPanel();
    }
    private void OnEnable()
    {
        if (popupWindow != null)
        {
            popupWindow.DOKill();
            popupWindow.anchoredPosition = new Vector2(0, -1200); // 重置位置
            popupWindow.DOLocalMove(Vector2.zero, 0.4f) // 播放动画
                .SetEase(Ease.OutBack)
                .SetUpdate(true);
        }
    }
    public void Open()
    {
        gameObject.SetActive(true);
    }

    public void Close()
    {
        if (popupWindow != null)
        {
            popupWindow.DOKill();
            // 3. 播放滑出动画
            popupWindow.DOLocalMove(new Vector2(0, -1200), 0.4f)
                .SetEase(Ease.InBack)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    gameObject.SetActive(false); // 动画结束后禁用
                });
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void ShowTeachingPanel()
    {
        if (tutorialPanel) tutorialPanel.SetActive(true);
        if (huTypePanel) huTypePanel.SetActive(false);

        if (tabTutorialArrow) tabTutorialArrow.SetActive(true);
        if (tabHuTypeArrow) tabHuTypeArrow.SetActive(false);
    }

    public void ShowOtherPanel()
    {
        if (tutorialPanel) tutorialPanel.SetActive(false);
        if (huTypePanel) huTypePanel.SetActive(true);

        if (tabTutorialArrow) tabTutorialArrow.SetActive(false);
        if (tabHuTypeArrow) tabHuTypeArrow.SetActive(true);
    }
}