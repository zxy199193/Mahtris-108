using UnityEngine;
[CreateAssetMenu(menuName = "Protocols/Realpolitik")]
public class RealpolitikProtocol : ProtocolData
{
    public override void ApplyEffect(GameManager gm) { gm.isRealpolitikActive = true; }
    public override void RemoveEffect(GameManager gm) { gm.isRealpolitikActive = false; }
}