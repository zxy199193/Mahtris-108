// FileName: Spawner.cs
using UnityEngine;
using System.Collections.Generic;

public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject[] tetrominoPrefabs;
    [SerializeField] private BlockPool blockPool;
    [SerializeField] private TetrisGrid tetrisGrid;

    // ---���������ԡ�---
    // �����ⲿ����Ԥ�Ƽ��б�
    public GameObject[] TetrominoPrefabs => tetrominoPrefabs;

    private GameSettings settings;
    private GameObject nextTetrominoPrefab;
    private List<int> nextTileIds;

    public void StartSpawning(GameSettings gameSettings)
    {
        this.settings = gameSettings;
        if (tetrominoPrefabs == null || tetrominoPrefabs.Length == 0)
        {
            Debug.LogError("Spawner��û�������κ�TetrominoԤ�Ƽ���");
            return;
        }
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

        tetromino.Initialize(settings, tetrisGrid);

        var blockUnits = blockGO.GetComponentsInChildren<BlockUnit>();
        for (int i = 0; i < blockUnits.Length && i < nextTileIds.Count; i++)
        {
            blockUnits[i].Initialize(nextTileIds[i], blockPool);
        }

        PrepareNextTetromino();
    }
}