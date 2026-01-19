// FileName: ChaoSuanLiProtocol.cs
using UnityEngine;

[CreateAssetMenu(fileName = "ChaoSuanLiProtocol", menuName = "Protocols/ChaoSuanLi")]
public class ChaoSuanLiProtocol : ProtocolData
{
    [Header("倍率配置")]
    [Tooltip("拥有5格方块时的额外得分倍率 (例如 3 = x3)")]
    public float extraMultiplier = 3f;

    [Tooltip("5格方块生成的权重倍率 (例如 3 = 概率提升200%)")]
    public float spawnWeightMultiplier = 3f;

    public override void ApplyEffect(GameManager gameManager)
    {
        gameManager.isChaoSuanLiActive = true;
        gameManager.SetChaoSuanLiSpawnMultiplier(spawnWeightMultiplier);
        gameManager.RecalculateChaoSuanLiStatus();
    }

    public override void RemoveEffect(GameManager gameManager)
    {
        gameManager.isChaoSuanLiActive = false;
        gameManager.SetChaoSuanLiSpawnMultiplier(1f);
        gameManager.RecalculateChaoSuanLiStatus();
    }
}