// FileName: QueYiMenProtocol.cs
using UnityEngine;

[CreateAssetMenu(fileName = "QueYiMenProtocol", menuName = "Protocols/QueYiMen")]
public class QueYiMenProtocol : ProtocolData
{
    public float blockMultiplierPenalty = -16f;

    public override void ApplyEffect(GameManager gameManager)
    {
        gameManager.ApplyBlockMultiplierModifier(blockMultiplierPenalty);
        gameManager.queYiMenSuitToRemove = Random.Range(0, 3); // 0=Í², 1=Íò, 2=Ìõ
        gameManager.useQueYiMenFilter = true;
    }

    public override void RemoveEffect(GameManager gameManager)
    {
        gameManager.ApplyBlockMultiplierModifier(-blockMultiplierPenalty);
        gameManager.useQueYiMenFilter = false;
        gameManager.queYiMenSuitToRemove = -1;
    }
}