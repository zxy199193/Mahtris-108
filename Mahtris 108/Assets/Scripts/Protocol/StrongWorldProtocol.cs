using UnityEngine;
[CreateAssetMenu(menuName = "Protocols/StrongWorld")]
public class StrongWorldProtocol : ProtocolData
{
    public float extraMultiplier = 5f;
    public override void ApplyEffect(GameManager gm) { gm.ApplyExtraMultiplier(extraMultiplier); gm.isStrongWorldActive = true; }
    public override void RemoveEffect(GameManager gm) { gm.ApplyExtraMultiplier(1f / extraMultiplier); gm.isStrongWorldActive = false; }
}