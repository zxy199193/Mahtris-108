using UnityEngine;

[CreateAssetMenu(fileName = "New DropBomb", menuName = "Items/Common/Drop Bomb")]
public class DropBombItem : ItemData
{
    public override bool Use(GameManager gameManager)
    {
        gameManager.ActivateDropBomb();
        return true; // ÏûºÄµÀ¾ß
    }
}