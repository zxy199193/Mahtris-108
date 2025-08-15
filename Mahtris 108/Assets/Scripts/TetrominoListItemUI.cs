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
}