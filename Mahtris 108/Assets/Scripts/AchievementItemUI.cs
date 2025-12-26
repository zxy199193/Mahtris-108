using UnityEngine;
using UnityEngine.UI;

public class AchievementItemUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Text titleText;
    [SerializeField] private Text descText;
    [SerializeField] private Text rewardText;
    [SerializeField] private GameObject unlockedBadge; // "已达成"标记
    [SerializeField] private GameObject lockedOverlay;   // 未达成时的灰色遮罩

    public void Setup(AchievementData data, bool isUnlocked)
    {
        if (iconImage) iconImage.sprite = data.icon;
        if (titleText)
        {
            titleText.text = data.GetName();
            if (LocalizationManager.Instance) LocalizationManager.Instance.UpdateFont(titleText);
        }
        if (descText)
        {
            descText.text = data.GetDescription();
            if (LocalizationManager.Instance) LocalizationManager.Instance.UpdateFont(descText);
        }
        if (rewardText)
        {
            rewardText.text = $"{data.rewardGold}";
            if (LocalizationManager.Instance) LocalizationManager.Instance.UpdateFont(rewardText);
        }

            // 状态显示
            if (unlockedBadge) unlockedBadge.SetActive(isUnlocked);
        if (lockedOverlay) lockedOverlay.SetActive(!isUnlocked);
    }
}