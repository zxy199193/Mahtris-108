// FileName: TransformerItem.cs
using UnityEngine;

[CreateAssetMenu(fileName = "Transformer", menuName = "Items/Common/Transformer")]
public class TransformerItem : ItemData
{
    public override bool Use(GameManager gameManager)
    {
        // Spawner是GameManager的私有变量，通过其公共属性访问
        if (gameManager.Spawner != null)
        {
            return gameManager.Spawner.TransformNextBlock();
        }
        return false;
    }
}