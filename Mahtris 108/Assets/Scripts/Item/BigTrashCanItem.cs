using UnityEngine;
[CreateAssetMenu(fileName = "BigTrashCan", menuName = "Items/Advanced/BigTrashCan")]
public class BigTrashCanItem : ItemData
{
    [Tooltip("跳过麻将判定的次数")]
    public int ignoreCount = 3;
    public override bool Use(GameManager gameManager)
    {
        // 调用我们为“垃圾筒”道具创建的同一个方法
        gameManager.SetIgnoreMahjongCheck(ignoreCount);
        return true;
    }
}