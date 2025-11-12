using UnityEngine;
[CreateAssetMenu(menuName = "Protocols/RoutineWork")]
public class RoutineWorkProtocol : ProtocolData
{
    public override void ApplyEffect(GameManager gm) { gm.isRoutineWorkActive = true; }
    public override void RemoveEffect(GameManager gm) { gm.isRoutineWorkActive = false; }
}