using UnityEngine;

[CreateAssetMenu(fileName = "BloomingOnKong", menuName = "Protocols/Blooming On Kong")]
public class BloomingOnKongProtocol : ProtocolData
{
    public override void ApplyEffect(GameManager manager)
    {
        manager.isBloomingOnKongActive = true;

        // 方块倍率 -20
        // GameManager 内部会有逻辑确保倍率最低为 1 (RecalculateBlockMultiplier 中的 Math.Max(1f, ...))
        manager.ApplyBlockMultiplierModifier(-16f);
    }

    public override void RemoveEffect(GameManager manager)
    {
        manager.isBloomingOnKongActive = false;

        // 恢复倍率
        manager.ApplyBlockMultiplierModifier(16f);
    }
}