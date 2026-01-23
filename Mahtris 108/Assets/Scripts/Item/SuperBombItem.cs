// FileName: SuperBombItem.cs
using UnityEngine;

[CreateAssetMenu(fileName = "SuperBomb", menuName = "Items/Advanced/SuperBomb")]
public class SuperBombItem : ItemData
{
    public int rowsToClear = 6;

    public override bool Use(GameManager gameManager)
    {
        bool success = gameManager.ForceClearRowsFromBottom(rowsToClear);

        if (!success)
        {
            if (AudioManager.Instance) AudioManager.Instance.PlayBuyFailSound();
            var ui = FindObjectOfType<GameUIController>();
            if (ui != null)
            {
                string msg = LocalizationManager.Instance ? LocalizationManager.Instance.GetText("ITEM_TIPS_01") : "没有可以消除的方块！";
                ui.ShowToast(msg);
            }
            return false;
        }

        return true;
    }
}