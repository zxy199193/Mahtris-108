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
    public void InitializeForPrefab(GameObject uiPrefab, string text)
    {
        if (shapeContainer != null)
        {
            // 清空旧的形状
            foreach (Transform child in shapeContainer)
            {
                Destroy(child.gameObject);
            }
            // 实例化新的形状
            if (uiPrefab != null)
            {
                Instantiate(uiPrefab, shapeContainer);
            }
        }

        if (multiplierText != null)
        {
            multiplierText.text = text;
        }
    }
}