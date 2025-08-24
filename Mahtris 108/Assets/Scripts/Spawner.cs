// FileName: Spawner.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Spawner : MonoBehaviour
{
    [Header("Tetromino Ԥ�Ƽ��б�")]
    [Tooltip("��Ϸ��ʼʱʹ�õ�Tetromino")]
    [SerializeField] private GameObject[] initialTetrominoPrefabs;
    [Tooltip("���п��ܳ��ֵ�Tetromino�����ں��ƺ������ȡ��")]
    [SerializeField] private GameObject[] masterTetrominoPrefabs;

    [Header("ģ������")]
    [SerializeField] private BlockPool blockPool;
    [SerializeField] private TetrisGrid tetrisGrid;

    public IEnumerable<GameObject> GetActivePrefabs() => activeTetrominoPool;

    private List<GameObject> activeTetrominoPool;
    private GameSettings settings;
    private GameObject nextTetrominoPrefab;
    private List<int> nextTileIds;

    // ���������״̬
    private int replicationCount = 0;
    private GameObject replicationPrefab = null;

    // --- ������������---
    // �޸��� 'ActivateReplicator' not found �Ĵ���
    public void ActivateReplicator(int count)
    {
        if (nextTetrominoPrefab != null)
        {
            replicationPrefab = nextTetrominoPrefab;
            // ��Ϊ��ǰ��"��һ��"���Ͼ�Ҫ�����ˣ�����ʵ�ʸ��ƵĴ����� count - 1
            replicationCount = count - 1;
            Debug.Log($"�������Ѽ������ [{replicationPrefab.name}] �������ٳ��� {replicationCount} �Ρ�");
        }
    }

    public void InitializeForNewGame(GameSettings gameSettings)
    {
        this.settings = gameSettings;
        activeTetrominoPool = new List<GameObject>(initialTetrominoPrefabs);
        replicationCount = 0; // ���ø�����״̬
        replicationPrefab = null;

        if (activeTetrominoPool == null || activeTetrominoPool.Count == 0)
        {
            Debug.LogError("Spawner��û�������κγ�ʼTetromino��");
            return;
        }
        StartNextRound();
    }

    public void StartNextRound()
    {
        PrepareNextTetromino();
        SpawnBlock();
    }

    public GameObject AddRandomTetrominoOfLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= settings.tetrominoLevels.Count) return null;

        var levelDef = settings.tetrominoLevels[levelIndex];
        var candidates = masterTetrominoPrefabs.Where(p => {
            int count = p.GetComponentsInChildren<BlockUnit>().Length;
            return count >= levelDef.minBlocks && count <= levelDef.maxBlocks;
        }).ToList();

        if (candidates.Count == 0) return null;

        var chosenPrefab = candidates[Random.Range(0, candidates.Count)];
        activeTetrominoPool.Add(chosenPrefab);

        GameManager.Instance.RecalculateTotalMultiplier();

        return chosenPrefab;
    }

    private void PrepareNextTetromino()
    {
        // ���ȴ��������߼�
        if (replicationCount > 0 && replicationPrefab != null)
        {
            nextTetrominoPrefab = replicationPrefab;
            replicationCount--;
            if (replicationCount == 0) // ��������һ�θ��ƣ������״̬
            {
                replicationPrefab = null;
            }
        }
        else // ��������߼�
        {
            if (activeTetrominoPool.Count > 0)
            {
                nextTetrominoPrefab = activeTetrominoPool[Random.Range(0, activeTetrominoPool.Count)];
            }
        }

        if (nextTetrominoPrefab == null) { GameEvents.TriggerGameOver(); return; }

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
        if (nextTetrominoPrefab == null) { GameEvents.TriggerGameOver(); return; }

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


