using UnityEngine;
[CreateAssetMenu(menuName = "Protocols/MarshLand")]
public class MarshLandProtocol : ProtocolData
{
    public float extraMultiplier = 2f;
    public override void ApplyEffect(GameManager gm) { gm.ApplyExtraMultiplier(extraMultiplier); gm.isMarshLandActive = true; }
    public override void RemoveEffect(GameManager gm) { gm.ApplyExtraMultiplier(1f / extraMultiplier); gm.isMarshLandActive = false; }
}