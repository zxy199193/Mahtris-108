using UnityEngine;
using UnityEngine.UI;

public class HuTileUI : MonoBehaviour
{
    public Image image;

    // ��ʼ�����棬tileId��ӦSprite��������
    public void Init(int tileId, Sprite[] mjSprites)
    {
        if (tileId >= 0 && tileId < mjSprites.Length)
        {
            image.sprite = mjSprites[tileId];
            image.SetNativeSize(); // ����Image�ߴ��Spriteƥ��
        }
        else
        {
            Debug.LogWarning("HuTileUI: tileId ������Χ");
        }
    }
}
