// FileName: FastForwardItem.cs
using UnityEngine;

[CreateAssetMenu(fileName = "FastForward", menuName = "Items/Common/FastForward")]
public class FastForwardItem : ItemData
{
    [Tooltip("永久增加的速度等级")]
    public int speedBonus = 3;

    public override bool Use(GameManager gameManager)
    {
        // 1. 永久增加速度 +3
        gameManager.ApplyPermanentSpeedBonus(speedBonus);

        // 2. 【修改】直接跳到本圈最后一轮
        // 效果：下次胡牌必定触发高级奖励，并进入下一圈
        gameManager.SkipToLastRoundOfLoop();

        return true;
    }
}