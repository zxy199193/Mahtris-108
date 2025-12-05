using UnityEngine;
[CreateAssetMenu(menuName = "Protocols/TimeIsMoney")]
public class TimeIsMoneyProtocol : ProtocolData
{
    public override void ApplyEffect(GameManager gm) { gm.isTimeIsMoneyActive = true; }
    public override void RemoveEffect(GameManager gm) { gm.isTimeIsMoneyActive = false; }
}