using UnityEngine;
using UnityEngine.UI;

public class HuTileUI : MonoBehaviour
{
    public Image image;

    // 初始化牌面，tileId对应Sprite数组索引
    public void Init(int tileId, Sprite[] mjSprites)
    {
        if (tileId >= 0 && tileId < mjSprites.Length)
        {
            image.sprite = mjSprites[tileId];
            image.SetNativeSize(); // 设置Image尺寸和Sprite匹配
        }
        else
        {
            Debug.LogWarning("HuTileUI: tileId 超出范围");
        }
    }
}
