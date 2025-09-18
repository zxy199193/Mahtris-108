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
    public void InitializeForPrefab(GameObject uiPrefab, string text)
    {
        if (shapeContainer != null)
        {
            // ��վɵ���״
            foreach (Transform child in shapeContainer)
            {
                Destroy(child.gameObject);
            }
            // ʵ�����µ���״
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