// FileName: ProtocolData.cs
using UnityEngine;

public abstract class ProtocolData : ScriptableObject
{
    public string protocolName;
    public Sprite protocolIcon;
    [TextArea(3, 5)]
    public string protocolDescription;
    public bool isLegendary = false;
    [Header("商店配置")]
    public int price = 100;

    [Tooltip("是否为初始条约（默认已解锁）")]
    public bool isInitial = false;

    [Tooltip("仅传奇条约有效：显示该条约需要已解锁多少个普通条约才能看见")]
    public int unlockConditionCount = 0;
    public abstract void ApplyEffect(GameManager gameManager);
    public abstract void RemoveEffect(GameManager gameManager);
}