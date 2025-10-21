// FileName: StopwatchItem.cs
using UnityEngine;

[CreateAssetMenu(fileName = "Stopwatch", menuName = "Items/Common/Stopwatch")]
public class StopwatchItem : ItemData
{
    public int pauseCountToAdd = 5;

    public override bool Use(GameManager gameManager)
    {
        gameManager.AddPauseCount(pauseCountToAdd);
        gameManager.SetStopwatchActive(true); // 通知GameManager效果已激活
        return true;
    }
}