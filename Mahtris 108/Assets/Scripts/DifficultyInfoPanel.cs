// FileName: DifficultyInfoPanel.cs
using UnityEngine;
using UnityEngine.UI;

public class DifficultyInfoPanel : MonoBehaviour
{
    [Header("文本引用")]
    [SerializeField] private Text targetScoreText;
    [SerializeField] private Text goldRewardText;
    [SerializeField] private Text timeLimitText;
    [SerializeField] private Text blockSpeedText;
    [SerializeField] private Text startItemCountText;
    [SerializeField] private Text startProtocolCountText;

    // 【新增】用于显示初始方块描述 (可选)
    [SerializeField] private Text initialBlocksDescText;

    public void UpdateUI(DifficultyProfile profile, string blockDesc = "")
    {
        if (profile == null) return;

        if (targetScoreText) targetScoreText.text = $"{profile.targetScore:N0}";
        if (goldRewardText) goldRewardText.text = $"{profile.goldReward}";
        if (timeLimitText) timeLimitText.text = $"{profile.initialTime}s";
        if (blockSpeedText) blockSpeedText.text = $"{profile.blockSpeed}";

        if (startItemCountText) startItemCountText.text = $"{profile.initialItemCount}";
        if (startProtocolCountText) startProtocolCountText.text = $"{profile.initialProtocolCount}";

        if (initialBlocksDescText) initialBlocksDescText.text = blockDesc;
    }
}