// FileName: ReplicatorMk2Item.cs
using UnityEngine;

[CreateAssetMenu(fileName = "ReplicatorMk2", menuName = "Items/Advanced/ReplicatorMk2")]
public class ReplicatorMk2Item : ItemData
{
    public override bool Use(GameManager gameManager)
    {
        ItemData lastItem = gameManager.GetLastUsedItem();

        // 确保上一个道具存在，且不是自己，防止无限复制
        if (lastItem != null && lastItem.itemName != this.itemName)
        {
            // InventoryManager是私有变量，需要通过GameManager的公共接口访问
            gameManager.Inventory.AddItem(lastItem);
            return true;
        }
        return false;
    }
}