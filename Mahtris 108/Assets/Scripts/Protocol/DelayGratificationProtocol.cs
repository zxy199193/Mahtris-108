using UnityEngine;
[CreateAssetMenu(menuName = "Protocols/DelayGratification")]
public class DelayGratificationProtocol : ProtocolData
{
    public override void ApplyEffect(GameManager gm) { gm.isDelayGratificationActive = true; gm.UpdateCurrentBaseScore(); }
    public override void RemoveEffect(GameManager gm) { gm.isDelayGratificationActive = false; gm.UpdateCurrentBaseScore(); }
}