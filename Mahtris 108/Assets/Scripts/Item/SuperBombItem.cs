// FileName: SuperBombItem.cs
using UnityEngine;

[CreateAssetMenu(fileName = "SuperBomb", menuName = "Items/Advanced/SuperBomb")]
public class SuperBombItem : ItemData
{
    public int rowsToClear = 6;

    public override bool Use(GameManager gameManager)
    {
        gameManager.ForceClearRowsFromBottom(rowsToClear);
        return true;
    }
}