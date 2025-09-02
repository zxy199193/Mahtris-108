// FileName: ItemData.cs
using UnityEngine;

public abstract class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite itemIcon;
    [TextArea(3, 5)]
    public string itemDescription;

    // ����boolֵ��ʾ�����Ƿ�ʹ�óɹ�
    public abstract bool Use(GameManager gameManager);
}