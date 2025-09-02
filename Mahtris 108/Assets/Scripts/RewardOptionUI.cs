// FileName: RewardOptionUI.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class RewardOptionUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI 引用")]
    public Button optionButton;
    public Image optionIcon;
    public Text optionText; // 可选，用于显示方块倍率等

    private Action<RewardOptionUI> onClickCallback;
    private Action<string, string> onHoverEnterCallback;
    private Action onHoverExitCallback;

    private string title;
    private string description;

    public void Initialize(Sprite icon, string title, string desc, Action<RewardOptionUI> onClick, Action<string, string> onHoverEnter, Action onHoverExit)
    {
        if (optionIcon) this.optionIcon.sprite = icon;
        this.title = title;
        this.description = desc;
        this.onClickCallback = onClick;
        this.onHoverEnterCallback = onHoverEnter;
        this.onHoverExitCallback = onHoverExit;

        if (optionButton) optionButton.onClick.AddListener(OnClick);
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