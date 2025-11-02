// FileName: DuanYaoJiuProtocol.cs
using UnityEngine;

[CreateAssetMenu(fileName = "DuanYaoJiuProtocol", menuName = "Protocols/DuanYaoJiu")]
public class DuanYaoJiuProtocol : ProtocolData
{
    public float blockMultiplierPenalty = -12f;

    public override void ApplyEffect(GameManager gameManager)
    {
        gameManager.ApplyBlockMultiplierModifier(blockMultiplierPenalty);
        gameManager.useDuanYaoJiuFilter = true;
        // 过滤器将在下次重置牌库时生效
    }

    public override void RemoveEffect(GameManager gameManager)
    {
        gameManager.ApplyBlockMultiplierModifier(-blockMultiplierPenalty);
        gameManager.useDuanYaoJiuFilter = false;
    }
}