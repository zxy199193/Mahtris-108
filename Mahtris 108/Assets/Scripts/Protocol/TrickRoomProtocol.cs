// FileName: TrickRoomProtocol.cs
using UnityEngine;

[CreateAssetMenu(fileName = "TrickRoomProtocol", menuName = "Protocols/TrickRoom")]
public class TrickRoomProtocol : ProtocolData
{
    public float extraMultiplier = 3f;

    public override void ApplyEffect(GameManager gameManager)
    {
        gameManager.ApplyExtraMultiplier(extraMultiplier);
        gameManager.isTrickRoomActive = true;
    }

    public override void RemoveEffect(GameManager gameManager)
    {
        gameManager.ApplyExtraMultiplier(1f / extraMultiplier);
        gameManager.isTrickRoomActive = false;
    }
}