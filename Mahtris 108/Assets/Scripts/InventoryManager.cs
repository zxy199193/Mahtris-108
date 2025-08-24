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

    public void Initialize(GameSettings settings, GameManager manager)
    {
        this.maxSlots = settings.itemSlotCount;
        this.gameManager = manager;
        itemSlots = new List<ItemData>(new ItemData[maxSlots]);
    }

    public bool AddItem(ItemData itemToAdd)
    {
        for (int i = 0; i < maxSlots; i++)
        {
            if (itemSlots[i] == null)
            {
                itemSlots[i] = itemToAdd;
                OnInventoryChanged?.Invoke(new List<ItemData>(itemSlots));
                return true;
            }
        }
        return false;
    }

    public void UseItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= maxSlots || itemSlots[slotIndex] == null) return;

        bool success = itemSlots[slotIndex].Use(gameManager);
        if (success)
        {
            itemSlots[slotIndex] = null;
            OnInventoryChanged?.Invoke(new List<ItemData>(itemSlots));
        }
    }

    public void ClearInventory()
    {
        for (int i = 0; i < maxSlots; i++)
        {
            itemSlots[i] = null;
        }
        OnInventoryChanged?.Invoke(new List<ItemData>(itemSlots));
    }
}