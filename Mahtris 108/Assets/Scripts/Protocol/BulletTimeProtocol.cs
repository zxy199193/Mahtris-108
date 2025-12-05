using UnityEngine;
[CreateAssetMenu(menuName = "Protocols/BulletTime")]
public class BulletTimeProtocol : ProtocolData
{
    public override void ApplyEffect(GameManager gm) { gm.isBulletTimeActive = true; }
    public override void RemoveEffect(GameManager gm) { gm.isBulletTimeActive = false; }
}