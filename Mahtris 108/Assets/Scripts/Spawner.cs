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

    private int replicationCount = 0;
    private GameObject replicationPrefab = null;

    public GameObject AddRandomTetrominoOfLevel(int levelIndex)
    {
        // --- �����������־ 1/3��---
        Debug.Log("====== ��ʼ����� Tetromino ======");

        if (levelIndex < 0 || levelIndex >= settings.tetrominoLevels.Count)
        {
            Debug.LogError($"����ĵȼ�����: {levelIndex}");
            return null;
        }

        var levelDef = settings.tetrominoLevels[levelIndex];

        // --- �����������־ 2/3��---
        Debug.Log($"ɸѡ����: �ȼ�='{levelDef.levelName}', ��������Χ=[{levelDef.minBlocks}, {levelDef.maxBlocks}]");
        Debug.Log("--- ��� Master List �е����з��� ---");
        foreach (var p in masterTetrominoPrefabs)
        {
            if (p != null)
                Debug.Log($"-> ��鵽: {p.name}, �������� BlockUnit ����: {p.GetComponentsInChildren<BlockUnit>().Length}");
        }
        Debug.Log("------------------------------------");


        var candidates = masterTetrominoPrefabs.Where(p => {
            if (p == null) return false;
            int count = p.GetComponentsInChildren<BlockUnit>().Length;
            return count >= levelDef.minBlocks && count <= levelDef.maxBlocks;
        }).ToList();

        if (candidates.Count == 0)
        {
            Debug.LogWarning("ɸѡ��������ѡ�б�Ϊ�գ�δ���ҵ��κη��������� Tetromino��");
            Debug.Log("====== ����� Tetromino ʧ�� ======");
            return null;
        }

        var chosenPrefab = candidates[Random.Range(0, candidates.Count)];
        activeTetrominoPool.Add(chosenPrefab);

        // --- �����������־ 3/3��---
        Debug.Log($"ɸѡ�ɹ�����ѡ��������: {candidates.Count}�������ѡ�����: [{chosenPrefab.name}]");
        Debug.Log($"��ǰ��Ծ���з�������: {activeTetrominoPool.Count}");
        Debug.Log("====== ����� Tetromino �ɹ� ======");

        return chosenPrefab;
    }

    // (�������з�������һ����ȫ��ͬ)
    #region Unchanged Code
    public void ActivateReplicator(int count)
    {
        if (nextTetrominoPrefab != null)
        {
            replicationPrefab = nextTetrominoPrefab;
            replicationCount = count - 1;
        }
    }

    public void InitializeForNewGame(GameSettings gameSettings)
    {
        this.settings = gameSettings;
        activeTetrominoPool = new List<GameObject>(initialTetrominoPrefabs);
        replicationCount = 0;
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

    private void PrepareNextTetromino()
    {
        if (replicationCount > 0 && replicationPrefab != null)
        {
            nextTetrominoPrefab = replicationPrefab;
            replicationCount--;
            if (replicationCount == 0) replicationPrefab = null;
        }
        else
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
    #endregion
}

