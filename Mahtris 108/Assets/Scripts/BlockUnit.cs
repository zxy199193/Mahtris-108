// FileName: BlockUnit.cs
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

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

    [Header("牌库预览配置")]
    [SerializeField] private GameObject countInfoGroup;
    [SerializeField] private Text countText;
    [SerializeField] private GameObject darkMask;

    private Color _originalTextColor = Color.white;
    private BlockPool blockPool;

    // 【标记】记录当前是否处于牌库模式，防止 Awake 误关
    private bool _isPoolMode = false;

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

        // 【关键修复】只有在非牌库模式下，才强制关闭 UI
        // 如果是从 GameUIController 初始化的，_isPoolMode 已经是 true 了，这里就不要关了
        if (!_isPoolMode)
        {
            DisablePoolUI();
        }
    }

    private void LateUpdate()
    {
        if (spriteHolder != null && (GameManager.Instance == null || !GameManager.Instance.isNoGravityActive))
        {
            spriteHolder.rotation = Quaternion.identity;
        }
    }

    // 1. 游戏内初始化
    public void Initialize(int id, BlockPool pool)
    {
        this.blockPool = pool;
        DisablePoolUI(); // 游戏内强制关闭

        if (id == -1)
        {
            blockId = -1;
            if (uiImage != null) { uiImage.sprite = emptySprite; uiImage.color = emptyColor; uiImage.enabled = true; }
            if (spriteRenderer != null) { spriteRenderer.sprite = emptySprite; spriteRenderer.color = emptyColor; }
            if (statusText != null) { statusText.text = emptyMessage; statusText.gameObject.SetActive(true); }
        }
        else
        {
            if (uiImage != null) uiImage.color = Color.white;
            if (spriteRenderer != null) spriteRenderer.color = Color.white;
            if (statusText != null) statusText.gameObject.SetActive(false);
            ApplyIdAndSprite(id);
        }
    }

    // 2. 牌库预览初始化
    public void InitializeForPoolViewer(int id, int count, BlockPool pool)
    {
        this.blockPool = pool;
        this.blockId = id;
        this._isPoolMode = true; // 【标记】进入牌库模式

        ApplyIdAndSprite(id);

        // 开启背板
        if (countInfoGroup != null) countInfoGroup.SetActive(true);

        // 设置文本
        if (countText != null)
        {
            countText.text = count.ToString();
            // 确保开启
            countText.gameObject.SetActive(true);
            countText.color = (count == 0) ? Color.red : _originalTextColor;
        }

        if (darkMask != null)
        {
            darkMask.SetActive(count <= 0);
        }

        // 确保其他干扰 UI 关闭
        if (statusText != null) statusText.gameObject.SetActive(false);
    }

    // 强制关闭牌库UI
    public void DisablePoolUI()
    {
        this._isPoolMode = false; // 【标记】退出牌库模式

        if (countInfoGroup != null) countInfoGroup.SetActive(false);
        if (countText != null) countText.gameObject.SetActive(false);
        if (darkMask != null) darkMask.SetActive(false);
    }

    // 3. 【双重保险】当物体被激活时，根据模式再次确认状态
    // 这可以防止 Animator 或其他脚本在 OnEnable 时重置了显隐状态
    void OnEnable()
    {
        if (_isPoolMode)
        {
            if (countInfoGroup != null) countInfoGroup.SetActive(true);
            if (countText != null) countText.gameObject.SetActive(true);
        }
    }

    private void ApplyIdAndSprite(int id)
    {
        this.blockId = id;
        if (blockPool != null)
        {
            Sprite sprite = blockPool.GetSpriteForBlock(id);
            if (spriteRenderer != null) spriteRenderer.sprite = sprite;
            if (uiImage != null) uiImage.sprite = sprite;

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