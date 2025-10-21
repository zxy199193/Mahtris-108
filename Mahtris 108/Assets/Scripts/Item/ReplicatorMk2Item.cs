// FileName: ReplicatorMk2Item.cs
using UnityEngine;

[CreateAssetMenu(fileName = "ReplicatorMk2", menuName = "Items/Advanced/ReplicatorMk2")]
public class ReplicatorMk2Item : ItemData
{
    public override bool Use(GameManager gameManager)
    {
        ItemData lastItem = gameManager.GetLastUsedItem();

        // ȷ����һ�����ߴ��ڣ��Ҳ����Լ�����ֹ���޸���
        if (lastItem != null && lastItem.itemName != this.itemName)
        {
            // InventoryManager��˽�б�������Ҫͨ��GameManager�Ĺ����ӿڷ���
            gameManager.Inventory.AddItem(lastItem);
            return true;
        }
        return false;
    }
}