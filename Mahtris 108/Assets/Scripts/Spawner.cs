using UnityEngine;
using System.Collections.Generic;

public class Spawner : MonoBehaviour
{
    [Header("Ԥ�Ƽ�")]
    [SerializeField] private GameObject[] tetrominoPrefabs;

    [Header("ģ������")]
    [SerializeField] private BlockPool blockPool;
    [SerializeField] private TetrisGrid tetrisGrid; // ������Grid������

    private GameSettings settings;
    private GameObject nextTetrominoPrefab;
    private List<int> nextTileIds;

    // ---�������㡿---
    // StartSpawning ������Ҫ����������ʹ���Լ����е�settings����
    public void StartSpawning(GameSettings gameSettings)
    {
        this.settings = gameSettings;
        PrepareNextTetromino();
        SpawnBlock();
    }

    private void PrepareNextTetromino()
    {
        nextTetrominoPrefab = tetrominoPrefabs[Random.Range(0, tetrominoPrefabs.Length)];

        int tilesNeeded = nextTetrominoPrefab.GetComponentsInChildren<BlockUnit>().Length;
        nextTileIds = blockPool.GetBlockIds(tilesNeeded);

        if (nextTileIds == null)
        {
            GameEvents.TriggerGameOver();
            return;
        }
        GameEvents.TriggerNextBlockReady(nextTetrominoPrefab, nextTileIds);
    }

    public void SpawnBlock()
    {
        if (nextTetrominoPrefab == null)
        {
            GameEvents.TriggerGameOver();
            return;
        }

        GameObject blockGO = Instantiate(nextTetrominoPrefab, transform.position, Quaternion.identity);
        var tetromino = blockGO.GetComponent<Tetromino>();

        // ---�������㡿---
        // ����Initializeʱ������settings��tetrisGrid��������
        tetromino.Initialize(settings, tetrisGrid);

        var blockUnits = blockGO.GetComponentsInChildren<BlockUnit>();
        for (int i = 0; i < blockUnits.Length && i < nextTileIds.Count; i++)
        {
            blockUnits[i].Initialize(nextTileIds[i], blockPool);
        }

        PrepareNextTetromino();
    }
}
