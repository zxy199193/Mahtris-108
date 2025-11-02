// FileName: UltimateProtocol.cs
using UnityEngine;

[CreateAssetMenu(fileName = "UltimateProtocol", menuName = "Protocols/Ultimate")]
public class UltimateProtocol : ProtocolData
{
    public float extraMultiplier = 16f;

    public override void ApplyEffect(GameManager gameManager)
    {
        gameManager.ApplyExtraMultiplier(extraMultiplier);
    }

    public override void RemoveEffect(GameManager gameManager)
    {
        gameManager.ApplyExtraMultiplier(1f / extraMultiplier);
    }
}