using UnityEngine;
[CreateAssetMenu(menuName = "Protocols/AdventFood")]
public class AdventFoodProtocol : ProtocolData
{
    public override void ApplyEffect(GameManager gm) { gm.isAdventFoodActive = true; }
    public override void RemoveEffect(GameManager gm) { gm.isAdventFoodActive = false; }
}