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
                OnInventoryChanged?.Invoke(new List<ItemData>(itemSlots));
                return true; // 添加成功
            }
        }
        return false; // 道具栏已满
    }

    public void UseItem(int slotIndex)
    {
        if (!isUsable)
        {
            Debug.Log("狂牌士条约生效中，无法使用道具！");
            return;
        }

        if (slotIndex < 0 || slotIndex >= maxSlots || itemSlots[slotIndex] == null) return;

        // 使用道具并检查是否成功
        bool success = itemSlots[slotIndex].Use(gameManager);

        // 只有成功使用的道具才会被消耗
        if (success)
        {
            itemSlots[slotIndex] = null; // 消耗道具
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
}