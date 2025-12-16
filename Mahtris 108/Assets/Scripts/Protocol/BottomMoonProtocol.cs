using UnityEngine;

[CreateAssetMenu(fileName = "BottomMoon", menuName = "Protocols/Bottom Moon")]
public class BottomMoonProtocol : ProtocolData
{
    public override void ApplyEffect(GameManager manager)
    {
        manager.isBottomMoonActive = true;
    }

    public override void RemoveEffect(GameManager manager)
    {
        manager.isBottomMoonActive = false;
    }
}