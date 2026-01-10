using UnityEngine;
[CreateAssetMenu(fileName = "Jetpack", menuName = "Items/Common/Jetpack")]
public class JetpackItem : ItemData
{
    public int blockCount = 10;
    public override bool Use(GameManager gameManager)
    {
        gameManager.ActivateJetpack(blockCount);
        return true;
    }
}