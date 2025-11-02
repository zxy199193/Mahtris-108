// FileName: ChaoSuanLiProtocol.cs
using UnityEngine;

[CreateAssetMenu(fileName = "ChaoSuanLiProtocol", menuName = "Protocols/ChaoSuanLi")]
public class ChaoSuanLiProtocol : ProtocolData
{
    public float extraMultiplier = 3f;

    public override void ApplyEffect(GameManager gameManager)
    {
        gameManager.ApplyExtraMultiplier(extraMultiplier);
        gameManager.isChaoSuanLiActive = true;
    }

    public override void RemoveEffect(GameManager gameManager)
    {
        gameManager.ApplyExtraMultiplier(1f / extraMultiplier);
        gameManager.isChaoSuanLiActive = false;
    }
}