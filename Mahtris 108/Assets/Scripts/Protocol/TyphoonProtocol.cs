// FileName: TyphoonProtocol.cs
using UnityEngine;

[CreateAssetMenu(fileName = "TyphoonProtocol", menuName = "Protocols/Typhoon")]
public class TyphoonProtocol : ProtocolData
{
    public float extraMultiplier = 2f;

    public override void ApplyEffect(GameManager gameManager)
    {
        gameManager.ApplyExtraMultiplier(extraMultiplier);
        gameManager.isTyphoonActive = true;
    }

    public override void RemoveEffect(GameManager gameManager)
    {
        gameManager.ApplyExtraMultiplier(1f / extraMultiplier);
        gameManager.isTyphoonActive = false;
    }
}