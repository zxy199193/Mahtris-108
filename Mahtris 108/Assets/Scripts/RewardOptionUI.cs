// FileName: RewardOptionUI.cs
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RewardOptionUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI 引用")]
    public Button optionButton;
    public Image optionIcon;
    public Transform shapeContainer;
    public GameObject checkMark;
    public GameObject legendaryBadge;
    public Image backgroundImage;

    [Header("文本显示")]
    public GameObject textContainer;
    public Text optionText;

    // 数据引用
    private ItemData _itemData;
    private ProtocolData _protocolData;
    private bool _isBlock;

    private Action<RewardOptionUI> _onClick;

    // --- 初始化方法 1: 道具 ---
    public void Setup(ItemData item, Action<RewardOptionUI> onClick)
    {
        _itemData = item;
        _isBlock = false;
        SetupCommon(item.itemIcon, onClick);
        if (legendaryBadge) legendaryBadge.SetActive(item.isLegendary);
    }

    // --- 初始化方法 2: 条约 ---
    public void Setup(ProtocolData protocol, Action<RewardOptionUI> onClick)
    {
        _protocolData = protocol;
        _isBlock = false;
        SetupCommon(protocol.protocolIcon, onClick);
        if (legendaryBadge) legendaryBadge.SetActive(protocol.isLegendary);
    }

    // --- 初始化方法 3: 方块 ---
    public void Setup(GameObject prefab, Action<RewardOptionUI> onClick)
    {
        _isBlock = true;
        if (optionIcon) optionIcon.gameObject.SetActive(false);
        if (shapeContainer)
        {
            shapeContainer.gameObject.SetActive(true);
            foreach (Transform child in shapeContainer) Destroy(child.gameObject);
            var tet = prefab.GetComponent<Tetromino>();
            if (tet && tet.uiPrefab) Instantiate(tet.uiPrefab, shapeContainer);
        }
        if (textContainer) textContainer.SetActive(true);
        if (optionText)
        {
            var tet = prefab.GetComponent<Tetromino>();
            if (tet) optionText.text = $"{tet.extraMultiplier:F0}";
        }
        if (legendaryBadge) legendaryBadge.SetActive(false);
        _onClick = onClick;
        if (optionButton)
        {
            optionButton.onClick.RemoveAllListeners();
            optionButton.onClick.AddListener(() => _onClick?.Invoke(this));
        }
        SetSelected(false);
    }

    private void SetupCommon(Sprite icon, Action<RewardOptionUI> onClick)
    {
        if (shapeContainer) shapeContainer.gameObject.SetActive(false);
        if (textContainer) textContainer.SetActive(false);
        if (optionIcon)
        {
            optionIcon.gameObject.SetActive(true);
            optionIcon.sprite = icon;
        }
        _onClick = onClick;
        if (optionButton)
        {
            optionButton.onClick.RemoveAllListeners();
            optionButton.onClick.AddListener(() => _onClick?.Invoke(this));
        }
        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        if (checkMark) checkMark.SetActive(selected);
    }

    public void SetInteractable(bool active)
    {
        if (optionButton) optionButton.interactable = active;
    }

    // --- 【核心修复】鼠标悬停逻辑 ---
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_isBlock) return;
        if (TooltipController.Instance == null) return;

        string title = "";
        string desc = "";
        Sprite icon = null;
        bool legendary = false;
        Sprite bg = null;

        // 【新增】定义类型变量，默认为 Common
        TooltipTriggerUI.TooltipType type = TooltipTriggerUI.TooltipType.Common;

        GameSettings settings = GameManager.Instance.GetSettings();

        if (_itemData != null)
        {
            title = _itemData.nameKey;
            desc = _itemData.descKey;
            icon = _itemData.itemIcon;
            legendary = _itemData.isLegendary;

            // 【新增】判断道具类型 (普通/高级)
            if (_itemData.isAdvanced)
            {
                type = TooltipTriggerUI.TooltipType.Advanced;
                bg = settings.tooltipBgAdvanced;
            }
            else
            {
                type = TooltipTriggerUI.TooltipType.Common;
                bg = settings.tooltipBgCommon;
            }
        }
        else if (_protocolData != null)
        {
            title = _protocolData.nameKey;
            desc = _protocolData.descKey;
            icon = _protocolData.protocolIcon;
            legendary = _protocolData.isLegendary;

            // 【新增】设定为条约类型
            type = TooltipTriggerUI.TooltipType.Protocol;
            bg = settings.tooltipBgProtocol;
        }

        // 传奇背景覆盖
        if (legendary) bg = settings.tooltipBgLegendary;
        // 保底背景
        if (bg == null) bg = settings.tooltipBgCommon;

        // 【修复】传入 type 参数 (第 6 个参数)
        TooltipController.Instance.Show(title, desc, icon, bg, legendary, type, this.transform);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipController.Instance != null) TooltipController.Instance.Hide();
    }
}