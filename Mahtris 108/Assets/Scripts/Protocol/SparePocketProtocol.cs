// FileName: SparePocketProtocol.cs
using UnityEngine;

[CreateAssetMenu(fileName = "SparePocketProtocol", menuName = "Protocols/SparePocket")]
public class SparePocketProtocol : ProtocolData
{
    public int extraSlots = 2;
    public float blockMultiplierPenalty = -12f;

    public override void ApplyEffect(GameManager gameManager)
    {
        FindObjectOfType<InventoryManager>().ModifySlotCount(extraSlots);
        gameManager.ApplyBlockMultiplierModifier(blockMultiplierPenalty);
    }

    public override void RemoveEffect(GameManager gameManager)
    {
        // ȷ������Ϸ����������ȷ����״̬
        var inventory = FindObjectOfType<InventoryManager>();
        if (inventory) inventory.ModifySlotCount(-extraSlots);

        gameManager.ApplyBlockMultiplierModifier(-blockMultiplierPenalty);
    }
}