// FileName: NoGravityProtocol.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NoGravityProtocol", menuName = "Protocols/NoGravity")]
public class NoGravityProtocol : ProtocolData
{
    public float extraMultiplier = 2f;

    public override void ApplyEffect(GameManager gameManager)
    {
        gameManager.ApplyExtraMultiplier(extraMultiplier);
        gameManager.isNoGravityActive = true;
    }

    public override void RemoveEffect(GameManager gameManager)
    {
        gameManager.ApplyExtraMultiplier(1f / extraMultiplier);
        gameManager.isNoGravityActive = false;
    }
}