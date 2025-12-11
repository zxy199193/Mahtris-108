// FileName: BlockUnit.cs
using UnityEngine;
using UnityEngine.UI; // 引入UI命名空间

public class BlockUnit : MonoBehaviour
{
    public int blockId { get; private set; } = -1;

    [Header("引用")]
    [SerializeField] private Transform spriteHolder;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Image uiImage; // 【新增】UI Image引用

    private BlockPool blockPool;

    void Awake()
    {
        if (spriteRenderer == null && spriteHolder != null)
        {
            spriteRenderer = spriteHolder.GetComponent<SpriteRenderer>();
        }

        // 【新增】如果 uiImage 未在Inspector中指定，则自动获取
        if (uiImage == null)
        {
            uiImage = GetComponent<Image>();
        }
    }

    private void LateUpdate()
    {
        // 【修改】仅在“无重力”条约*未*激活时才重置旋转
        if (spriteHolder != null && (GameManager.Instance == null || !GameManager.Instance.isNoGravityActive))
        {
            spriteHolder.rotation = Quaternion.identity;
        }
    }

    public void Initialize(int id, BlockPool pool)
    {
        this.blockPool = pool;
        ApplyIdAndSprite(id);
    }

    private void ApplyIdAndSprite(int id)
    {
        this.blockId = id;
        if (blockPool != null)
        {
            Sprite sprite = blockPool.GetSpriteForBlock(id);

            // 【修改】同时更新 SpriteRenderer 和 Image
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = sprite;
            }
            if (uiImage != null)
            {
                uiImage.sprite = sprite;
            }
            // 【新增】“无重力”逻辑
            if (GameManager.Instance != null && GameManager.Instance.isNoGravityActive && spriteHolder != null)
            {
                int[] rotations = { 0, 90, 180, 270 };
                float randomRotation = rotations[Random.Range(0, rotations.Length)];
                spriteHolder.localRotation = Quaternion.Euler(0, 0, randomRotation);
            }
            else if (spriteHolder != null) // 确保非条约时转回来
            {
                spriteHolder.localRotation = Quaternion.identity;
            }
            // --- 修改结束 ---
        }
    }
    // 【新增】供“深邃黑暗幻想”条约调用
    public void StartFadeToBlack()
    {
        // 检查条约是否激活，且我们有可以修改的Sprite
        if (GameManager.Instance != null && GameManager.Instance.isDarkFantasyActive)
        {
            // 使用 DoTween 插件在 3 秒内将颜色变为黑色
            // 确保你已经在项目和脚本中正确导入了 DoTween

            // if (spriteRenderer != null)
            // {
            //     spriteRenderer.DOColor(Color.black, 3f);
            // }
            // if (uiImage != null)
            // {
            //     uiImage.DOColor(Color.black, 3f);
            // }

            // --- 如果没有 DoTween，使用 Coroutine ---
            StartCoroutine(FadeCoroutine(12f));
        }
    }

    private System.Collections.IEnumerator FadeCoroutine(float duration)
    {
        float timer = 0f;
        Color startColor = Color.white;
        if (spriteRenderer != null) startColor = spriteRenderer.color;
        if (uiImage != null) startColor = uiImage.color;

        Color endColor = Color.black;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            Color newColor = Color.Lerp(startColor, endColor, timer / duration);

            if (spriteRenderer != null) spriteRenderer.color = newColor;
            if (uiImage != null) uiImage.color = newColor;

            yield return null;
        }

        if (spriteRenderer != null) spriteRenderer.color = endColor;
        if (uiImage != null) uiImage.color = endColor;
    }
}