using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class ItemSlotUI : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Header("UI 组件 - 有道具时显示")]
    [SerializeField] private Image iconImage;      // 道具图标
    [SerializeField] private Image backplateImage; // 道具背板

    [Header("UI 组件 - 空槽位时显示")]
    [SerializeField] private Image emptyStateImage; // 空状态图片

    [Header("UI 组件 - 通用")]
    [SerializeField] private Text shortcutText;
    [SerializeField] private Button deleteButton;
    [SerializeField] private GameObject pendingOverlay;

    private ItemData currentItem;
    private int slotIndex = -1; // 记录自己在背包中的索引 (0-4)
    private bool isEmpty = true;
    private Vector3 originalScale;

    void Awake()
    {
        originalScale = transform.localScale;
    }

    public void Setup(ItemData item, int index)
    {
        currentItem = item;
        slotIndex = index;
        HideDeleteButton();
        if (shortcutText != null)
        {
            shortcutText.text = (index + 1).ToString();
            shortcutText.gameObject.SetActive(true);
        }

        // 获取 Trigger
        TooltipTriggerUI trigger = GetComponent<TooltipTriggerUI>();
        if (trigger == null) trigger = gameObject.AddComponent<TooltipTriggerUI>();

        if (item != null)
        {
            // === 有道具 ===
            isEmpty = false;
            if (iconImage) { iconImage.gameObject.SetActive(true); iconImage.sprite = item.itemIcon; }
            if (backplateImage)
            {
                backplateImage.gameObject.SetActive(true);
                if (GameManager.Instance != null)
                {
                    GameSettings settings = GameManager.Instance.GetSettings();
                    if (settings != null)
                    {
                        Sprite bg = item.isAdvanced ? settings.tooltipBgAdvanced : settings.tooltipBgCommon;
                        if (item.isLegendary) bg = settings.tooltipBgLegendary;
                        backplateImage.sprite = bg;
                    }
                }
            }
            if (emptyStateImage) emptyStateImage.gameObject.SetActive(false);
            if (deleteButton) deleteButton.onClick.AddListener(OnDeleteClicked);
            // 【核心修复】计算类型并传入
            TooltipTriggerUI.TooltipType type = item.isAdvanced
                ? TooltipTriggerUI.TooltipType.Advanced
                : TooltipTriggerUI.TooltipType.Common;

            // 传入 5 个参数，确保 type 被传递
            trigger.SetData(item.nameKey, item.descKey, item.itemIcon, item.isLegendary, type);
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
        currentItem = null;
        HideDeleteButton();
        // 1. 隐藏内容层
        if (iconImage) iconImage.gameObject.SetActive(false);
        if (backplateImage) backplateImage.gameObject.SetActive(false);

        // 2. 显示空状态层
        if (emptyStateImage) emptyStateImage.gameObject.SetActive(true);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isEmpty) return;

        // 1. 获取 UI 控制器
        var uiController = FindObjectOfType<GameUIController>();

        // 2. 判断当前状态
        if (uiController != null && uiController.IsHuPopupActive())
        {
            // === 结算状态：显示/隐藏删除按钮 ===
            uiController.OnItemSlotClicked(this);
        }
        else
        {
            AttemptUseItem();
        }
    }
    public void AttemptUseItem()
    {
        if (isEmpty) return;

        // 1. 检查禁止使用道具的状态
        if (GameManager.Instance != null && GameManager.Instance.isFrenziedActive)
        {
            if (AudioManager.Instance) AudioManager.Instance.PlayBuyFailSound();
            string msg = LocalizationManager.Instance != null ? LocalizationManager.Instance.GetText("ITEM_TIPS_KARATE") : "空手道大师生效中：无法使用道具！";
            var ui = FindObjectOfType<GameUIController>();
            if (ui) ui.ShowToast(msg);
            return;
        }

        // 2. 播放点击音效
        if (AudioManager.Instance) AudioManager.Instance.PlayButtonClickSound();

        // 3. 执行逻辑 (InventoryManager 内部会清空该槽位的数据，导致图标立即消失)
        InventoryManager inventory = FindObjectOfType<InventoryManager>();
        if (inventory != null && slotIndex != -1)
        {
            inventory.UseItem(slotIndex);
        }
    }

    public void ShowDeleteButton()
    {
        if (!isEmpty)
        {
            if (deleteButton) deleteButton.gameObject.SetActive(true);
            if (pendingOverlay) pendingOverlay.SetActive(true);
        }
    }

    public void HideDeleteButton()
    {
        if (deleteButton) deleteButton.gameObject.SetActive(false);
        if (pendingOverlay) pendingOverlay.SetActive(false);
    }
    public bool IsDeleteButtonActive()
    {
        return deleteButton != null && deleteButton.gameObject.activeSelf;
    }

    private void OnDeleteClicked()
    {
        if (isEmpty) return;

        // 1. 隐藏按钮
        HideDeleteButton();

        // 2. 调用 Inventory 删除数据
        InventoryManager inventory = FindObjectOfType<InventoryManager>();
        if (inventory != null && slotIndex != -1)
        {
            inventory.RemoveItem(slotIndex);
        }
    }
    public ItemData GetItemData() => currentItem;
    public void OnPointerDown(PointerEventData eventData)
    {
        if (isEmpty) return;

        transform.DOKill();
        // 按下时缩小到 0.95 倍
        transform.DOScale(originalScale * 0.95f, 0.1f).SetUpdate(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isEmpty) return;

        transform.DOKill();
        // 松开时恢复原状
        transform.DOScale(originalScale, 0.1f).SetUpdate(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isEmpty) return;

        // 如果鼠标按住并移出了道具栏范围，取消缩放状态
        transform.DOKill();
        transform.DOScale(originalScale, 0.1f).SetUpdate(true);
    }
}