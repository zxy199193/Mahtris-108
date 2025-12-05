using UnityEngine;
[CreateAssetMenu(menuName = "Protocols/OldSchool")]
public class OldSchoolProtocol : ProtocolData
{
    public override void ApplyEffect(GameManager gm) { gm.isOldSchoolActive = true; }
    public override void RemoveEffect(GameManager gm) { gm.isOldSchoolActive = false; }
}