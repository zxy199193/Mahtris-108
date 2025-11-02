using UnityEngine;
[CreateAssetMenu(fileName = "HandOfMidas", menuName = "Items/Advanced/HandOfMidas")]
public class HandOfMidasItem : ItemData
{
    [Tooltip("效果持续时间（秒）")]
    public float duration = 20f;
    public override bool Use(GameManager gameManager)
    {
        gameManager.ActivateMidas(duration);
        return true;
    }
}