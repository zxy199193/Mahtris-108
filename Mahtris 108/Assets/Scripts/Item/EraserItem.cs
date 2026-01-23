// FileName: EraserItem.cs
using UnityEngine;

[CreateAssetMenu(fileName = "Eraser", menuName = "Items/Common/Eraser")]
public class EraserItem : ItemData
{
    public override bool Use(GameManager gameManager)
    {
        // 尝试移除胡牌区最后一组牌
        bool success = gameManager.HuPaiArea.RemoveLastSet();

        // 如果移除失败（胡牌区为空），显示提示并播放错误音效
        if (!success)
        {
            if (AudioManager.Instance) AudioManager.Instance.PlayBuyFailSound();

            var ui = FindObjectOfType<GameUIController>();
            if (ui != null)
            {
                // 获取多语言文本，Key 为 ITEM_TIPS_03
                string msg = LocalizationManager.Instance ? LocalizationManager.Instance.GetText("ITEM_TIPS_03") : "胡牌区没有可以移除的牌！";
                ui.ShowToast(msg);
            }
            return false; // 返回 false，不消耗道具
        }

        return true; // 成功移除，消耗道具
    }
}