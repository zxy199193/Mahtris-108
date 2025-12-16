using UnityEngine;

[CreateAssetMenu(fileName = "Protocol_NatureReserve", menuName = "Protocols/Nature Reserve")]
public class NatureReserveProtocol : ProtocolData
{
    public override void ApplyEffect(GameManager manager)
    {
        manager.isNatureReserveActive = true;
    }

    public override void RemoveEffect(GameManager manager)
    {
        manager.isNatureReserveActive = false;
    }
}