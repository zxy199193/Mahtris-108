// FileName: TetrominoListItemUI.cs
using UnityEngine;
using UnityEngine.UI;

public class TetrominoListItemUI : MonoBehaviour
{
    [Header("UIԪ������")]
    [Tooltip("��������UI Tetromino��״�ĸ���������")]
    public Transform shapeContainer;

    [Tooltip("������ʾ���ʵ�Text���")]
    public Text multiplierText;

    [Header("�ѵ���ʾ (��ѡ)")]
    [Tooltip("������ʾ�ѵ�������Text��� (���� 'x2')")]
    public Text countText;
}