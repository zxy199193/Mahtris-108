// FileName: RewardOptionUI.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class RewardOptionUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI ����")]
    public Button optionButton;
    public Image optionIcon; // ������ʾ����/��Լ��ͼ��
    public Transform shapeContainer; // �������������ɷ�����״UIԤ�Ƽ�������
    public Text optionText; // ������ʾ���鱶�ʵ�

    private Action<RewardOptionUI> onClickCallback;
    private Action<string, string> onHoverEnterCallback;
    private Action onHoverExitCallback;

    private string title;
    private string description;

    // ����1�����ڳ�ʼ����ʾSpriteͼ��Ľ���������/��Լ��
    public void InitializeForSprite(Sprite icon, string title, string desc, Action<RewardOptionUI> onClick, Action<string, string> onHoverEnter, Action onHoverExit)
    {
        if (shapeContainer) shapeContainer.gameObject.SetActive(false);
        if (optionIcon)
        {
            optionIcon.gameObject.SetActive(true);
            optionIcon.sprite = icon;
        }
        if (optionText) optionText.text = ""; // ���ߺ���Լ����ʾ�����ı�

        SetupCallbacks(title, desc, onClick, onHoverEnter, onHoverExit);
    }

    // ����2�����ڳ�ʼ����ʾGameObjectԤ�Ƽ��Ľ��������飩
    public void InitializeForPrefab(GameObject uiPrefab, string text, string title, string desc, Action<RewardOptionUI> onClick, Action<string, string> onHoverEnter, Action onHoverExit)
    {
        if (optionIcon) optionIcon.gameObject.SetActive(false);
        if (shapeContainer)
        {
            shapeContainer.gameObject.SetActive(true);
            // ��վɵ���״
            foreach (Transform child in shapeContainer) Destroy(child.gameObject);
            // ʵ�����µ���״
            if (uiPrefab) Instantiate(uiPrefab, shapeContainer);
        }
        if (optionText) optionText.text = text; // ��ʾ���鱶��

        SetupCallbacks(title, desc, onClick, onHoverEnter, onHoverExit);
    }

    // ͳһ���ûص���ͨ����Ϣ
    private void SetupCallbacks(string title, string desc, Action<RewardOptionUI> onClick, Action<string, string> onHoverEnter, Action onHoverExit)
    {
        this.title = title;
        this.description = desc;
        this.onClickCallback = onClick;
        this.onHoverEnterCallback = onHoverEnter;
        this.onHoverExitCallback = onHoverExit;

        if (optionButton)
        {
            optionButton.onClick.RemoveAllListeners(); // ���Ƴ��ɵļ�����
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