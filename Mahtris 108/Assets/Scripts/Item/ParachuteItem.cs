// FileName: ParachuteItem.cs
using UnityEngine;

[CreateAssetMenu(fileName = "Parachute", menuName = "Items/Parachute")]
public class ParachuteItem : ItemData
{
    [Tooltip("速度降低的百分比数值，例如输入30代表降低当前速度的30%")]
    public float speedReductionPercent = 30f;

    public override bool Use(GameManager gameManager)
    {
        Debug.Log($"使用了【{itemName}】，速度降低了 {speedReductionPercent}%。");
        // ---【重大修正点】---
        // 调用 GameManager 中正确的新方法名 ModifySpeedByPercentage
        gameManager.ModifySpeedByPercentage(-speedReductionPercent); // 传入负值表示降低
        return true;
    }
}