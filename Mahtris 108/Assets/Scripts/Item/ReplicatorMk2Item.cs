// FileName: ReplicatorMk2Item.cs
using UnityEngine;

[CreateAssetMenu(fileName = "ReplicatorMk2", menuName = "Items/Advanced/ReplicatorMk2")]
public class ReplicatorMk2Item : ItemData
{
    public override bool Use(GameManager gameManager)
    {
        ItemData lastItem = gameManager.GetLastUsedItem();

        // =========================================================
        // 【新增判断】如果玩家本局游戏尚未用过任何道具，拦截使用
        // =========================================================
        if (lastItem == null)
        {
            // 播放失败音效
            if (AudioManager.Instance) AudioManager.Instance.PlayBuyFailSound();

            // 弹出 UI 提示
            var ui = FindObjectOfType<GameUIController>();
            if (ui != null)
            {
                // 获取多语言文本，Key 为 ITEM_TIPS_07
                string msg = LocalizationManager.Instance ? LocalizationManager.Instance.GetText("ITEM_TIPS_07") : "尚未使用任何道具！";
                ui.ShowToast(msg);
            }
            return false; // 返回 false，道具栏里的图标不会消失
        }

        // 确保不是试图复制自己（虽然 GameManager 层面已经做了过滤，但保留双重保险）
        if (lastItem.itemName != this.itemName)
        {
            // 成功复制：将上一个道具塞回背包
            gameManager.Inventory.AddItem(lastItem);
            return true; // 成功，消耗复制器
        }

        return false;
    }
}