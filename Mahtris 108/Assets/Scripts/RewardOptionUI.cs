// FileName: RewardOptionUI.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class RewardOptionUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI 引用")]
    public Button optionButton;
    public Image optionIcon; // 用于显示道具/条约的图标
    public Transform shapeContainer; // 新增：用于容纳方块形状UI预制件的容器
    public Text optionText; // 用于显示方块倍率等

    private Action<RewardOptionUI> onClickCallback;
    private Action<string, string> onHoverEnterCallback;
    private Action onHoverExitCallback;

    private string title;
    private string description;

    // 方法1：用于初始化显示Sprite图标的奖励（道具/条约）
    public void InitializeForSprite(Sprite icon, string title, string desc, Action<RewardOptionUI> onClick, Action<string, string> onHoverEnter, Action onHoverExit)
    {
        if (shapeContainer) shapeContainer.gameObject.SetActive(false);
        if (optionIcon)
        {
            optionIcon.gameObject.SetActive(true);
            optionIcon.sprite = icon;
        }
        if (optionText) optionText.text = ""; // 道具和条约不显示额外文本

        SetupCallbacks(title, desc, onClick, onHoverEnter, onHoverExit);
    }

    // 方法2：用于初始化显示GameObject预制件的奖励（方块）
    public void InitializeForPrefab(GameObject uiPrefab, string text, string title, string desc, Action<RewardOptionUI> onClick, Action<string, string> onHoverEnter, Action onHoverExit)
    {
        if (optionIcon) optionIcon.gameObject.SetActive(false);
        if (shapeContainer)
        {
            shapeContainer.gameObject.SetActive(true);
            // 清空旧的形状
            foreach (Transform child in shapeContainer) Destroy(child.gameObject);
            // 实例化新的形状
            if (uiPrefab) Instantiate(uiPrefab, shapeContainer);
        }
        if (optionText) optionText.text = text; // 显示方块倍率

        SetupCallbacks(title, desc, onClick, onHoverEnter, onHoverExit);
    }

    // 统一设置回调和通用信息
    private void SetupCallbacks(string title, string desc, Action<RewardOptionUI> onClick, Action<string, string> onHoverEnter, Action onHoverExit)
    {
        this.title = title;
        this.description = desc;
        this.onClickCallback = onClick;
        this.onHoverEnterCallback = onHoverEnter;
        this.onHoverExitCallback = onHoverExit;

        if (optionButton)
        {
            optionButton.onClick.RemoveAllListeners(); // 先移除旧的监听器
            optionButton.onClick.AddListener(OnClick);
        }
    }

    private void OnClick()
    {
        onClickCallback?.Invoke(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        onHoverEnterCallback?.Invoke(title, description);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        onHoverExitCallback?.Invoke();
    }

    public void SetInteractable(bool interactable)
    {
        if (optionButton) optionButton.interactable = interactable;
    }
}