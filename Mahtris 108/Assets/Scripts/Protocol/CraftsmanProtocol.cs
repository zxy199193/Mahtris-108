// FileName: CraftsmanProtocol.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Protocols/Craftsman")]
public class CraftsmanProtocol : ProtocolData
{
    [Header("π§Ω≥≈‰÷√")]
    public float triggerInterval = 80f;
    public override void ApplyEffect(GameManager gm)
    {
        gm.ActivateCraftsman(triggerInterval);
    }

    public override void RemoveEffect(GameManager gm)
    {
        gm.isCraftsmanActive = false;
    }
}