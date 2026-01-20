using UnityEngine;

[CreateAssetMenu(fileName = "Mist", menuName = "Protocols/Mist")]
public class MistProtocol : ProtocolData
{
    public override void ApplyEffect(GameManager manager)
    {
        manager.isMistActive = true;

        // 1. 开启视觉干扰
        manager.ToggleMistUI(true);

        // 2. 额外倍率 x3
        manager.ApplyExtraMultiplier(2f);
    }

    public override void RemoveEffect(GameManager manager)
    {
        manager.isMistActive = false;

        // 1. 关闭视觉干扰
        manager.ToggleMistUI(false);

        // 2. 移除倍率 (除以 2)
        manager.ApplyExtraMultiplier(1f / 2f);
    }
}