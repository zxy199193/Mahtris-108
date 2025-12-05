using UnityEngine;
[CreateAssetMenu(menuName = "Protocols/LogBridge")]
public class LogBridgeProtocol : ProtocolData
{
    public float extraMultiplier = 3f;
    public override void ApplyEffect(GameManager gm) { gm.ApplyExtraMultiplier(extraMultiplier); gm.isLogBridgeActive = true; }
    public override void RemoveEffect(GameManager gm) { gm.ApplyExtraMultiplier(1f / extraMultiplier); gm.isLogBridgeActive = false; }
}