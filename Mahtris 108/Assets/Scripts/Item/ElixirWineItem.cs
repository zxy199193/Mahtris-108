// FileName: ElixirWineItem.cs
using UnityEngine;

[CreateAssetMenu(fileName = "ElixirWine", menuName = "Items/Advanced/ElixirWine")]
public class ElixirWineItem : ItemData
{
    public float multiplier = 2f;

    public override bool Use(GameManager gameManager)
    {
        gameManager.ApplyPermanentBaseScoreMultiplier(multiplier);
        return true;
    }
}