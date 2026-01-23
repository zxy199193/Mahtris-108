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
        // 1. 先尝试执行核心功能：消除底部行
        // 如果场上没有任何已锁定的方块，这个方法会返回 false
        bool success = gameManager.ForceClearRowsFromBottom(rowsToClear);

        if (!success)
        {
            // 失败：播放错误音效，弹出提示，且不消耗道具
            if (AudioManager.Instance) AudioManager.Instance.PlayBuyFailSound();
            var ui = FindObjectOfType<GameUIController>();
            if (ui != null)
            {
                string msg = LocalizationManager.Instance ? LocalizationManager.Instance.GetText("ITEM_TIPS_01") : "没有可以消除的方块！";
                ui.ShowToast(msg);
            }
            return false;
        }

        // 2. 消除成功后，再应用所有增益效果
        gameManager.ApplyPermanentBaseScoreBonus(baseScoreBonus);
        gameManager.ApplyPermanentSpeedBonus(speedBonus);
        gameManager.AddTime(timeBonus);

        return true; // 消耗道具
    }
}