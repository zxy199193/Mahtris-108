// FileName: SpeedStarProtocol.cs
using UnityEngine;

[CreateAssetMenu(fileName = "SpeedStarProtocol", menuName = "Protocols/SpeedStar")]
public class SpeedStarProtocol : ProtocolData
{
    public int speedBonus = 10;
    public float extraMultiplier = 2.0f;

    public override void ApplyEffect(GameManager gameManager)
    {
        gameManager.ApplyPermanentSpeedBonus(speedBonus);
        gameManager.ApplyExtraMultiplier(extraMultiplier);
    }

    public override void RemoveEffect(GameManager gameManager)
    {
        gameManager.ApplyPermanentSpeedBonus(-speedBonus);
        gameManager.ApplyExtraMultiplier(1f / extraMultiplier);
    }
}