// FileName: ReservationItem.cs
using UnityEngine;

[CreateAssetMenu(fileName = "Reservation", menuName = "Items/Advanced/Reservation")]
public class ReservationItem : ItemData
{
    [Tooltip("效果持续时间（秒）")]
    public float duration = 40f;

    public override bool Use(GameManager gameManager)
    {
        // 激活 GameManager 中的效果
        gameManager.ActivateReservation(duration);

        // 如果您有对应的UI提示，也可以在这里播放
        var ui = FindObjectOfType<GameUIController>();
        if (ui != null)
        {
            string msg = LocalizationManager.Instance ? LocalizationManager.Instance.GetText("ITEM_TIPS_10") : $"预约席位已激活！{duration}秒内只收刻子！";
            ui.ShowToast(msg);
        }

        return true; // 消耗道具
    }
}