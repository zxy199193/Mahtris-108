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
    public IEnumerable<GameObject> GetMasterList() => masterTetrominoPrefabs;

    private List<GameObject> activeTetrominoPool;
    private GameSettings settings;
    private GameObject nextTetrominoPrefab;
    private List<int> nextTileIds;

    private int replicationCount = 0;
    private GameObject replicationPrefab = null;
    public GameObject[] GetInitialTetrominoPrefabs()
    {
        return initialTetrominoPrefabs;
    }
    public void ActivateReplicator(int count)
    {
        if (nextTetrominoPrefab != null)
        {
            replicationPrefab = nextTetrominoPrefab;
            replicationCount = count - 1;
        }
    }

    // ���޸ġ�����ǩ�������� List<GameObject> initialPrefabs ����
    public void InitializeForNewGame(GameSettings gameSettings, List<GameObject> initialPrefabs)
    {
        this.settings = gameSettings;
        // ���޸ġ�ʹ�ô���ķ����б������ǹ̶��� initialTetrominoPrefabs
        activeTetrominoPool = new List<GameObject>(initialPrefabs);
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

    public void AddTetrominoToPool(GameObject prefab)
    {
        if (prefab != null)
        {
            activeTetrominoPool.Add(prefab);
            GameManager.Instance.UpdateActiveBlockListUI();
            Debug.Log($"�·��� [{prefab.name}] ����ӵ���Ծ�ء���ǰ������ {activeTetrominoPool.Count} �����顣");
        }
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
            else
            {
                // �����Ծ��Ϊ�գ���̫���ܷ���������Ϊ���գ�
                GameEvents.TriggerGameOver();
                return;
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

        Vector3 spawnPosition = new Vector3(settings.gridWidth / 2, settings.gridHeight - 2, 0);
        GameObject blockGO = Instantiate(nextTetrominoPrefab, spawnPosition, Quaternion.identity);
        var tetromino = blockGO.GetComponent<Tetromino>();
        tetromino.Initialize(settings, tetrisGrid);

        var blockUnits = blockGO.GetComponentsInChildren<BlockUnit>();
        var sortedBlockUnits = blockUnits.OrderBy(bu => bu.gameObject.name).ToArray();
        for (int i = 0; i < sortedBlockUnits.Length && i < nextTileIds.Count; i++)
        {
            sortedBlockUnits[i].Initialize(nextTileIds[i], blockPool); // ���޸ġ�ʹ������������
        }
        PrepareNextTetromino();
    }
    // �����������������ߵĺ����߼�
    public bool TransformNextBlock()
    {
        if (nextTetrominoPrefab == null) return false;

        // 1. ���㵱ǰ����һ�����顱�ж��ٸ�������
        int currentBlockCount = nextTetrominoPrefab.GetComponentsInChildren<BlockUnit>().Length;

        // 2. �����з���ĸ�б��У��ҳ���������������ͬ�������ֲ�ͬ�ķ���
        var potentialTransformations = masterTetrominoPrefabs
            .Where(p => p.GetComponentsInChildren<BlockUnit>().Length == currentBlockCount && p.name != nextTetrominoPrefab.name)
            .ToList();

        // 3. ����ҵ��˿��Ա��ε�Ŀ��
        if (potentialTransformations.Count > 0)
        {
            // ���ѡ��һ������״���滻
            nextTetrominoPrefab = potentialTransformations[Random.Range(0, potentialTransformations.Count)];

            // 4. ���´����¼�����UI����Ϊ����״��Ԥ��
            // �齫��ID(nextTileIds)����Ҫ�䣬��Ϊ������ͬ
            GameEvents.TriggerNextBlockReady(nextTetrominoPrefab, nextTileIds);

            Debug.Log($"���γɹ�����һ�������ѱ�Ϊ: {nextTetrominoPrefab.name}");
            return true; // ʹ�óɹ�
        }

        Debug.Log("����ʧ�ܣ�û���ҵ���ͬ������������״��");
        return false; // ʹ��ʧ�ܣ����߲�����
    }
}