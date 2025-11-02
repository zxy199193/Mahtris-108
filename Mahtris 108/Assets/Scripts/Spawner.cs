// FileName: Spawner.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Spawner : MonoBehaviour
{
    [Header("Tetromino 预制件列表")]
    [Tooltip("游戏开始时使用的Tetromino")]
    [SerializeField] private GameObject[] initialTetrominoPrefabs;
    [Tooltip("所有可能出现的Tetromino（用于胡牌后随机抽取）")]
    [SerializeField] private GameObject[] masterTetrominoPrefabs;

    [Header("模块引用")]
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
    private GameObject forcedNextBlock = null;
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

    // 【修改】方法签名增加了 List<GameObject> initialPrefabs 参数
    public void InitializeForNewGame(GameSettings gameSettings, List<GameObject> initialPrefabs)
    {
        this.settings = gameSettings;
        // 【修改】使用传入的方块列表，而不是固定的 initialTetrominoPrefabs
        activeTetrominoPool = new List<GameObject>(initialPrefabs);
        replicationCount = 0;
        replicationPrefab = null;

        if (activeTetrominoPool == null || activeTetrominoPool.Count == 0)
        {
            Debug.LogError("Spawner中没有配置任何初始Tetromino！");
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
            Debug.Log($"新方块 [{prefab.name}] 已添加到活跃池。当前池中有 {activeTetrominoPool.Count} 个方块。");
        }
    }

    private void PrepareNextTetromino()
    {
        if (forcedNextBlock != null)
        {
            nextTetrominoPrefab = forcedNextBlock;
            forcedNextBlock = null; // 立即消耗
            replicationCount = 0; // 强制覆盖“复制器”的效果
            replicationPrefab = null;
        }
        else if (replicationCount > 0 && replicationPrefab != null)
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
                // 如果活跃池为空（不太可能发生，但作为保险）
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
        // 【新增】通知GameManager一个方块已被生成，用于喷气背包计数
        GameManager.Instance.NotifyBlockSpawned();
        if (nextTetrominoPrefab == null) { GameEvents.TriggerGameOver(); return; }

        Vector3 spawnPosition = new Vector3(settings.gridWidth / 2, settings.gridHeight - 2, 0);
        GameObject blockGO = Instantiate(nextTetrominoPrefab, spawnPosition, Quaternion.identity);
        var tetromino = blockGO.GetComponent<Tetromino>();
        tetromino.Initialize(settings, tetrisGrid);

        var blockUnits = blockGO.GetComponentsInChildren<BlockUnit>();
        var sortedBlockUnits = blockUnits.OrderBy(bu => bu.gameObject.name).ToArray();
        for (int i = 0; i < sortedBlockUnits.Length && i < nextTileIds.Count; i++)
        {
            sortedBlockUnits[i].Initialize(nextTileIds[i], blockPool); // 【修改】使用排序后的数组
        }
        PrepareNextTetromino();
    }
    // 【新增】变形器道具的核心逻辑
    public bool TransformNextBlock()
    {
        if (nextTetrominoPrefab == null) return false;

        // 1. 计算当前“下一个方块”有多少个基础块
        int currentBlockCount = nextTetrominoPrefab.GetComponentsInChildren<BlockUnit>().Length;

        // 2. 从所有方块母列表中，找出所有与它块数相同，但名字不同的方块
        var potentialTransformations = masterTetrominoPrefabs
            .Where(p => p.GetComponentsInChildren<BlockUnit>().Length == currentBlockCount && p.name != nextTetrominoPrefab.name)
            .ToList();

        // 3. 如果找到了可以变形的目标
        if (potentialTransformations.Count > 0)
        {
            // 随机选择一个新形状并替换
            nextTetrominoPrefab = potentialTransformations[Random.Range(0, potentialTransformations.Count)];

            // 4. 重新触发事件，让UI更新为新形状的预览
            // 麻将牌ID(nextTileIds)不需要变，因为块数相同
            GameEvents.TriggerNextBlockReady(nextTetrominoPrefab, nextTileIds);

            Debug.Log($"变形成功，下一个方块已变为: {nextTetrominoPrefab.name}");
            return true; // 使用成功
        }

        Debug.Log("变形失败，没有找到相同块数的其他形状。");
        return false; // 使用失败，道具不消耗
    }
    public bool ForceNextBlock(string prefabName)
    {
        // 1. 从 MasterList 查找预制件
        var prefab = masterTetrominoPrefabs.FirstOrDefault(p => p.name == prefabName);

        if (prefab == null)
        {
            Debug.LogError($"[Spawner] 无法找到名为 '{prefabName}' 的预制件！");
            return false; // 道具使用失败
        }

        // 2. 设置强制标记
        forcedNextBlock = prefab;

        // 3. 立即重新准备下一个方块，以便UI可以更新
        PrepareNextTetromino();

        return true; // 道具使用成功
    }
}