// FileName: BountyItem.cs
using UnityEngine;

[CreateAssetMenu(fileName = "Bounty", menuName = "Items/Advanced/Bounty")]
public class BountyItem : ItemData
{
    public override bool Use(GameManager gameManager)
    {
        gameManager.ActivateBounty();
        return true;
    }
}