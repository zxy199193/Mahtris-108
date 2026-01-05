// FileName: BombItem.cs
using UnityEngine;

[CreateAssetMenu(fileName = "Bomb", menuName = "Items/Common/Bomb")]
public class BombItem : ItemData
{
    public int rowsToClear = 3;

    public override bool Use(GameManager gameManager)
    {
        gameManager.ForceClearRowsFromBottom(rowsToClear);
        return true;
    }
}