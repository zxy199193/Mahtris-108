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
        if (spriteHolder != null)
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
        }
    }
}