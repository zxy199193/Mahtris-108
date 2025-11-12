using UnityEngine;
[CreateAssetMenu(menuName = "Protocols/RenewableEnergy")]
public class RenewableEnergyProtocol : ProtocolData
{
    public override void ApplyEffect(GameManager gm) { gm.isRenewableEnergyActive = true; }
    public override void RemoveEffect(GameManager gm) { gm.isRenewableEnergyActive = false; }
}