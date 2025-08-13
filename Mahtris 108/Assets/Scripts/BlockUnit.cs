using UnityEngine;

public class BlockUnit : MonoBehaviour
{
    public int blockId { get; private set; } = -1;

    [Header("ÒýÓÃ")]
    [SerializeField] private Transform spriteHolder;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private BlockPool blockPool;

    void Awake()
    {
        if (spriteRenderer == null && spriteHolder != null)
        {
            spriteRenderer = spriteHolder.GetComponent<SpriteRenderer>();
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
        if (spriteRenderer != null && blockPool != null)
        {
            spriteRenderer.sprite = blockPool.GetSpriteForBlock(id);
        }
    }
}
