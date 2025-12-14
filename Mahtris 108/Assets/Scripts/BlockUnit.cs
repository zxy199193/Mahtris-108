// FileName: BlockUnit.cs
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // 如果你使用了DoTween，保留引用

public class BlockUnit : MonoBehaviour
{
    public int blockId { get; private set; } = -1;

    [Header("核心引用")]
    [SerializeField] private Transform spriteHolder;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Image uiImage;

    [Header("游戏内状态配置")]
    [SerializeField] private Text statusText;
    [SerializeField] private Sprite emptySprite;
    [SerializeField] private string emptyMessage = "缺";
    [SerializeField] private Color emptyColor = new Color(0.8f, 0.2f, 0.2f, 1f);

    // =========================================================
    // 【新增】牌库预览专用配置 (Pool Viewer)
    // =========================================================
    [Header("牌库预览配置")]
    [SerializeField] private GameObject countInfoGroup; // 数量显示的父节点/背板
    [SerializeField] private Text countText;            // 数量文本
    [SerializeField] private GameObject darkMask;       // 数量为0时的遮罩
    
    private Color _originalTextColor = Color.white;
    private BlockPool blockPool;

    void Awake()
    {
        if (spriteRenderer == null && spriteHolder != null)
            spriteRenderer = spriteHolder.GetComponent<SpriteRenderer>();

        if (uiImage == null)
            uiImage = GetComponent<Image>();

        if (countText != null)
        {
            _originalTextColor = countText.color;
        }
        DisablePoolUI();
    }

    private void LateUpdate()
    {
        // 仅在“无重力”条约*未*激活时才重置旋转
        if (spriteHolder != null && (GameManager.Instance == null || !GameManager.Instance.isNoGravityActive))
        {
            spriteHolder.rotation = Quaternion.identity;
        }
    }

    // ---------------------------------------------------------
    // 1. 原有的初始化方法 (用于游戏内方块、下一个方块预览)
    // ---------------------------------------------------------
    public void Initialize(int id, BlockPool pool)
    {
        this.blockPool = pool;

        // 确保牌库UI是关闭的
        DisablePoolUI();

        if (id == -1)
        {
            // === 状态：麻将不足 ===
            blockId = -1;
            if (uiImage != null) { uiImage.sprite = emptySprite; uiImage.color = emptyColor; uiImage.enabled = true; }
            if (spriteRenderer != null) { spriteRenderer.sprite = emptySprite; spriteRenderer.color = emptyColor; }
            if (statusText != null) { statusText.text = emptyMessage; statusText.gameObject.SetActive(true); }
        }
        else
        {
            // === 状态：正常 ===
            if (uiImage != null) uiImage.color = Color.white;
            if (spriteRenderer != null) spriteRenderer.color = Color.white;
            if (statusText != null) statusText.gameObject.SetActive(false);
            ApplyIdAndSprite(id);
        }
    }

    // ---------------------------------------------------------
    // 2. 【新增】牌库预览专用初始化方法
    // ---------------------------------------------------------
    public void InitializeForPoolViewer(int id, int count, BlockPool pool)
    {
        this.blockPool = pool;
        this.blockId = id;

        // 1. 设置麻将图案
        ApplyIdAndSprite(id);

        // 2. 开启背板
        if (countInfoGroup != null) countInfoGroup.SetActive(true);

        // 3. 设置数量
        if (countText != null)
        {
            countText.text = count.ToString();
            if (count == 0)
            {
                countText.color = Color.red;
            }
            else
            {
                // 恢复为 Awake 时记录的颜色
                countText.color = _originalTextColor;
            }
        }

        // 4. 设置遮罩 (没牌了就遮住)
        if (darkMask != null)
        {
            darkMask.SetActive(count <= 0);
        }
    }

    // 【新增】强制关闭牌库UI (用于防止在普通方块上显示数量)
    public void DisablePoolUI()
    {
        if (countInfoGroup != null) countInfoGroup.SetActive(false);
        if (darkMask != null) darkMask.SetActive(false);
    }

    // ---------------------------------------------------------
    // 内部逻辑
    // ---------------------------------------------------------
    private void ApplyIdAndSprite(int id)
    {
        this.blockId = id;
        if (blockPool != null)
        {
            Sprite sprite = blockPool.GetSpriteForBlock(id);
            if (spriteRenderer != null) spriteRenderer.sprite = sprite;
            if (uiImage != null) uiImage.sprite = sprite;

            // 无重力逻辑
            if (GameManager.Instance != null && GameManager.Instance.isNoGravityActive && spriteHolder != null)
            {
                int[] rotations = { 0, 90, 180, 270 };
                float randomRotation = rotations[Random.Range(0, rotations.Length)];
                spriteHolder.localRotation = Quaternion.Euler(0, 0, randomRotation);
            }
            else if (spriteHolder != null)
            {
                spriteHolder.localRotation = Quaternion.identity;
            }
        }
    }

    // 黑暗幻想淡出逻辑 (保持不变)
    public void StartFadeToBlack()
    {
        if (GameManager.Instance != null && GameManager.Instance.isDarkFantasyActive)
        {
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