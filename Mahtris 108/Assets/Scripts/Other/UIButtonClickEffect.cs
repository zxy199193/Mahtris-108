using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class UIButtonClickEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("缩放参数")]
    public float scaleFactor = 0.95f;       // 按下时缩小比例
    public float scaleSpeed = 20f;         // 缩放动画速度

    [Header("粒子特效")]
    public bool enableParticleEffect = false;
    public GameObject particlePrefab;       // 粒子预制体
    public Canvas parentCanvas;            // 按钮所在 Canvas（必填）

    private Vector3 originalScale;
    private Vector3 targetScale;

    void Awake()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    void Update()
    {
        // 平滑缩放
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        targetScale = originalScale * scaleFactor;

        // 播放粒子
        PlayParticleEffect();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        targetScale = originalScale;
    }

    private void PlayParticleEffect()
    {
        if (!enableParticleEffect) return;   // ← 关键：是否启用
        if (particlePrefab == null || parentCanvas == null)
            return;

        // 获取按钮位置（世界坐标）
        RectTransform rect = GetComponent<RectTransform>();
        Vector3 worldPos = rect.transform.position;

        GameObject effect = Instantiate(particlePrefab, worldPos, Quaternion.identity);
        effect.transform.SetParent(parentCanvas.transform, true);
        Destroy(effect, 2f);
    }
}
