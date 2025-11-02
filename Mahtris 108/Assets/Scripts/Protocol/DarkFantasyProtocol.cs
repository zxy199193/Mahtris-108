// FileName: DarkFantasyProtocol.cs
using UnityEngine;

[CreateAssetMenu(fileName = "DarkFantasyProtocol", menuName = "Protocols/DarkFantasy")]
public class DarkFantasyProtocol : ProtocolData
{
    public float extraMultiplier = 4f;

    public override void ApplyEffect(GameManager gameManager)
    {
        gameManager.ApplyExtraMultiplier(extraMultiplier);
        gameManager.isDarkFantasyActive = true;
    }

    public override void RemoveEffect(GameManager gameManager)
    {
        gameManager.ApplyExtraMultiplier(1f / extraMultiplier);
        gameManager.isDarkFantasyActive = false;
    }
}