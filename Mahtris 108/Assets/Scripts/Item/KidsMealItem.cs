// FileName: KidsMealItem.cs
using UnityEngine;

[CreateAssetMenu(fileName = "KidsMeal", menuName = "Items/Advanced/KidsMeal")]
public class KidsMealItem : ItemData
{
    [Tooltip("持续时间")]
    public float duration = 60f;

    public override bool Use(GameManager gameManager)
    {
        gameManager.ActivateKidsMeal(duration);
        return true;
    }
}