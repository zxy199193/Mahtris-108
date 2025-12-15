using UnityEngine;

[CreateAssetMenu(fileName = "New Scissors", menuName = "Items/Common/Scissors")]
public class ScissorsItem : ItemData
{
    public override bool Use(GameManager gameManager)
    {
        // ActivateScissors 返回 bool，如果没方块可删(false)则不消耗道具
        return gameManager.ActivateScissors();
    }
}