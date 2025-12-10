using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class UIButtonClickEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("缩放参数")]
    public float scaleFactor = 0.95f;       // 按下时缩小比例
    public float scaleSpeed = 20f;         // 缩放动画速度

    [Header("音效设置")]
    public bool enableSound = true;        // 是否播放声音
    [Tooltip("如果不填，则使用 AudioManager 里的默认点击声；如果填了，则播放这个声音")]
    public AudioClip customSound;          // 可选：自定义音效

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
        // 1. 设置缩放目标
        targetScale = originalScale * scaleFactor;

        // 2. 播放粒子
        PlayParticleEffect();

        // 3. 播放声音 (新增)
        PlayButtonSound();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // 恢复缩放
        targetScale = originalScale;
    }

    private void PlayParticleEffect()
    {
        if (!enableParticleEffect) return;
        if (particlePrefab == null || parentCanvas == null)
            return;

        RectTransform rect = GetComponent<RectTransform>();
        Vector3 worldPos = rect.transform.position;

        GameObject effect = Instantiate(particlePrefab, worldPos, Quaternion.identity);
        effect.transform.SetParent(parentCanvas.transform, true);
        Destroy(effect, 2f);
    }

    // 【新增】处理声音逻辑
    private void PlayButtonSound()
    {
        if (!enableSound) return;
        if (AudioManager.Instance == null) return;

        if (customSound != null)
        {
            // 如果配置了特殊音效，就播特殊的
            AudioManager.Instance.PlaySFX(customSound);
        }
        else
        {
            // 否则，播通用的点击声
            AudioManager.Instance.PlayButtonClickSound();
        }
    }
}