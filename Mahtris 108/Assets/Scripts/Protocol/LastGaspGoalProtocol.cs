// FileName: ProtocolLastGaspGoal.cs
using UnityEngine;

[CreateAssetMenu(fileName = "LastGaspGoal", menuName = "Protocols/Last Gasp Goal")]
public class ProtocolLastGaspGoal : ProtocolData
{
    public override void ApplyEffect(GameManager manager)
    {
        manager.isLastGaspGoalActive = true;
    }

    public override void RemoveEffect(GameManager manager)
    {
        manager.isLastGaspGoalActive = false;
        // 移除效果后，GameManager 的 Update 会自动清理 lastGaspGoalBonus
        // 但为了保险，也可以在这里强制刷新一下
        manager.UpdateCurrentBaseScore();
    }
}