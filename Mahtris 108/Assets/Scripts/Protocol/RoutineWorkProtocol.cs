using UnityEngine;

[CreateAssetMenu(menuName = "Protocols/RoutineWork")]
public class RoutineWorkProtocol : ProtocolData
{
    public override void ApplyEffect(GameManager gm)
    {
        // 1. 激活原始逻辑标记 (固定时间)
        gm.isRoutineWorkActive = true;

        // 2. 【新增】应用 3倍 额外倍率
        gm.ApplyExtraMultiplier(3f);

        Debug.Log("朝九晚五生效：时间锁定95s，额外倍率 x3");
    }

    public override void RemoveEffect(GameManager gm)
    {
        // 1. 移除原始逻辑标记
        gm.isRoutineWorkActive = false;

        // 2. 【新增】移除倍率 (除以3还原)
        gm.ApplyExtraMultiplier(1f / 3f);

        Debug.Log("朝九晚五移除：倍率还原");
    }
}