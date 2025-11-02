// FileName: MeteorShowerProtocol.cs
using UnityEngine;

[CreateAssetMenu(fileName = "MeteorShowerProtocol", menuName = "Protocols/MeteorShower")]
public class MeteorShowerProtocol : ProtocolData
{
    public float extraMultiplier = 2f;

    public override void ApplyEffect(GameManager gameManager)
    {
        gameManager.ApplyExtraMultiplier(extraMultiplier);
        gameManager.isMeteorShowerActive = true;
    }

    public override void RemoveEffect(GameManager gameManager)
    {
        gameManager.ApplyExtraMultiplier(1f / extraMultiplier);
        gameManager.isMeteorShowerActive = false;
    }
}