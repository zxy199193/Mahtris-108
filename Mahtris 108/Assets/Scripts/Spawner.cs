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

    private List<GameObject> activeTetrominoPool;
    private GameSettings settings;
    private GameObject nextTetrominoPrefab;
    private List<int> nextTileIds;

    private int replicationCount = 0;
    private GameObject replicationPrefab = null;

    public GameObject AddRandomTetrominoOfLevel(int levelIndex)
    {
        // --- 【新增诊断日志 1/3】---
        Debug.Log("====== 开始添加新 Tetromino ======");

        if (levelIndex < 0 || levelIndex >= settings.tetrominoLevels.Count)
        {
            Debug.LogError($"错误的等级索引: {levelIndex}");
            return null;
        }

        var levelDef = settings.tetrominoLevels[levelIndex];

        // --- 【新增诊断日志 2/3】---
        Debug.Log($"筛选条件: 等级='{levelDef.levelName}', 方块数范围=[{levelDef.minBlocks}, {levelDef.maxBlocks}]");
        Debug.Log("--- 检查 Master List 中的所有方块 ---");
        foreach (var p in masterTetrominoPrefabs)
        {
            if (p != null)
                Debug.Log($"-> 检查到: {p.name}, 它包含的 BlockUnit 数量: {p.GetComponentsInChildren<BlockUnit>().Length}");
        }
        Debug.Log("------------------------------------");


        var candidates = masterTetrominoPrefabs.Where(p => {
            if (p == null) return false;
            int count = p.GetComponentsInChildren<BlockUnit>().Length;
            return count >= levelDef.minBlocks && count <= levelDef.maxBlocks;
        }).ToList();

        if (candidates.Count == 0)
        {
            Debug.LogWarning("筛选结束，候选列表为空！未能找到任何符合条件的 Tetromino。");
            Debug.Log("====== 添加新 Tetromino 失败 ======");
            return null;
        }

        var chosenPrefab = candidates[Random.Range(0, candidates.Count)];
        activeTetrominoPool.Add(chosenPrefab);

        // --- 【新增诊断日志 3/3】---
        Debug.Log($"筛选成功！候选方块数量: {candidates.Count}。已随机选择并添加: [{chosenPrefab.name}]");
        Debug.Log($"当前活跃池中方块总数: {activeTetrominoPool.Count}");
        Debug.Log("====== 添加新 Tetromino 成功 ======");

        return chosenPrefab;
    }

    // (其余所有方法与上一版完全相同)
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

