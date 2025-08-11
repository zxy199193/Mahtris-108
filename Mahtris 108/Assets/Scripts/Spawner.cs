using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject[] tetrominoPrefabs;
    public Vector3 spawnPosition = Vector3.zero;

    void Start()
    {
        SpawnBlock();
    }

    public void SpawnBlock()
    {
        if (tetrominoPrefabs == null || tetrominoPrefabs.Length == 0) { Debug.LogError("Spawner: 没有tetrominoPrefabs"); return; }
        if (BlockPool.Instance == null) { Debug.LogWarning("Spawner: BlockPool.Instance null"); return; }

        List<int> ids = BlockPool.Instance.GetBlockIds(4);
        if (ids == null || ids.Count < 4) { Debug.LogWarning("Spawner: 牌池不足，无法生成方块"); return; }

        int idx = Random.Range(0, tetrominoPrefabs.Length);
        GameObject go = Instantiate(tetrominoPrefabs[idx], spawnPosition == Vector3.zero ? transform.position : spawnPosition, Quaternion.identity);
        var units = go.GetComponentsInChildren<BlockUnit>();
        for (int i = 0; i < units.Length && i < ids.Count; i++)
            units[i].Init(ids[i]);

        Debug.Log($"[Spawner] Spawned tetromino idx={idx} with ids {string.Join(",", ids)}");
    }
}
