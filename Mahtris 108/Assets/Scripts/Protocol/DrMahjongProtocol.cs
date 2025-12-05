using UnityEngine;
[CreateAssetMenu(menuName = "Protocols/DrMahjong")]
public class DrMahjongProtocol : ProtocolData
{
    public override void ApplyEffect(GameManager gm) { gm.isDrMahjongActive = true; }
    public override void RemoveEffect(GameManager gm) { gm.isDrMahjongActive = false; }
}