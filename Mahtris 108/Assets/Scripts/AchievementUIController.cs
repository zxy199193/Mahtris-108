using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class AchievementUIController : MonoBehaviour
{
    [Header("弹窗结构")]
    [SerializeField] private GameObject root;         // 黑色遮罩
    [SerializeField] private RectTransform container; // 弹窗本体
    [SerializeField] private Transform listContent;   // ScrollView 的 Content
    [SerializeField] private GameObject itemPrefab;   // 列表项预制体
    [SerializeField] private Button closeButton;

    // 动画参数 (与您其他的弹窗保持一致)
    private const float POPUP_SLIDE_DURATION = 0.6f;
    private const float POPUP_HIDDEN_Y = -1500f;

    private void Awake()
    {
        if (closeButton) closeButton.onClick.AddListener(HidePopup);
    }

    public void ShowPopup()
    {
        if (root == null || container == null) return;

        // 1. 刷新数据
        RefreshList();

        // 2. 播放动画
        root.SetActive(true);
        container.DOKill();
        container.anchoredPosition = new Vector2(0, POPUP_HIDDEN_Y);
        container.DOAnchorPosY(0, POPUP_SLIDE_DURATION)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);
    }

    public void HidePopup()
    {
        if (root == null || container == null) return;

        container.DOKill();
        container.DOAnchorPosY(POPUP_HIDDEN_Y, 0.5f)
            .SetEase(Ease.InBack)
            .SetUpdate(true)
            .OnComplete(() => root.SetActive(false));
    }

    private void RefreshList()
    {
        if (AchievementManager.Instance == null) return;

        // 清空旧列表
        foreach (Transform child in listContent) Destroy(child.gameObject);

        // 生成新列表
        // 这里的排序逻辑：已完成的放后面，未完成的放前面？或者按ID排？
        // 目前按 Inspector 里的顺序
        foreach (var data in AchievementManager.Instance.allAchievements)
        {
            GameObject go = Instantiate(itemPrefab, listContent);
            AchievementItemUI ui = go.GetComponent<AchievementItemUI>();

            bool isUnlocked = AchievementManager.Instance.IsUnlocked(data);
            ui.Setup(data, isUnlocked);
        }
    }
}