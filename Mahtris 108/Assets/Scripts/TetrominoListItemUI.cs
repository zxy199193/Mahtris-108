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

    [Header("堆叠显示")]
    [Tooltip("堆叠数量显示的父容器（背板），用于整体控制显隐")]
    public GameObject countGroup;
    [Tooltip("用于显示堆叠数量的Text组件 (例如 'x2')")]
    public Text countText;
    [Tooltip("被强化时的图标标志")]
    public GameObject buffIcon;
    // 【修改】增加 overrideMultiplier 参数
    public void InitializeForPrefab(GameObject uiPrefab, string text, float overrideMultiplier = -1f, bool isBuffed = false)
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
                multiplierText.text = $"{overrideMultiplier:F0}";
            }
            else
            {
                multiplierText.text = text;
            }
        }
        if (buffIcon != null)
        {
            buffIcon.SetActive(isBuffed);
        }
    }
    public void SetStackCount(int count)
    {
        bool shouldShow = count > 1;

        // 1. 优先控制背板容器的显隐
        if (countGroup != null)
        {
            countGroup.SetActive(shouldShow);
        }
        else if (countText != null)
        {
            // 兼容旧设置：如果没有背板组，直接控制文本
            countText.gameObject.SetActive(shouldShow);
        }

        // 2. 设置文本内容
        if (shouldShow && countText != null)
        {
            countText.text = $"x{count}";
        }
    }
}