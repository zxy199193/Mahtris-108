using UnityEngine;

[CreateAssetMenu(fileName = "New Champagne", menuName = "Items/Common/Champagne")]
public class ChampagneItem : ItemData
{
    public override bool Use(GameManager gameManager)
    {
        gameManager.ActivateChampagne();
        return true; // ÏûºÄµÀ¾ß
    }
}