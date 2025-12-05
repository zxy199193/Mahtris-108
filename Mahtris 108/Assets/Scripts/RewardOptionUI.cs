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
    public Transform shapeContainer;
    public GameObject checkMark; // 【新增】勾选标记
    public GameObject legendaryBadge; // 【新增】传奇角标
    public Image backgroundImage; // 【新增】用于切换背板 (需在Prefab中拖拽)

    [Header("文本显示")]
    public GameObject textContainer;
    public Text optionText;

    // 数据引用
    private ItemData _itemData;
    private ProtocolData _protocolData;
    private bool _isBlock;

    // 回调
    private Action<RewardOptionUI> _onClick;

    // --- 初始化方法 1: 道具 ---
    public void Setup(ItemData item, Action<RewardOptionUI> onClick)
    {
        _itemData = item;
        _isBlock = false;
        SetupCommon(item.itemIcon, onClick);

        // 传奇显示
        if (legendaryBadge) legendaryBadge.SetActive(item.isLegendary);
    }

    // --- 初始化方法 2: 条约 ---
    public void Setup(ProtocolData protocol, Action<RewardOptionUI> onClick)
    {
        _protocolData = protocol;
        _isBlock = false;
        SetupCommon(protocol.protocolIcon, onClick);

        // 传奇显示
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

        if (legendaryBadge) legendaryBadge.SetActive(false); // 方块无传奇

        // 绑定点击
        _onClick = onClick;
        if (optionButton)
        {
            optionButton.onClick.RemoveAllListeners();
            optionButton.onClick.AddListener(() => _onClick?.Invoke(this));
        }
        SetSelected(false);
    }

    // 私有通用设置
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

    // 交互状态控制
    public void SetSelected(bool selected)
    {
        if (checkMark) checkMark.SetActive(selected);
        // 如果选中了，按钮不再可交互（防止重复点击），但也可能通过 SetInteractable 统一控制
    }

    public void SetInteractable(bool active)
    {
        if (optionButton) optionButton.interactable = active;
    }

    // --- 鼠标悬停逻辑 (调用 TooltipController) ---
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 方块不显示浮窗 (根据需求)
        if (_isBlock) return;

        if (TooltipController.Instance == null) return;

        string title = "";
        string desc = "";
        Sprite icon = null;
        bool legendary = false;
        Sprite bg = null;
        GameSettings settings = GameManager.Instance.GetSettings();

        if (_itemData != null)
        {
            title = _itemData.itemName;
            desc = _itemData.itemDescription;
            icon = _itemData.itemIcon;
            legendary = _itemData.isLegendary;
            // 简单判断背板
            bg = legendary ? settings.tooltipBgLegendary : settings.tooltipBgCommon;
        }
        else if (_protocolData != null)
        {
            title = _protocolData.protocolName;
            desc = _protocolData.protocolDescription;
            icon = _protocolData.protocolIcon;
            legendary = _protocolData.isLegendary;
            bg = legendary ? settings.tooltipBgLegendary : settings.tooltipBgProtocol;
        }

        // 显示浮窗
        TooltipController.Instance.Show(title, desc, icon, bg, legendary, this.transform);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipController.Instance != null) TooltipController.Instance.Hide();
    }
}