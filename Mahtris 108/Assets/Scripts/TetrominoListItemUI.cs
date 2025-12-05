// FileName: TetrominoListItemUI.cs
using UnityEngine;
using UnityEngine.UI;

public class TetrominoListItemUI : MonoBehaviour
{
    [Header("UI元素引用")]
    [Tooltip("用于容纳UI Tetromino形状的父物体容器")]
    public Transform shapeContainer;

    [Tooltip("用于显示倍率的Text组件")]
    public Text multiplierText;

    [Header("堆叠显示 (可选)")]
    [Tooltip("用于显示堆叠数量的Text组件 (例如 'x2')")]
    public Text countText;
    // 【修改】增加 overrideMultiplier 参数
    public void InitializeForPrefab(GameObject uiPrefab, string text, float overrideMultiplier = -1f)
    {
        if (shapeContainer != null)
        {
            foreach (Transform child in shapeContainer) Destroy(child.gameObject);
            if (uiPrefab != null) Instantiate(uiPrefab, shapeContainer);
        }

        if (multiplierText != null)
        {
            // 如果有覆盖值（>0），则显示覆盖值，否则显示原文本
            if (overrideMultiplier > 0)
            {
                multiplierText.text = $"x{overrideMultiplier:F1}";
            }
            else
            {
                multiplierText.text = text;
            }
        }
    }
}