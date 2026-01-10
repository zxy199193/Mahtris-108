// FileName: ProtocolLastStand.cs
using UnityEngine;

[CreateAssetMenu(fileName = "LastStand", menuName = "Protocols/Last Stand")]
public class LastStandProtocol : ProtocolData
{
    public override void ApplyEffect(GameManager manager)
    {
        manager.isLastStandActive = true;

        // 额外倍率 x16
        manager.ApplyExtraMultiplier(16f);
    }

    public override void RemoveEffect(GameManager manager)
    {
        manager.isLastStandActive = false;

        // 恢复倍率 (除以 16)
        manager.ApplyExtraMultiplier(1f / 16f);
    }
}