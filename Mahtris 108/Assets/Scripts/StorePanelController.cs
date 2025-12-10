using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using DG.Tweening;

public class StorePanelController : MonoBehaviour
{
    [Header("页签")]
    [SerializeField] private Button tabItemsButton;
    [SerializeField] private Button tabProtocolsButton;
    [SerializeField] private Text tabItemsCountText;
    [SerializeField] private Text tabProtocolsCountText;
    [SerializeField] private Color activeTabColor = Color.white;
    [SerializeField] private Color inactiveTabColor = Color.gray;

    [Header("列表容器")]
    [SerializeField] private Transform gridContent;
    [SerializeField] private GameObject slotPrefab;

    [Header("提示信息")]
    [SerializeField] private Text toastText; // 用于显示"金币不足"
    [SerializeField] private CanvasGroup toastCanvasGroup;

    [Header("通用")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Text currentGoldText;

    [Header("核心配置 (必须拖入)")]
    [SerializeField] private GameSettings settings;
    private bool showingItems = true; // 当前是否显示道具页签

    void Start()
    {
        if (settings == null)
        {
            Debug.LogError("严重错误：StorePanelController 没有配置 GameSettings！请在 Inspector 中拖入文件。");
            return;
        }

        tabItemsButton.onClick.AddListener(ShowItemsTab);
        tabProtocolsButton.onClick.AddListener(ShowProtocolsTab);
        closeButton.onClick.AddListener(CloseStore);

        // 初始化隐藏Toast
        if (toastCanvasGroup) toastCanvasGroup.alpha = 0;
    }
    public GameSettings GetSettings()
    {
        return settings;
    }

    public void OpenStore()
    {
        gameObject.SetActive(true);

        // 动画：从下往上弹出
        RectTransform rt = GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(0, -1200);
        rt.DOLocalMove(Vector2.zero, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);

        UpdateGoldDisplay();
        ShowItemsTab(); // 默认打开道具页
    }

    public void CloseStore()
    {
        // 动画：往下掉
        RectTransform rt = GetComponent<RectTransform>();
        rt.DOLocalMove(new Vector2(0, -1200), 0.3f).SetEase(Ease.InBack).SetUpdate(true).OnComplete(() =>
        {
            gameObject.SetActive(false);
        });

        if (AudioManager.Instance) AudioManager.Instance.PlayButtonClickSound();
    }

    private void UpdateGoldDisplay()
    {
        if (currentGoldText && GameSession.Instance)
        {
            currentGoldText.text = GameSession.Instance.CurrentGold.ToString();
        }
    }

    private void ShowItemsTab()
    {
        showingItems = true;
        UpdateButtonStates();
        RefreshGrid();
        if (AudioManager.Instance) AudioManager.Instance.PlayButtonClickSound();
    }

    private void ShowProtocolsTab()
    {
        showingItems = false;
        UpdateButtonStates();
        RefreshGrid();
        if (AudioManager.Instance) AudioManager.Instance.PlayButtonClickSound();
    }

    private void UpdateButtonStates()
    {
        tabItemsButton.image.color = showingItems ? activeTabColor : inactiveTabColor;
        tabProtocolsButton.image.color = !showingItems ? activeTabColor : inactiveTabColor;

        // 计算进度
        int unlockedItems = GetAllItems().Count(i => SaveManager.IsItemUnlocked(i.itemName, i.isInitial));
        int totalItems = GetAllItems().Count;
        tabItemsCountText.text = $"{unlockedItems}/{totalItems}";

        int unlockedProtocols = settings.protocolPool.Count(p => SaveManager.IsProtocolUnlocked(p.protocolName, p.isInitial));
        int totalProtocols = settings.protocolPool.Count;
        tabProtocolsCountText.text = $"{unlockedProtocols}/{totalProtocols}";
    }

    // 获取所有道具 (合并普通和高级)
    private List<ItemData> GetAllItems()
    {
        var list = new List<ItemData>();
        list.AddRange(settings.commonItemPool);
        list.AddRange(settings.advancedItemPool);
        return list;
    }

    private void RefreshGrid()
    {
        // 清空列表
        foreach (Transform child in gridContent) Destroy(child.gameObject);

        if (showingItems)
        {
            // 【修改】获取所有道具，并按价格从低到高排序
            // 如果价格相同，再按名称排序以保持列表稳定
            var allItems = GetAllItems()
                .OrderBy(i => i.price)
                .ThenBy(i => i.itemName) // 可选：价格一样时按名字排
                .ToList();

            int unlockedCount = allItems.Count(i => SaveManager.IsItemUnlocked(i.itemName, i.isInitial));

            foreach (var item in allItems)
            {
                var slotGO = Instantiate(slotPrefab, gridContent);
                var slotUI = slotGO.GetComponent<StoreSlotUI>();

                StoreSlotUI.SlotStatus status = StoreSlotUI.SlotStatus.Locked;

                if (SaveManager.IsItemUnlocked(item.itemName, item.isInitial))
                {
                    status = StoreSlotUI.SlotStatus.Unlocked;
                }
                else if (item.isLegendary && unlockedCount < item.unlockConditionCount)
                {
                    status = StoreSlotUI.SlotStatus.Hidden;
                }

                slotUI.SetupItem(item, status, this);
            }
        }
        else
        {
            // 【修改】获取所有条约，并按价格从低到高排序
            var allProtocols = settings.protocolPool
                .OrderBy(p => p.price)
                .ThenBy(p => p.protocolName) // 可选
                .ToList();

            int unlockedCount = allProtocols.Count(p => SaveManager.IsProtocolUnlocked(p.protocolName, p.isInitial));

            foreach (var proto in allProtocols)
            {
                var slotGO = Instantiate(slotPrefab, gridContent);
                var slotUI = slotGO.GetComponent<StoreSlotUI>();

                StoreSlotUI.SlotStatus status = StoreSlotUI.SlotStatus.Locked;

                if (SaveManager.IsProtocolUnlocked(proto.protocolName, proto.isInitial))
                {
                    status = StoreSlotUI.SlotStatus.Unlocked;
                }
                else if (proto.isLegendary && unlockedCount < proto.unlockConditionCount)
                {
                    status = StoreSlotUI.SlotStatus.Hidden;
                }

                slotUI.SetupProtocol(proto, status, this);
            }
        }
    }

    public void TryBuy(StoreSlotUI slot)
    {
        int price = slot.GetPrice();
        if (GameSession.Instance.CurrentGold >= price)
        {
            // 购买成功
            GameSession.Instance.AddGold(-price); // 扣钱
            UpdateGoldDisplay();

            if (slot.IsItem())
            {
                SaveManager.UnlockItem(slot.GetName());
            }
            else
            {
                SaveManager.UnlockProtocol(slot.GetName());
            }

            if (AudioManager.Instance) AudioManager.Instance.PlayBuySuccessSound();

            // 刷新列表以更新状态
            UpdateButtonStates();
            RefreshGrid();
        }
        else
        {
            // 余额不足
            if (AudioManager.Instance) AudioManager.Instance.PlayBuyFailSound();
            ShowToast("金币不足！");
        }
    }

    private void ShowToast(string message)
    {
        if (toastText == null || toastCanvasGroup == null) return;

        toastCanvasGroup.DOKill();
        toastText.text = message;
        toastCanvasGroup.alpha = 1;

        // 显示 0.8s 后，用 0.4s 淡出
        Sequence seq = DOTween.Sequence();
        seq.AppendInterval(0.8f);
        seq.Append(toastCanvasGroup.DOFade(0, 0.4f));
        seq.SetUpdate(true); // 确保在暂停时也能运行（如果商店在主菜单，这里无所谓；如果在游戏中则重要）
    }
}