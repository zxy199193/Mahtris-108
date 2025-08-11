using UnityEngine;

public class BlockUnit : MonoBehaviour
{
    [Header("数据")]
    public int blockId = -1;

    [Header("引用")]
    public Transform spriteHolder; // 用于保持图片正立（可选）
    public SpriteRenderer spriteRenderer;

    // 是否只是用于展示（胡牌区）——展示用销毁时不回收
    private bool isDisplayOnly = false;

    private void Awake()
    {
        if (spriteRenderer == null && spriteHolder != null)
            spriteRenderer = spriteHolder.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    // 兼容旧接口：Spawner 等调用 Init(id)
    public void Init(int id)
    {
        InitFromPool(id);
    }

    // 从牌库分配的方块（棋盘用），非展示
    public void InitFromPool(int id)
    {
        isDisplayOnly = false;
        ApplyIdAndSprite(id);
    }

    // 标记为胡牌区展示用（不会在销毁时回收）
    public void SetAsDisplay(int id)
    {
        isDisplayOnly = true;
        ApplyIdAndSprite(id);
    }

    // 强制设置（可指定是否展示）
    public void SetBlockId(int id, bool displayOnly = false)
    {
        isDisplayOnly = displayOnly;
        ApplyIdAndSprite(id);
    }

    private void ApplyIdAndSprite(int id)
    {
        blockId = id;

        if (spriteRenderer != null && BlockPool.Instance != null)
        {
            Sprite s = BlockPool.Instance.GetSpriteForBlock(blockId);
            spriteRenderer.sprite = s;
            spriteRenderer.enabled = (s != null);
        }

        if (spriteHolder != null)
            spriteHolder.localRotation = Quaternion.identity;
    }

    private void LateUpdate()
    {
        if (spriteHolder != null)
            spriteHolder.rotation = Quaternion.identity;
    }

    // 显式：把自己回收到牌池并销毁（外部调用）
    public void ReturnToPoolAndDestroy()
    {
        if (!isDisplayOnly)
        {
            if (BlockPool.Instance != null && blockId >= 0)
            {
                BlockPool.Instance.ReturnBlock(blockId);
                Debug.Log($"[BlockUnit] ReturnToPoolAndDestroy returned id={blockId}");
            }
        }
        Destroy(gameObject);
    }

    // 把这个单元标记为展示（不会回收），并设置父对象（用于 HuPaiArea 的移动方案）
    public void MakeDisplayAndReparent(Transform newParent, Vector3 localPos)
    {
        isDisplayOnly = true;
        transform.SetParent(newParent, worldPositionStays: false);
        transform.localPosition = localPos;
        // 更新 sprite 保证显示
        if (spriteHolder != null) spriteHolder.localRotation = Quaternion.identity;
    }
}
