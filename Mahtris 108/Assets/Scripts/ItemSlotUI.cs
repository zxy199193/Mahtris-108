using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemSlotUI : MonoBehaviour, IPointerClickHandler
{
    [Header("UI 组件 - 有道具时显示")]
    [SerializeField] private Image iconImage;      // 道具图标
    [SerializeField] private Image backplateImage; // 道具背板

    [Header("UI 组件 - 空槽位时显示")]
    [SerializeField] private Image emptyStateImage; // 空状态图片

    [Header("UI 组件 - 通用")]
    [SerializeField] private Text shortcutText;

    private ItemData currentItem;
    private int slotIndex = -1; // 记录自己在背包中的索引 (0-4)
    private bool isEmpty = true;

    // 初始化方法
    public void Setup(ItemData item, int index)
    {
        currentItem = item;
        slotIndex = index;

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
            if (backplateImage) backplateImage.gameObject.SetActive(true);
            if (emptyStateImage) emptyStateImage.gameObject.SetActive(false);

            // 【核心修复】计算类型并传入
            TooltipTriggerUI.TooltipType type = item.isAdvanced
                ? TooltipTriggerUI.TooltipType.Advanced
                : TooltipTriggerUI.TooltipType.Common;

            // 传入 5 个参数，确保 type 被传递
            trigger.SetData(item.itemName, item.itemDescription, item.itemIcon, item.isLegendary, type);
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

        // 1. 隐藏内容层
        if (iconImage) iconImage.gameObject.SetActive(false);
        if (backplateImage) backplateImage.gameObject.SetActive(false);

        // 2. 显示空状态层
        if (emptyStateImage) emptyStateImage.gameObject.SetActive(true);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 如果是空的，点击无反应
        if (isEmpty) return;

        // 播放点击音效
        if (AudioManager.Instance) AudioManager.Instance.PlayButtonClickSound();

        // 调用库存管理器使用道具
        // FindObjectOfType 开销极小，因为只有点击时才调用一次
        InventoryManager inventory = FindObjectOfType<InventoryManager>();
        if (inventory != null && slotIndex != -1)
        {
            inventory.UseItem(slotIndex);
        }
    }

    // 用于 Tooltip 获取数据
    public ItemData GetItemData() => currentItem;
}