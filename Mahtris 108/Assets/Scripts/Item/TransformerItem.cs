// FileName: TransformerItem.cs
using UnityEngine;

[CreateAssetMenu(fileName = "Transformer", menuName = "Items/Common/Transformer")]
public class TransformerItem : ItemData
{
    public override bool Use(GameManager gameManager)
    {
        // Spawner��GameManager��˽�б�����ͨ���乫�����Է���
        if (gameManager.Spawner != null)
        {
            return gameManager.Spawner.TransformNextBlock();
        }
        return false;
    }
}