// FileName: SpeedStarProtocol.cs
using UnityEngine;

[CreateAssetMenu(fileName = "SpeedStarProtocol", menuName = "Protocols/SpeedStar")]
public class SpeedStarProtocol : ProtocolData
{
    public float speedIncrease = 1.0f; // Ôö¼Ó100%
    public float extraMultiplier = 2.0f;

    public override void ApplyEffect(GameManager gameManager)
    {
        gameManager.ApplySpeedToCurrentTetromino(speedIncrease * 100f);
        gameManager.ApplyExtraMultiplier(extraMultiplier);
    }

    public override void RemoveEffect(GameManager gameManager)
    {
        gameManager.ApplySpeedToCurrentTetromino(-speedIncrease * 100f);
        gameManager.ApplyExtraMultiplier(1f / extraMultiplier);
    }
}