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

            // 选项：如果你希望没有道具时也显示数字（推荐，方便记忆键位），保持 SetActive(true)
            shortcutText.gameObject.SetActive(true);

            // 选项：如果你希望只有在有道具时才显示数字，可以使用下面这行：
            // shortcutText.gameObject.SetActive(item != null);
        }
        if (item != null)
        {
            // === 有道具状态 ===
            isEmpty = false;

            // 1. 显示内容层
            if (iconImage)
            {
                iconImage.gameObject.SetActive(true);
                iconImage.sprite = item.itemIcon;
            }
            if (backplateImage) backplateImage.gameObject.SetActive(true);

            // 2. 隐藏空状态层
            if (emptyStateImage) emptyStateImage.gameObject.SetActive(false);
        }
        else
        {
            // === 空槽位状态 ===
            SetEmpty();
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