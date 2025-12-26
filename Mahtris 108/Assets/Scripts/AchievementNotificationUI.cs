// FileName: AchievementNotificationUI.cs
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class AchievementNotificationUI : MonoBehaviour
{
    [SerializeField] private RectTransform container;
    [SerializeField] private Image icon;
    [SerializeField] private Text title;
    [SerializeField] private Text reward;
    [SerializeField] private Text description;
    [SerializeField] private Canvas canvas;

    public void Show(AchievementData data, Action onComplete)
    {
        if (icon) icon.sprite = data.icon;
        if (title) title.text = data.GetName();
        if (reward) reward.text = $"{data.rewardGold}";
        if (description) description.text = data.GetDescription();
        // 1. 强制置顶渲染层级
        if (canvas == null) canvas = GetComponent<Canvas>();
        if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = 32767;

        if (container)
        {
            // 强制重置锚点为 Top-Center
            container.anchorMin = new Vector2(0.5f, 1f);
            container.anchorMax = new Vector2(0.5f, 1f);
            container.pivot = new Vector2(0.5f, 1f);

            // 初始位置
            container.anchoredPosition = new Vector2(0, 150);

            // 2. 动画序列
            container.DOKill();
            Sequence seq = DOTween.Sequence();

            // 下滑 (进入)
            seq.Append(container.DOAnchorPosY(0f, 0.3f).SetEase(Ease.OutBack));

            // 停留
            seq.AppendInterval(2.4f);

            // 上滑 (退出)
            seq.Append(container.DOAnchorPosY(150, 0.3f).SetEase(Ease.InBack));

            // 【核心修改】动画全部完成后
            seq.OnComplete(() => {
                // 1. 先通知管理器“我播完了，你可以播下一个了”
                onComplete?.Invoke();

                // 2. 再自我销毁
                Destroy(gameObject);
            });

            seq.SetUpdate(true);
        }
        else
        {
            //以此防预制体配置错误导致的回调丢失，导致队列卡死
            onComplete?.Invoke();
            Destroy(gameObject);
        }
    }
}