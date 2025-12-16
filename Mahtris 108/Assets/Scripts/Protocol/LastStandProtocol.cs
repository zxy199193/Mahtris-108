// FileName: ProtocolLastStand.cs
using UnityEngine;

[CreateAssetMenu(fileName = "LastStand", menuName = "Protocols/Last Stand")]
public class LastStandProtocol : ProtocolData
{
    public override void ApplyEffect(GameManager manager)
    {
        manager.isLastStandActive = true;

        // 额外倍率 x10
        manager.ApplyExtraMultiplier(10f);
    }

    public override void RemoveEffect(GameManager manager)
    {
        manager.isLastStandActive = false;

        // 恢复倍率 (除以 10)
        manager.ApplyExtraMultiplier(0.1f);
    }
}