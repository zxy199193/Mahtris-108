using UnityEngine;
[CreateAssetMenu(menuName = "Protocols/GreatRevolution")]
public class GreatRevolutionProtocol : ProtocolData
{
    public float extraMultiplier = 5f;
    public override void ApplyEffect(GameManager gm) { gm.ApplyExtraMultiplier(extraMultiplier); gm.isGreatRevolutionActive = true; }
    public override void RemoveEffect(GameManager gm) { gm.ApplyExtraMultiplier(1f / extraMultiplier); gm.isGreatRevolutionActive = false; }
}