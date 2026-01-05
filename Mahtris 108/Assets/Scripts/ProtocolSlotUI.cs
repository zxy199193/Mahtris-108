using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ProtocolSlotUI : MonoBehaviour, IPointerClickHandler
{
    [Header("UI 组件")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Image backplateImage;
    [SerializeField] private Button deleteButton; // 删除按钮
    [SerializeField] private GameObject pendingOverlay; // "准备删除"的遮罩(含文字图片)


    [SerializeField] private Image emptyStateImage;
    private ProtocolData currentProtocol;
    private bool isPendingRemoval = false;
    private bool isEmpty = true;

    // 初始化
    public void Setup(ProtocolData protocol)
    {
        currentProtocol = protocol;
        HideDeleteButton();
        if (deleteButton) deleteButton.onClick.RemoveAllListeners();

        TooltipTriggerUI trigger = GetComponent<TooltipTriggerUI>();
        if (trigger == null) trigger = gameObject.AddComponent<TooltipTriggerUI>();

        if (protocol != null)
        {
            isEmpty = false;
            if (iconImage) { iconImage.gameObject.SetActive(true); iconImage.sprite = protocol.protocolIcon; }
            if (backplateImage) backplateImage.gameObject.SetActive(true);
            if (emptyStateImage) emptyStateImage.gameObject.SetActive(false);

            isPendingRemoval = GameManager.Instance.IsProtocolMarkedForRemoval(protocol);
            if (pendingOverlay) pendingOverlay.SetActive(isPendingRemoval);
            if (deleteButton) deleteButton.onClick.AddListener(OnDeleteClicked);

            // 【核心修复】强制传入 Protocol 类型
            trigger.SetData(protocol.nameKey, protocol.descKey, protocol.protocolIcon, protocol.isLegendary, TooltipTriggerUI.TooltipType.Protocol);
        }
        else
        {
            SetEmpty();
            trigger.SetData(null, null);
        }
    }
    private void SetEmpty()
    {
        isEmpty = true;
        currentProtocol = null;
        isPendingRemoval = false;
        pendingOverlay.SetActive(false);

        // 1. 隐藏内容层
        if (iconImage) iconImage.gameObject.SetActive(false);
        if (backplateImage) backplateImage.gameObject.SetActive(false);

        // 2. 显示空状态层
        if (emptyStateImage)
        {
            emptyStateImage.gameObject.SetActive(true);
        }
    }
    // (1) 点击图标
    public void OnPointerClick(PointerEventData eventData)
    {
        if (isEmpty) return;

        // 1. 获取 UI 控制器
        var uiController = FindObjectOfType<GameUIController>();

        // 2. 【核心判断】只有在“胡牌弹窗”激活时，点击才有效
        if (uiController != null && uiController.IsHuPopupActive())
        {
            // 通知 UI 显示删除按钮 (会调用 ShowDeleteButton)
            uiController.OnProtocolSlotClicked(this);
        }
        else
        {
            // 游戏进行中点击无反应
            // 可选：播放个“不可操作”的音效，或者什么都不做
        }
    }

    // 显示删除按钮
    public void ShowDeleteButton()
    {
        if (!isEmpty && !isPendingRemoval) // 只有非空且未待删时才能显示
        {
            deleteButton.gameObject.SetActive(true);
        }
    }

    // 隐藏删除按钮
    public void HideDeleteButton()
    {
        if (deleteButton != null)
            deleteButton.gameObject.SetActive(false);
    }

    // (2) 点击删除按钮
    private void OnDeleteClicked()
    {
        if (isEmpty) return;

        // 1. 隐藏删除按钮
        HideDeleteButton();

        // 2. 【核心修改】不再标记，而是直接调用 GM 立即删除
        // 旧逻辑: GameManager.Instance.MarkProtocolForRemoval(currentProtocol);

        GameManager.Instance.RemoveProtocolImmediately(currentProtocol);

        // 注意：RemoveProtocolImmediately 会触发 gameUI.UpdateProtocolUI
        // 这会导致所有槽位被重新 Setup，视觉上该条约会瞬间消失，后面的条约会补位
    }

    // 用于 Tooltip 显示
    public ProtocolData GetProtocolData() => currentProtocol;
    public bool IsDeleteButtonActive() => deleteButton.gameObject.activeSelf;
    public bool IsEmpty() => isEmpty;
}