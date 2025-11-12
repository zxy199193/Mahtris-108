using UnityEngine;
[CreateAssetMenu(menuName = "Protocols/UnstableCurrent")]
public class UnstableCurrentProtocol : ProtocolData
{
    public override void ApplyEffect(GameManager gm) { gm.isUnstableCurrentActive = true; }
    public override void RemoveEffect(GameManager gm) { gm.isUnstableCurrentActive = false; }
}