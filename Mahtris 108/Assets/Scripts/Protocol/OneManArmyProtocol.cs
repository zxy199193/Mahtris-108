using UnityEngine;

[CreateAssetMenu(fileName = "OneManArmy", menuName = "Protocols/One Man Army")]
public class OneManArmyProtocol : ProtocolData
{
    public override void ApplyEffect(GameManager manager)
    {
        manager.isOneManArmyActive = true;
        // 立即触发一次计算 (处理刚获得时的 x2)
        manager.RecalculateOneManArmy();
    }

    public override void RemoveEffect(GameManager manager)
    {
        manager.isOneManArmyActive = false;
        // 立即触发一次计算 (处理移除时的清理)
        manager.RecalculateOneManArmy();
    }
}