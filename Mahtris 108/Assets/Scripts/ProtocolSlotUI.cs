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

        // 重置交互状态
        HideDeleteButton();
        deleteButton.onClick.RemoveAllListeners();

        if (protocol != null)
        {
            // === 状态 A：有条约 ===
            isEmpty = false;

            // 1. 显示内容层
            if (iconImage)
            {
                iconImage.gameObject.SetActive(true);
                iconImage.sprite = protocol.protocolIcon;
            }
            if (backplateImage) backplateImage.gameObject.SetActive(true);

            // 2. 隐藏空状态层
            if (emptyStateImage) emptyStateImage.gameObject.SetActive(false);

            // 3. 处理交互逻辑
            isPendingRemoval = GameManager.Instance.IsProtocolMarkedForRemoval(protocol);
            pendingOverlay.SetActive(isPendingRemoval);

            deleteButton.onClick.AddListener(OnDeleteClicked);
        }
        else
        {
            // === 状态 B：空槽位 ===
            SetEmpty();
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
        // 【新增】如果是空槽位，点击没有任何反应
        if (isEmpty) return;

        if (isPendingRemoval) return;

        FindObjectOfType<GameUIController>().OnProtocolSlotClicked(this);
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
        if (isEmpty) return; // 安全检查

        isPendingRemoval = true;
        pendingOverlay.SetActive(true);
        HideDeleteButton();

        GameManager.Instance.MarkProtocolForRemoval(currentProtocol);
    }

    // 用于 Tooltip 显示
    public ProtocolData GetProtocolData() => currentProtocol;
    public bool IsDeleteButtonActive() => deleteButton.gameObject.activeSelf;
    public bool IsEmpty() => isEmpty;
}