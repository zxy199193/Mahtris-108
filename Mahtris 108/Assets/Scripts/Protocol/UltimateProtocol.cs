// FileName: UltimateProtocol.cs
using UnityEngine;

[CreateAssetMenu(fileName = "UltimateProtocol", menuName = "Protocols/Ultimate")]
public class UltimateProtocol : ProtocolData
{
    public float extraMultiplier = 36f;

    public override void ApplyEffect(GameManager gameManager)
    {
        gameManager.ApplyExtraMultiplier(extraMultiplier);
        gameManager.isUltimateActive = true;
        gameManager.ultimateHuCount = 0;
    }

    public override void RemoveEffect(GameManager gameManager)
    {
        gameManager.ApplyExtraMultiplier(1f / extraMultiplier);
        gameManager.isUltimateActive = false;
    }
}