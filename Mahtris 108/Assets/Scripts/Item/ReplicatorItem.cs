using UnityEngine;
[CreateAssetMenu(fileName = "Replicator", menuName = "Items/Replicator")]
public class ReplicatorItem : ItemData
{
    public int repeatCount = 3;
    public override bool Use(GameManager gameManager)
    {
        gameManager.Spawner.ActivateReplicator(repeatCount);
        return true;
    }
}