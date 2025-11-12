// FileName: ReviveStoneItem.cs
using UnityEngine;

[CreateAssetMenu(fileName = "ReviveStone", menuName = "Items/Advanced/ReviveStone")]
public class ReviveStoneItem : ItemData
{
    [Tooltip("复活后增加的时间")]
    public float addedTime = 30f;

    public override bool Use(GameManager gameManager)
    {
        gameManager.ActivateReviveStone(addedTime);
        return true;
    }
}