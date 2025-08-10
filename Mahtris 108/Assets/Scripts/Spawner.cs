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
        // һ����Ҫ4������С����ID
        List<int> ids = BlockPool.Instance.GetBlockIds(4);

        if (ids == null || ids.Count < 4)
        {
            Debug.LogWarning("BlockPool ��û���㹻��С���������·��飬�ȴ����ա���");
            // ��ֱ��Game Over������Ϸ�ȴ�
            return;
        }

        // �������������״
        int randomIndex = Random.Range(0, tetrominoPrefabs.Length);
        GameObject block = Instantiate(tetrominoPrefabs[randomIndex], transform.position, Quaternion.identity);

        // ��������ڲ��нű����� ids���������ﴫ��ȥ
        BlockUnit[] units = block.GetComponentsInChildren<BlockUnit>();
        for (int i = 0; i < units.Length && i < ids.Count; i++)
        {
            units[i].Init(ids[i]);
        }
    }
}