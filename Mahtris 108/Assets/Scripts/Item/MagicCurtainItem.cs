using UnityEngine;

[CreateAssetMenu(fileName = "New MagicCurtain", menuName = "Items/Common/Magic Curtain")]
public class MagicCurtainItem : ItemData
{
    public override bool Use(GameManager gameManager)
    {
        // 调用 GameManager 的激活逻辑，获取是否成功
        bool success = gameManager.ActivateMagicCurtain();

        // 如果场上没有方块，拦截使用
        if (!success)
        {
            // 播放失败音效
            if (AudioManager.Instance) AudioManager.Instance.PlayBuyFailSound();

            var ui = FindObjectOfType<GameUIController>();
            if (ui != null)
            {
                // 获取多语言文本，Key 为 ITEM_TIPS_04
                string msg = LocalizationManager.Instance ? LocalizationManager.Instance.GetText("ITEM_TIPS_04") : "没有可以排序的麻将！";
                ui.ShowToast(msg);
            }
            return false; // 返回 false，道具栏里的图标不会消失
        }

        return true; // 成功，消耗道具
    }
}