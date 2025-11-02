// FileName: ObeliskItem.cs
using UnityEngine;

[CreateAssetMenu(fileName = "Obelisk", menuName = "Items/Common/Obelisk")]
public class ObeliskItem : ItemData
{
    [Tooltip("要生成的7格长条方块的预制件名称")]
    public string sevenBlockPrefabName = "T7-I";

    public override bool Use(GameManager gameManager)
    {
        // 检查 GameManger 上的 Spawner 引用 是否存在
        if (gameManager.Spawner != null)
        {
            // 调用 Spawner 上的新方法 来强制生成下一个方块
            // 如果 Spawner 中没有 "T7-I" 预制件，此方法将返回 false
            return gameManager.Spawner.ForceNextBlock(sevenBlockPrefabName);
        }
        return false;
    }
}