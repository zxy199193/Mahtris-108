using UnityEngine;

[CreateAssetMenu(fileName = "New MagicCurtain", menuName = "Items/Common/Magic Curtain")]
public class MagicCurtainItem : ItemData
{
    public override bool Use(GameManager gameManager)
    {
        gameManager.ActivateMagicCurtain();
        return true; // ÏûºÄµÀ¾ß
    }
}