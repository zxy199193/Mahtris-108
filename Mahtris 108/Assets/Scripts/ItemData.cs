// FileName: ItemData.cs
using UnityEngine;

public abstract class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite itemIcon;
    [TextArea(3, 5)]
    public string itemDescription;

    public abstract bool Use(GameManager gameManager); // 改为接收GameManager引用
}