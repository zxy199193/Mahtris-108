using UnityEngine;

[CreateAssetMenu(fileName = "ElixirWine", menuName = "Items/Advanced/ElixirWine")]
public class ElixirWineItem : ItemData
{
    public float multiplier = 1.5f; // 如果您希望是50%，请在Inspector里填 1.5

    public override bool Use(GameManager gameManager)
    {
        // 【修改】不再使用 ApplyPermanentBaseScoreMultiplier
        // 改为调用新写的一次性结算逻辑
        gameManager.ActivateElixirWine(multiplier);
        return true;
    }
}