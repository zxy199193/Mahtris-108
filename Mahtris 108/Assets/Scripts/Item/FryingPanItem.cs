using UnityEngine;

[CreateAssetMenu(fileName = "FryingPan", menuName = "Items/Common/FryingPan")]
public class FryingPanItem : ItemData
{
    [Header("平底锅配置")]
    [Tooltip("增加的游戏时间 (秒)")]
    public float timeBonus = 30f;
    [Tooltip("永久增加的基础分")]
    public int baseScoreBonus = 5;

    public override bool Use(GameManager gameManager)
    {
        // 调用 GameManager 的激活逻辑
        // 如果方块不足，ActivateFryingPan 会返回 false，道具就不会被消耗
        return gameManager.ActivateFryingPan(timeBonus, baseScoreBonus);
    }
}