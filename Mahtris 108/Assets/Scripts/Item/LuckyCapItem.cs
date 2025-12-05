using UnityEngine;

[CreateAssetMenu(fileName = "LuckyCap", menuName = "Items/Common/LuckyCap")]
public class LuckyCapItem : ItemData
{
    public override bool Use(GameManager gameManager)
    {
        gameManager.ActivateLuckyCap();
        return true;
    }
}