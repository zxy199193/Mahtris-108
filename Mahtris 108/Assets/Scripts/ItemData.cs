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
    [Header("商店配置")]
    public int price = 100;

    [Tooltip("是否为初始道具（默认已解锁）")]
    public bool isInitial = false;

    [Tooltip("仅传奇道具有效：显示该道具需要已解锁多少个普通道具才能看见")]
    public int unlockConditionCount = 0;
    public abstract bool Use(GameManager gameManager);
}