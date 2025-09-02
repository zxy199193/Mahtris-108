// FileName: ReplicatorItem.cs
using UnityEngine;

[CreateAssetMenu(fileName = "Replicator", menuName = "Items/Common/Replicator")]
public class ReplicatorItem : ItemData
{
    public int repeatCount = 3;

    public override bool Use(GameManager gameManager)
    {
        gameManager.Spawner.ActivateReplicator(repeatCount);
        return true;
    }
}