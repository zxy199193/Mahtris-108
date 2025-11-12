using UnityEngine;
[CreateAssetMenu(menuName = "Protocols/AttackOnGiant")]
public class AttackOnGiantProtocol : ProtocolData
{
    public float blockMultiplierBonus = 36f; // ·½¿é±¶ÂÊ+36
    public override void ApplyEffect(GameManager gm)
    {
        gm.isAttackOnGiantActive = true;
        gm.ApplyBlockMultiplierModifier(blockMultiplierBonus);
    }
    public override void RemoveEffect(GameManager gm)
    {
        gm.isAttackOnGiantActive = false;
        gm.ApplyBlockMultiplierModifier(-blockMultiplierBonus);
    }
}