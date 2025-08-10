using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlockUnit : MonoBehaviour
{
    public int blockId;

    // ��Ҫ���ֲ���ת��ͼƬ������Transform��Inspector����
    public Transform spriteHolder;

    public void Init(int id)
    {
        blockId = id;

        // ��ֵͼƬ
        if (spriteHolder != null)
        {
            SpriteRenderer sr = spriteHolder.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = BlockPool.Instance.GetSpriteForBlock(blockId);
                sr.enabled = true;
            }

            // ��ʼ��ͼƬ�ֲ���תΪ0
            spriteHolder.localRotation = Quaternion.identity;
        }
    }

    void LateUpdate()
    {
        if (spriteHolder != null)
        {
            spriteHolder.rotation = Quaternion.identity;
        }
    }
}