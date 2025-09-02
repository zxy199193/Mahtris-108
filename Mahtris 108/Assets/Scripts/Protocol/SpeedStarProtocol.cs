// FileName: SpeedStarProtocol.cs
using UnityEngine;

[CreateAssetMenu(fileName = "SpeedStarProtocol", menuName = "Protocols/SpeedStar")]
public class SpeedStarProtocol : ProtocolData
{
    public float speedIncrease = 1.0f; // 增加100%
    public float extraMultiplier = 2.0f;

    public override void ApplyEffect(GameManager gameManager)
    {
        gameManager.ModifySpeedByPercentage(speedIncrease * 100f);
        gameManager.ApplyExtraMultiplier(extraMultiplier);
    }

    public override void RemoveEffect(GameManager gameManager)
    {
        // 游戏结束后，GameManager的状态会自动重置，但为了逻辑严谨可以实现移除效果
        gameManager.ModifySpeedByPercentage(-speedIncrease * 100f);
        gameManager.ApplyExtraMultiplier(1f / extraMultiplier);
    }
}