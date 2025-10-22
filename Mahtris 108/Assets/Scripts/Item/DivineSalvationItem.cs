// FileName: DivineSalvationItem.cs
using UnityEngine;

[CreateAssetMenu(fileName = "DivineSalvation", menuName = "Items/Advanced/DivineSalvation")]
public class DivineSalvationItem : ItemData
{
    [Header("新版效果 (V4.1)")]
    [Tooltip("本局游戏基础分增加 18")]
    public int baseScoreBonus = 18;

    [Tooltip("本局游戏速度等级降低 8")]
    public int speedBonus = -8;

    [Tooltip("增加 80 秒游戏时间")]
    public float timeBonus = 80f;

    [Tooltip("强行消除最底部的 3 行")]
    public int rowsToClear = 3;

    public override bool Use(GameManager gameManager)
    {
        // 1. 调用新的“永久基础分”系统
        gameManager.ApplyPermanentBaseScoreBonus(baseScoreBonus);

        // 2. 调用新的“永久速度”系统
        gameManager.ApplyPermanentSpeedBonus(speedBonus);

        // 3. 调用“增加时间”
        gameManager.AddTime(timeBonus);

        // 4. 调用“强制消行”
        gameManager.ForceClearRowsFromBottom(rowsToClear);

        return true;
    }
}