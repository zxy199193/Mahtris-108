// FileName: ItemData.cs
using UnityEngine;

public abstract class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite itemIcon;
    [TextArea(3, 5)]
    public string itemDescription;
    public bool isLegendary = false;
    public AudioClip useSound;
    // 返回bool值表示道具是否使用成功
    public abstract bool Use(GameManager gameManager);
}