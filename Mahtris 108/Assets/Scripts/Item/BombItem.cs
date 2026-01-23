// FileName: BombItem.cs
using UnityEngine;

[CreateAssetMenu(fileName = "Bomb", menuName = "Items/Common/Bomb")]
public class BombItem : ItemData
{
    public int rowsToClear = 3;

    public override bool Use(GameManager gameManager)
    {
        // 尝试使用炸弹，获取是否成功炸到方块
        bool success = gameManager.ForceClearRowsFromBottom(rowsToClear);

        if (!success)
        {
            // 失败音效
            if (AudioManager.Instance) AudioManager.Instance.PlayBuyFailSound();

            // 提示多语言文本
            var ui = FindObjectOfType<GameUIController>();
            if (ui != null)
            {
                string msg = LocalizationManager.Instance ? LocalizationManager.Instance.GetText("ITEM_TIPS_01") : "没有可以消除的方块！";
                ui.ShowToast(msg);
            }
            return false; // 返回 false 表示不消耗道具
        }

        return true; // 成功，消耗道具
    }
}