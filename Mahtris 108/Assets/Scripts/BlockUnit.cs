using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlockUnit : MonoBehaviour
{
    public int blockId;

    // 你要保持不旋转的图片子物体Transform，Inspector拖入
    public Transform spriteHolder;

    public void Init(int id)
    {
        blockId = id;

        // 赋值图片
        if (spriteHolder != null)
        {
            SpriteRenderer sr = spriteHolder.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = BlockPool.Instance.GetSpriteForBlock(blockId);
                sr.enabled = true;
            }

            // 初始化图片局部旋转为0
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