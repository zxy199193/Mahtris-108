using UnityEngine;
[CreateAssetMenu(fileName = "Hourglass", menuName = "Items/Hourglass")]
public class HourglassItem : ItemData
{
    public float timeToAdd = 40f;
    public override bool Use(GameManager gameManager)
    {
        gameManager.AddTime(timeToAdd);
        return true;
    }
}