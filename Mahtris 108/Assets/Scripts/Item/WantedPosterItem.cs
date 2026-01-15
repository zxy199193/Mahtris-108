using UnityEngine;

[CreateAssetMenu(fileName = "WantedPoster", menuName = "Items/Advanced/WantedPoster")]
public class WantedPosterItem : ItemData
{
    [Header("悬赏配置")]
    [Tooltip("增加基础奖金的百分比 (0.5 = 50%)")]
    public float bonusPercentage = 0.5f;

    public override bool Use(GameManager gameManager)
    {
        // 调用修改后的方法
        gameManager.ActivateWantedPoster(bonusPercentage);
        return true;
    }
}