// FileName: TrialSampleItem.cs
using UnityEngine;

[CreateAssetMenu(fileName = "TrialSample", menuName = "Items/Common/TrialSample")]
public class TrialSampleItem : ItemData
{
    [Tooltip("下次胡牌时额外获得的T1-Dot方块数量")]
    public int bonusBlockCount = 2;

    [Tooltip("要奖励的方块的预制件名称")]
    public string blockPrefabName = "T1-Dot";

    public override bool Use(GameManager gameManager)
    {
        gameManager.ActivateBonusBlocksOnHu(blockPrefabName, bonusBlockCount);
        return true;
    }
}