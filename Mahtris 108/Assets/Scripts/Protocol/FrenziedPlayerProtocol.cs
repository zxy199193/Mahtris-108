// FileName: FrenziedPlayerProtocol.cs
using UnityEngine;

[CreateAssetMenu(fileName = "FrenziedPlayerProtocol", menuName = "Protocols/FrenziedPlayer")]
public class FrenziedPlayerProtocol : ProtocolData
{
    public float extraMultiplier = 4.0f;

    public override void ApplyEffect(GameManager gameManager)
    {
        gameManager.isFrenziedActive = true;
        gameManager.ApplyExtraMultiplier(extraMultiplier);

        if (gameManager.Inventory != null)
        {
            // gameManager.Inventory.SetUsable(false); 
        }
    }

    public override void RemoveEffect(GameManager gameManager)
    {
        gameManager.isFrenziedActive = false;
        gameManager.ApplyExtraMultiplier(1f / extraMultiplier);

        if (gameManager.Inventory != null)
        {
            // gameManager.Inventory.SetUsable(true);
        }
    }
}