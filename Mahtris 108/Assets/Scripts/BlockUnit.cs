// FileName: BlockUnit.cs
using UnityEngine;
using UnityEngine.UI; // ����UI�����ռ�

public class BlockUnit : MonoBehaviour
{
    public int blockId { get; private set; } = -1;

    [Header("����")]
    [SerializeField] private Transform spriteHolder;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Image uiImage; // ��������UI Image����

    private BlockPool blockPool;

    void Awake()
    {
        if (spriteRenderer == null && spriteHolder != null)
        {
            spriteRenderer = spriteHolder.GetComponent<SpriteRenderer>();
        }

        // ����������� uiImage δ��Inspector��ָ�������Զ���ȡ
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

            // ���޸ġ�ͬʱ���� SpriteRenderer �� Image
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