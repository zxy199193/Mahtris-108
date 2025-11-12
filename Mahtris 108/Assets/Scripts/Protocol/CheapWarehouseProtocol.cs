using UnityEngine;
[CreateAssetMenu(menuName = "Protocols/CheapWarehouse")]
public class CheapWarehouseProtocol : ProtocolData
{
    public float extraMultiplier = 2f;
    public override void ApplyEffect(GameManager gm) { gm.ApplyExtraMultiplier(extraMultiplier); gm.isCheapWarehouseActive = true; }
    public override void RemoveEffect(GameManager gm) { gm.ApplyExtraMultiplier(1f / extraMultiplier); gm.isCheapWarehouseActive = false; }
}