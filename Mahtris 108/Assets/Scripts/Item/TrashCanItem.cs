// FileName: TrashCanItem.cs
using UnityEngine;

[CreateAssetMenu(fileName = "TrashCan", menuName = "Items/Common/TrashCan")]
public class TrashCanItem : ItemData
{
    [Tooltip("跳过麻将判定的次数")]
    public int ignoreCount = 1;

    public override bool Use(GameManager gameManager)
    {
        gameManager.SetIgnoreMahjongCheck(ignoreCount);
        return true;
    }
}