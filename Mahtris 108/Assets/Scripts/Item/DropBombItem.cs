using UnityEngine;

[CreateAssetMenu(fileName = "New DropBomb", menuName = "Items/Common/Drop Bomb")]
public class DropBombItem : ItemData
{
    public override bool Use(GameManager gameManager)
    {
        // 尝试从顶部消除
        bool success = gameManager.ActivateDropBomb();

        if (!success)
        {
            if (AudioManager.Instance) AudioManager.Instance.PlayBuyFailSound();
            var ui = FindObjectOfType<GameUIController>();
            if (ui != null)
            {
                // 空投炸弹的失败原因同样是场上没有方块
                string msg = LocalizationManager.Instance ? LocalizationManager.Instance.GetText("ITEM_TIPS_01") : "没有可以消除的方块！";
                ui.ShowToast(msg);
            }
            return false; // 不消耗道具
        }

        return true; // 消耗道具
    }
}