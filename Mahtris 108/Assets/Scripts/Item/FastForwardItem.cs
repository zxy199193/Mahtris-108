// FileName: FastForwardItem.cs
using UnityEngine;

[CreateAssetMenu(fileName = "FastForward", menuName = "Items/Common/FastForward")]
public class FastForwardItem : ItemData
{
    [Tooltip("永久增加的胡牌轮数")]
    public int huCountBonus = 2;

    [Tooltip("永久增加的速度等级")]
    public int speedBonus = 2;

    public override bool Use(GameManager gameManager)
    {
        // 调用GameManager的接口来修改速度
        gameManager.ApplyPermanentSpeedBonus(speedBonus);

        // 调用GameManager的接口来修改胡牌轮数
        gameManager.AddHuCount(huCountBonus);

        return true;
    }
}