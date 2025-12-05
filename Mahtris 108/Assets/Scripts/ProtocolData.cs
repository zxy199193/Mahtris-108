// FileName: ProtocolData.cs
using UnityEngine;

public abstract class ProtocolData : ScriptableObject
{
    public string protocolName;
    public Sprite protocolIcon;
    [TextArea(3, 5)]
    public string protocolDescription;
    public bool isLegendary = false;

    public abstract void ApplyEffect(GameManager gameManager);
    public abstract void RemoveEffect(GameManager gameManager);
}