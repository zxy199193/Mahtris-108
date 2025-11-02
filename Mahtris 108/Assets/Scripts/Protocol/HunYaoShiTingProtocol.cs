// FileName: HunYaoShiTingProtocol.cs
using UnityEngine;

[CreateAssetMenu(fileName = "HunYaoShiTingProtocol", menuName = "Protocols/HunYaoShiTing")]
public class HunYaoShiTingProtocol : ProtocolData
{
    public float blockMultiplierPenalty = -16f;

    public override void ApplyEffect(GameManager gameManager)
    {
        gameManager.ApplyBlockMultiplierModifier(blockMultiplierPenalty);
        gameManager.isHunYaoShiTingActive = true;
    }

    public override void RemoveEffect(GameManager gameManager)
    {
        gameManager.ApplyBlockMultiplierModifier(-blockMultiplierPenalty);
        gameManager.isHunYaoShiTingActive = false;
    }
}