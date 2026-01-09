using UnityEngine;
[CreateAssetMenu(menuName = "Protocols/AllMenAreEqual")]
public class AllMenAreEqualProtocol : ProtocolData
{
    public override void ApplyEffect(GameManager gm)
    {
        gm.isAllMenEqualActive = true;
        gm.UpdateActiveBlockListUI();
    }
    public override void RemoveEffect(GameManager gm)
    {
        gm.isAllMenEqualActive = false;
        gm.UpdateActiveBlockListUI();
    }
}