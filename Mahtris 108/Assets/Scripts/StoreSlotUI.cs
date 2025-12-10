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
        SetupStatus(status, data.price, data.isLegendary, data.unlockConditionCount, "道具");
    }

    public void SetupProtocol(ProtocolData data, SlotStatus status, StorePanelController ctrl)
    {
        protocolData = data;
        isItem = false;
        controller = ctrl;
        iconImage.sprite = data.protocolIcon;
        SetupStatus(status, data.price, data.isLegendary, data.unlockConditionCount, "条约");
    }

    private void SetupStatus(SlotStatus status, int price, bool isLegendary, int conditionCount, string typeName)
    {
        // 1. 先把所有状态相关的 UI 都隐藏/重置
        lockedOverlay.SetActive(false);
        hiddenOverlay.SetActive(false);

        // 【新增】显式隐藏购买按钮 (防止它不在 lockedOverlay 里时无法消失)
        if (buyButton != null) buyButton.gameObject.SetActive(false);

        buyButton.onClick.RemoveAllListeners();

        switch (status)
        {
            case SlotStatus.Unlocked:
                // 已解锁：什么都不用显示，保持上面的隐藏状态即可
                break;

            case SlotStatus.Locked:
                lockedOverlay.SetActive(true);

                // 【新增】锁定状态：显式打开购买按钮
                if (buyButton != null) buyButton.gameObject.SetActive(true);

                priceText.text = price.ToString();
                buyButton.onClick.AddListener(() => controller.TryBuy(this));
                break;

            case SlotStatus.Hidden:
                hiddenOverlay.SetActive(true);
                unlockConditionText.text = $"解锁 {conditionCount} 个{typeName}后显示";
                break;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 隐藏状态下不显示详情浮窗 (或显示特殊的)
        if (hiddenOverlay.activeSelf) return;

        if (TooltipController.Instance != null)
        {
            string title = isItem ? itemData.itemName : protocolData.protocolName;
            string desc = isItem ? itemData.itemDescription : protocolData.protocolDescription;
            Sprite icon = isItem ? itemData.itemIcon : protocolData.protocolIcon;
            bool legendary = isItem ? itemData.isLegendary : protocolData.isLegendary;

            GameSettings settings = null;
            if (controller != null)
            {
                settings = controller.GetSettings();
            }

            // 安全检查，防止没拿到 settings 导致后面报错
            if (settings != null)
            {
                Sprite bg = legendary ? settings.tooltipBgLegendary : (isItem ? settings.tooltipBgCommon : settings.tooltipBgProtocol);
                TooltipController.Instance.Show(title, desc, icon, bg, legendary, transform);
            }
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