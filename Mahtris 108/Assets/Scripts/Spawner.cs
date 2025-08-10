using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject[] tetrominoPrefabs;

    void Start()
    {
        SpawnBlock();
    }
    public void SpawnBlock()
    {
        // 一次需要4个基础小方块ID
        List<int> ids = BlockPool.Instance.GetBlockIds(4);

        if (ids == null || ids.Count < 4)
        {
            Debug.LogWarning("BlockPool 中没有足够的小方块生成新方块，等待回收……");
            // 不直接Game Over，让游戏等待
            return;
        }

        // 正常生成随机形状
        int randomIndex = Random.Range(0, tetrominoPrefabs.Length);
        GameObject block = Instantiate(tetrominoPrefabs[randomIndex], transform.position, Quaternion.identity);

        // 如果方块内部有脚本接收 ids，就在这里传过去
        BlockUnit[] units = block.GetComponentsInChildren<BlockUnit>();
        for (int i = 0; i < units.Length && i < ids.Count; i++)
        {
            units[i].Init(ids[i]);
        }
    }
}