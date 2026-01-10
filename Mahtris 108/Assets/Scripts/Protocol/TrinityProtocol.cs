using UnityEngine;
[CreateAssetMenu(menuName = "Protocols/Trinity")]
public class TrinityProtocol : ProtocolData
{
    public float blockMultPenalty = 0f;
    public override void ApplyEffect(GameManager gm) { gm.ApplyBlockMultiplierModifier(blockMultPenalty); gm.isTrinityActive = true; }
    public override void RemoveEffect(GameManager gm) { gm.ApplyBlockMultiplierModifier(-blockMultPenalty); gm.isTrinityActive = false; }
}