// FileName: StoreSlotUI.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class StoreSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public enum SlotStatus { Unlocked, Locked, Hidden }

    [Header("UI 组件")]
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject lockedOverlay;
    [SerializeField] private Text priceText;
    [SerializeField] private Button buyButton;
    [SerializeField] private GameObject hiddenOverlay;
    [SerializeField] private Text unlockConditionText;

    private ItemData itemData;
    private ProtocolData protocolData;
    private bool isItem;
    private StorePanelController controller;

    public void SetupItem(ItemData data, SlotStatus status, StorePanelController ctrl)
    {
        itemData = data;
        isItem = true;
        controller = ctrl;
        iconImage.sprite = data.itemIcon;

        // 【修改】获取 "道具" 的多语言文本 (Key: ITEM_PROTOCOL_ITEM)
        string typeName = "道具";
        if (LocalizationManager.Instance)
        {
            typeName = LocalizationManager.Instance.GetText("ITEM_PROTOCOL_ITEM");
        }

        SetupStatus(status, data.price, data.isLegendary, data.unlockConditionCount, typeName);
    }

    public void SetupProtocol(ProtocolData data, SlotStatus status, StorePanelController ctrl)
    {
        protocolData = data;
        isItem = false;
        controller = ctrl;
        iconImage.sprite = data.protocolIcon;

        // 【修改】获取 "条约" 的多语言文本 (Key: ITEM_PROTOCOL_PROTOCOL)
        string typeName = "条约";
        if (LocalizationManager.Instance)
        {
            typeName = LocalizationManager.Instance.GetText("ITEM_PROTOCOL_PROTOCOL");
        }

        SetupStatus(status, data.price, data.isLegendary, data.unlockConditionCount, typeName);
    }

    private void SetupStatus(SlotStatus status, int price, bool isLegendary, int conditionCount, string typeName)
    {
        // 1. 初始化通用状态 (保持你的原始逻辑)
        if (lockedOverlay) lockedOverlay.SetActive(false);
        if (hiddenOverlay) hiddenOverlay.SetActive(false);

        if (buyButton != null) buyButton.gameObject.SetActive(false);
        if (buyButton) buyButton.onClick.RemoveAllListeners();

        switch (status)
        {
            case SlotStatus.Unlocked:
                // 你的逻辑：解锁状态只刷新字体（可能显示的是"已拥有"或者不显示价格）
                if (priceText && LocalizationManager.Instance)
                    LocalizationManager.Instance.UpdateFont(priceText);
                break;

            case SlotStatus.Locked:
                // 你的逻辑：锁定状态显示遮罩和购买按钮
                if (lockedOverlay) lockedOverlay.SetActive(true);
                if (buyButton != null) buyButton.gameObject.SetActive(true);

                if (priceText)
                {
                    priceText.text = price.ToString();
                    // 【新增】刷新字体
                    if (LocalizationManager.Instance) LocalizationManager.Instance.UpdateFont(priceText);
                }

                // 重新绑定购买事件
                if (buyButton) buyButton.onClick.AddListener(() => controller.TryBuy(this));
                break;

            case SlotStatus.Hidden:
                if (hiddenOverlay) hiddenOverlay.SetActive(true);

                if (unlockConditionText)
                {
                    // 【修改】使用多语言格式化
                    // 原始: $"解锁 {conditionCount} 个{typeName}后显示";
                    string format = "解锁 {0} 个{1}后显示";
                    if (LocalizationManager.Instance)
                    {
                        format = LocalizationManager.Instance.GetText("ITEM_PROTOCOL_UNLOCK_CONDITION");
                    }

                    unlockConditionText.text = string.Format(format, conditionCount, typeName);

                    // 【新增】刷新字体
                    if (LocalizationManager.Instance) LocalizationManager.Instance.UpdateFont(unlockConditionText);
                }
                break;
        }
    }

    // 【核心修复】修改 OnPointerEnter 方法
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hiddenOverlay.activeSelf) return;

        if (TooltipController.Instance != null)
        {

            string title = isItem ? itemData.GetName() : protocolData.GetName();
            string desc = isItem ? itemData.GetDescription() : protocolData.GetDescription();
            Sprite icon = isItem ? itemData.itemIcon : protocolData.protocolIcon;
            bool legendary = isItem ? itemData.isLegendary : protocolData.isLegendary;

            // 获取 Settings
            GameSettings settings = null;
            if (controller != null) settings = controller.GetSettings();
            if (settings == null && GameManager.Instance != null) settings = GameManager.Instance.GetSettings();
            if (settings == null) return;

            Sprite bg = null;

            // 【核心修复】必须在这里准确计算出 type，既用于选背景，也用于传参数
            TooltipTriggerUI.TooltipType type = TooltipTriggerUI.TooltipType.Common;

            if (isItem)
            {
                if (itemData.isAdvanced)
                {
                    type = TooltipTriggerUI.TooltipType.Advanced; // 标记为高级
                    bg = settings.tooltipBgAdvanced;
                }
                else
                {
                    type = TooltipTriggerUI.TooltipType.Common; // 标记为普通
                    bg = settings.tooltipBgCommon;
                }
            }
            else // 是条约
            {
                type = TooltipTriggerUI.TooltipType.Protocol; // 标记为条约
                bg = settings.tooltipBgProtocol;
            }

            // 保底检查
            if (bg == null) bg = settings.tooltipBgCommon;
            // 传奇覆盖背景，但【不要】改变 type (保持类型标签显示 "高级道具" 或 "条约")
            if (legendary) bg = settings.tooltipBgLegendary;

            // 调用 Show，传入计算好的 type
            TooltipController.Instance.Show(title, desc, icon, bg, legendary, type, transform);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipController.Instance != null) TooltipController.Instance.Hide();
    }

    public int GetPrice() => isItem ? itemData.price : protocolData.price;
    public string GetName() => isItem ? itemData.itemName : protocolData.protocolName;
    public bool IsItem() => isItem;
}