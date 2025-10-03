using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class UIButtonClickEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("���Ų���")]
    public float scaleFactor = 0.95f;       // ����ʱ��С����
    public float scaleSpeed = 20f;         // ���Ŷ����ٶ�

    private Vector3 originalScale;
    private Vector3 targetScale;

    private void Awake()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    private void Update()
    {
        // ƽ������
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        targetScale = originalScale * scaleFactor;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        targetScale = originalScale;
    }
}
