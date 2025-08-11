using UnityEngine;

public class BlockUnit : MonoBehaviour
{
    [Header("����")]
    public int blockId = -1;

    [Header("����")]
    public Transform spriteHolder; // ���ڱ���ͼƬ��������ѡ��
    public SpriteRenderer spriteRenderer;

    // �Ƿ�ֻ������չʾ��������������չʾ������ʱ������
    private bool isDisplayOnly = false;

    private void Awake()
    {
        if (spriteRenderer == null && spriteHolder != null)
            spriteRenderer = spriteHolder.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    // ���ݾɽӿڣ�Spawner �ȵ��� Init(id)
    public void Init(int id)
    {
        InitFromPool(id);
    }

    // ���ƿ����ķ��飨�����ã�����չʾ
    public void InitFromPool(int id)
    {
        isDisplayOnly = false;
        ApplyIdAndSprite(id);
    }

    // ���Ϊ������չʾ�ã�����������ʱ���գ�
    public void SetAsDisplay(int id)
    {
        isDisplayOnly = true;
        ApplyIdAndSprite(id);
    }

    // ǿ�����ã���ָ���Ƿ�չʾ��
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

    // ��ʽ�����Լ����յ��Ƴز����٣��ⲿ���ã�
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

    // �������Ԫ���Ϊչʾ��������գ��������ø��������� HuPaiArea ���ƶ�������
    public void MakeDisplayAndReparent(Transform newParent, Vector3 localPos)
    {
        isDisplayOnly = true;
        transform.SetParent(newParent, worldPositionStays: false);
        transform.localPosition = localPos;
        // ���� sprite ��֤��ʾ
        if (spriteHolder != null) spriteHolder.localRotation = Quaternion.identity;
    }
}
