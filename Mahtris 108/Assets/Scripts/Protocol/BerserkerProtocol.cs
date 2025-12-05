using UnityEngine;
[CreateAssetMenu(menuName = "Protocols/Berserker")]
public class BerserkerProtocol : ProtocolData
{
    public float extraMultiplier = 4f;
    public override void ApplyEffect(GameManager gm) { gm.ApplyExtraMultiplier(extraMultiplier); gm.isBerserkerActive = true; }
    public override void RemoveEffect(GameManager gm) { gm.ApplyExtraMultiplier(1f / extraMultiplier); gm.isBerserkerActive = false; }
}