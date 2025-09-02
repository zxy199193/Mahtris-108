// FileName: EraserItem.cs
using UnityEngine;

[CreateAssetMenu(fileName = "Eraser", menuName = "Items/Common/Eraser")]
public class EraserItem : ItemData
{
    public override bool Use(GameManager gameManager)
    {
        return gameManager.HuPaiArea.RemoveLastSet();
    }
}