using UnityEngine;
[CreateAssetMenu(menuName = "Protocols/SSSVIP")]
public class SSSVIPProtocol : ProtocolData
{
    public override void ApplyEffect(GameManager gm) { gm.isSSSVIPActive = true; }
    public override void RemoveEffect(GameManager gm) { gm.isSSSVIPActive = false; }
}