// FileName: FrenziedPlayerProtocol.cs
using UnityEngine;

[CreateAssetMenu(fileName = "FrenziedPlayerProtocol", menuName = "Protocols/FrenziedPlayer")]
public class FrenziedPlayerProtocol : ProtocolData
{
    public float extraMultiplier = 3.0f;

    public override void ApplyEffect(GameManager gameManager)
    {
        FindObjectOfType<InventoryManager>().SetUsable(false);
        gameManager.ApplyExtraMultiplier(extraMultiplier);
    }

    public override void RemoveEffect(GameManager gameManager)
    {
        var inventory = FindObjectOfType<InventoryManager>();
        if (inventory) inventory.SetUsable(true);

        gameManager.ApplyExtraMultiplier(1f / extraMultiplier);
    }
}