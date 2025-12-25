// FileName: InventoryManager.cs
using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public event Action<List<ItemData>> OnInventoryChanged;

    private List<ItemData> itemSlots;
    private int maxSlots;
    private GameManager gameManager;
    private bool isUsable = true;

    public void Initialize(GameSettings settings, GameManager manager)
    {
        this.maxSlots = settings.itemSlotCount;
        this.gameManager = manager;
        itemSlots = new List<ItemData>(new ItemData[maxSlots]);
    }

    public void ModifySlotCount(int amount)
    {
        maxSlots += amount;
        // 调整列表大小以匹配新的槽位数量
        while (itemSlots.Count < maxSlots)
        {
            itemSlots.Add(null);
        }
        while (itemSlots.Count > maxSlots && itemSlots.Count > 0)
        {
            itemSlots.RemoveAt(itemSlots.Count - 1);
        }
        OnInventoryChanged?.Invoke(new List<ItemData>(itemSlots));
    }

    public void SetUsable(bool usable)
    {
        isUsable = usable;
    }

    public bool AddItem(ItemData itemToAdd)
    {
        for (int i = 0; i < maxSlots; i++)
        {
            if (itemSlots[i] == null)
            {
                itemSlots[i] = itemToAdd;

                // 【新增】传奇物品统计 (No.43)
                if (itemToAdd.isLegendary && AchievementManager.Instance != null)
                {
                    AchievementManager.Instance.AddProgress(AchievementType.AccumulateLegendary, 1);
                }

                OnInventoryChanged?.Invoke(new List<ItemData>(itemSlots));
                return true;
            }
        }
        return false;
    }

    public void UseItem(int slotIndex)
    {
        // ... (前面的检查代码)

        bool success = itemSlots[slotIndex].Use(gameManager);

        if (success)
        {
            // ... (播放音效等代码)

            // 【新增】统计本局使用道具数 (No.24, No.27, No.41)
            if (GameManager.Instance != null)
            {
                GameManager.Instance.IncrementItemUsedCount(); // 下面会在GM里添加这个方法
            }
            // 统计累计使用
            if (AchievementManager.Instance != null)
            {
                AchievementManager.Instance.AddProgress(AchievementType.AccumulateItemUse, 1);
            }

            itemSlots[slotIndex] = null;
            OnInventoryChanged?.Invoke(new List<ItemData>(itemSlots));
        }
    }

    public void ClearInventory()
    {
        for (int i = 0; i < maxSlots; i++)
        {
            if (i >= itemSlots.Count) break;
            itemSlots[i] = null;
        }
        OnInventoryChanged?.Invoke(new List<ItemData>(itemSlots));
    }
    // 【新增】狂战士条约：自动使用所有道具
    public void UseAllItems()
    {
        for (int i = 0; i < maxSlots; i++)
        {
            if (i < itemSlots.Count && itemSlots[i] != null)
            {
                bool success = itemSlots[i].Use(GameManager.Instance);
                if (success)
                {
                    itemSlots[i] = null;
                }
            }
        }
        OnInventoryChanged?.Invoke(new List<ItemData>(itemSlots));
    }
    public bool IsFull()
    {
        // 你的背包逻辑是预先生成固定数量的槽位 (itemSlots)，没东西时存的是 null
        // 所以我们遍历所有槽位，只要发现有一个是 null，就说明没满
        foreach (var item in itemSlots)
        {
            if (item == null) return false;
        }

        // 如果循环结束都没找到 null，说明全满了
        return true;
    }
    public void RemoveItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= maxSlots) return;

        if (itemSlots[slotIndex] != null)
        {
            // 直接置空
            itemSlots[slotIndex] = null;

            // 刷新 UI
            OnInventoryChanged?.Invoke(new List<ItemData>(itemSlots));

            Debug.Log($"已移除槽位 {slotIndex} 的道具。");
        }
    }
}