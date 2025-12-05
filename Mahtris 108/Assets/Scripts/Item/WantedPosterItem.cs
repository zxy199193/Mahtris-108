using UnityEngine;

[CreateAssetMenu(fileName = "WantedPoster", menuName = "Items/Advanced/WantedPoster")]
public class WantedPosterItem : ItemData
{
    [Tooltip("下次金币奖励倍率")]
    public int goldMultiplier = 3;
    [Tooltip("当前分数增加百分比 (0.2 = 20%)")]
    public float scoreIncreasePercent = 0.2f;

    public override bool Use(GameManager gameManager)
    {
        gameManager.ActivateWantedPoster(goldMultiplier, scoreIncreasePercent);
        return true;
    }
}