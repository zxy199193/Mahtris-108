using UnityEngine;

[CreateAssetMenu(fileName = "New GoldenApple", menuName = "Items/Advanced/Golden Apple")]
public class GoldenAppleItem : ItemData
{
    public override bool Use(GameManager gameManager)
    {
        gameManager.ActivateGoldenApple();
        return true; // ÏûºÄµÀ¾ß
    }
}