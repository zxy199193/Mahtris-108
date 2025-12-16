using UnityEngine;

[CreateAssetMenu(fileName = "Subspace", menuName = "Protocols/Subspace")]
public class SubspaceProtocol : ProtocolData
{
    public override void ApplyEffect(GameManager manager)
    {
        manager.isSubspaceActive = true;

        // 激活后，UI上的圈数目标需要立即刷新 (例如从 2/4 变成 2/3)
        // 速度也需要根据 (胡牌数 * +2) 立即提升
        // 我们通过触发一次 UpdateActiveBlockListUI 或者手动调用相关更新方法来实现
        // 但最简单的是让 GameUI 在 Update 或 GameManager 状态改变时自动刷新
        // 鉴于 GameManager.AddProtocol 里我们已经写了 UpdateTargetScoreUI，
        // 我们可以在这里手动刷新一下圈数文本

        // (需要在 GameManager 里暴露一个刷新 UI 的方法，或者直接在这里不做，依靠下一次事件)
        // 为了即时反馈，建议在 GameManager.AddProtocol 里增加一行：
        // gameUI.UpdateLoopProgressText(scoreManager.GetLoopProgressString());
    }

    public override void RemoveEffect(GameManager manager)
    {
        manager.isSubspaceActive = false;
        // 移除后，速度和圈数要求会自动还原
    }
}