using UnityEngine;
[CreateAssetMenu(fileName = "Parachute", menuName = "Items/Common/Parachute")]
public class ParachuteItem : ItemData
{
    [Header("降落伞配置")]
    [Tooltip("本轮游戏速度降低百分比 (0.6 = 60%)")]
    [Range(0f, 1f)]
    public float speedReductionPercent = 0.6f;

    public override bool Use(GameManager gameManager)
    {
        // 1. 获取当前显示的速度等级 (例如 50)
        int currentSpeed = gameManager.CurrentDisplaySpeed;

        // 2. 按照您的要求，先算出需要扣除的量：50 * 0.6 = 30
        int reductionAmount = Mathf.RoundToInt(currentSpeed * speedReductionPercent);

        // 3. 将扣除量传给 GameManager 进行减速
        gameManager.ApplyRoundSpeedBonus(-reductionAmount);

        Debug.Log($"降落伞生效：当前速度 {currentSpeed}，减少了 {reductionAmount} ({speedReductionPercent * 100}%)，最终速度变为 {currentSpeed - reductionAmount}");

        return true;
    }
}