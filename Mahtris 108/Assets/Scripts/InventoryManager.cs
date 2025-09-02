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
        // �����б��С��ƥ���µĲ�λ����
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
                return true; // ��ӳɹ�
            }
        }
        return false; // ����������
    }

    public void UseItem(int slotIndex)
    {
        if (!isUsable)
        {
            Debug.Log("����ʿ��Լ��Ч�У��޷�ʹ�õ��ߣ�");
            return;
        }

        if (slotIndex < 0 || slotIndex >= maxSlots || itemSlots[slotIndex] == null) return;

        // ʹ�õ��߲�����Ƿ�ɹ�
        bool success = itemSlots[slotIndex].Use(gameManager);

        // ֻ�гɹ�ʹ�õĵ��߲Żᱻ����
        if (success)
        {
            itemSlots[slotIndex] = null; // ���ĵ���
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