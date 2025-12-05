using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "Filter", menuName = "Items/Common/Filter")]
public class FilterItem : ItemData
{
    [Tooltip("效果持续时间")]
    public float duration = 40f;

    public override bool Use(GameManager gameManager)
    {
        // 检查是否有非 Lv3 的方块可供生成
        // 如果池子里全是 Lv3，则道具无法使用
        var pool = gameManager.Spawner.GetActivePrefabs();
        bool hasNonLv3 = pool.Any(p => !p.name.StartsWith("T5-")); // 假设 T5 是 Lv3

        if (!hasNonLv3) return false;

        gameManager.ActivateFilter(duration);
        return true;
    }
}