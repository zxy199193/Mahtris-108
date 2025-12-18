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
    private bool isFirstBlockOfRound = true;

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

    public void InitializeForNewGame(GameSettings gameSettings, List<GameObject> initialPrefabs)
    {
        this.settings = gameSettings;
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
        isFirstBlockOfRound = true;
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
        bool blockSelected = false;

        // 1. 【攻击的巨人】逻辑
        if (isFirstBlockOfRound && GameManager.Instance.isAttackOnGiantActive)
        {
            var giant = masterTetrominoPrefabs.FirstOrDefault(p => p.name == "T15-Giant");
            if (giant != null) { nextTetrominoPrefab = giant; blockSelected = true; }
        }

        // 2. 【方尖塔】逻辑
        if (!blockSelected && forcedNextBlock != null)
        {
            nextTetrominoPrefab = forcedNextBlock; forcedNextBlock = null; replicationCount = 0; replicationPrefab = null; blockSelected = true;
        }

        // 3. 【复制器】逻辑
        else if (!blockSelected && replicationCount > 0 && replicationPrefab != null)
        {
            nextTetrominoPrefab = replicationPrefab; replicationCount--; if (replicationCount == 0) replicationPrefab = null; blockSelected = true;
        }

        // 4. 【常规/儿童餐/漏斗/超算力】逻辑
        if (!blockSelected)
        {
            if (activeTetrominoPool.Count > 0)
            {
                // A. 【儿童餐】逻辑 (优先级最高，强制 Lv1)
                if (GameManager.Instance.IsKidsMealActive())
                {
                    var lv1Options = activeTetrominoPool.Where(p => p.GetComponentsInChildren<BlockUnit>(true).Length <= 3).ToList();
                    if (lv1Options.Count > 0)
                        nextTetrominoPrefab = lv1Options[Random.Range(0, lv1Options.Count)];
                    else
                        nextTetrominoPrefab = masterTetrominoPrefabs.Where(p => p.name.StartsWith("T1-") || p.name.StartsWith("T2-") || p.name.StartsWith("T3-")).OrderBy(x => Random.value).FirstOrDefault();
                }
                // B. 【漏斗】逻辑 (其次，屏蔽 Lv3)
                // 注意：漏斗屏蔽了 Lv3，所以超算力在这里不生效是正确的
                else if (GameManager.Instance.isFilterActive)
                {
                    var filteredPool = activeTetrominoPool.Where(p => !p.name.StartsWith("T5-")).ToList();

                    if (filteredPool.Count > 0)
                    {
                        nextTetrominoPrefab = filteredPool[Random.Range(0, filteredPool.Count)];
                    }
                    else
                    {
                        var backupPool = masterTetrominoPrefabs.Where(p => !p.name.StartsWith("T5-")).ToList();
                        if (backupPool.Count > 0)
                            nextTetrominoPrefab = backupPool[Random.Range(0, backupPool.Count)];
                        else
                            nextTetrominoPrefab = activeTetrominoPool[Random.Range(0, activeTetrominoPool.Count)];
                    }
                }
                // C. 【正常随机】(在此处加入 超算力 逻辑)
                else
                {
                    // 【新增】超算力：Lv3 权重翻倍 (x3)
                    if (GameManager.Instance.isChaoSuanLiActive)
                    {
                        // 将池子分为 Lv3 和 非Lv3
                        var lv3Blocks = activeTetrominoPool.Where(p => p.name.StartsWith("T5-")).ToList();
                        var otherBlocks = activeTetrominoPool.Where(p => !p.name.StartsWith("T5-")).ToList();

                        // 如果池中没有 Lv3 或全是 Lv3，加权没有意义，直接随机
                        if (lv3Blocks.Count == 0 || otherBlocks.Count == 0)
                        {
                            nextTetrominoPrefab = activeTetrominoPool[Random.Range(0, activeTetrominoPool.Count)];
                        }
                        else
                        {
                            // 计算权重
                            // Lv3 权重 = 数量 * 3 (增加200%)
                            // 其他 权重 = 数量 * 1
                            float wLv3 = lv3Blocks.Count * 3f;
                            float wOther = otherBlocks.Count * 1f;
                            float totalWeight = wLv3 + wOther;

                            float roll = Random.Range(0f, totalWeight);

                            if (roll < wLv3)
                            {
                                // 选中 Lv3 组
                                nextTetrominoPrefab = lv3Blocks[Random.Range(0, lv3Blocks.Count)];
                            }
                            else
                            {
                                // 选中其他组
                                nextTetrominoPrefab = otherBlocks[Random.Range(0, otherBlocks.Count)];
                            }
                        }
                    }
                    else
                    {
                        // 正常随机
                        nextTetrominoPrefab = activeTetrominoPool[Random.Range(0, activeTetrominoPool.Count)];
                    }
                }
            }
            else
            {
                GameEvents.TriggerGameOver();
                return;
            }
        }

        // --- 公共收尾逻辑 ---
        isFirstBlockOfRound = false;

        if (nextTetrominoPrefab == null) { GameEvents.TriggerGameOver(); return; }

        int tilesNeeded = nextTetrominoPrefab.GetComponentsInChildren<BlockUnit>(true).Length;
        nextTileIds = blockPool.PeekBlockIDs(tilesNeeded);

        int passportSuit = GameManager.Instance.GetActivePassportSuit();

        if (passportSuit != -1)
        {
            nextTileIds = blockPool.PeekPreferredSuitIDs(tilesNeeded, passportSuit);
        }
        else
        {
            nextTileIds = blockPool.PeekBlockIDs(tilesNeeded);
        }

        if (nextTileIds == null) { GameEvents.TriggerGameOver(); return; }

        GameEvents.TriggerNextBlockReady(nextTetrominoPrefab, nextTileIds);
    }

    public void SpawnBlock()
    {
        GameManager.Instance.NotifyBlockSpawned();
        if (nextTetrominoPrefab == null) { GameEvents.TriggerGameOver(); return; }
        int tilesNeeded = nextTetrominoPrefab.GetComponentsInChildren<BlockUnit>(true).Length;
        if (!blockPool.HasEnoughBlocks(tilesNeeded))
        {
            Debug.Log("【Game Over】牌库枯竭！无法生成下一个方块。");
            GameEvents.TriggerGameOver();
            return;
        }
        bool removeSuccess = blockPool.RemoveSpecificBlockIds(nextTileIds);

        if (!removeSuccess)
        {
            Debug.LogError("SpawnBlock 移除指定牌失败，回退到普通抽取");
            blockPool.GetBlockIds(tilesNeeded);
        }
        Vector3 spawnPosition = new Vector3(settings.gridWidth / 2, settings.gridHeight - 2, 0);
        GameObject blockGO = Instantiate(nextTetrominoPrefab, spawnPosition, Quaternion.identity);
        var tetromino = blockGO.GetComponent<Tetromino>();
        tetromino.Initialize(settings, tetrisGrid);

        var blockUnits = blockGO.GetComponentsInChildren<BlockUnit>();
        var sortedBlockUnits = blockUnits.OrderBy(bu => bu.gameObject.name).ToArray();
        for (int i = 0; i < sortedBlockUnits.Length && i < nextTileIds.Count; i++)
        {
            sortedBlockUnits[i].Initialize(nextTileIds[i], blockPool);
        }
        PrepareNextTetromino();
    }

    public bool TransformNextBlock()
    {
        if (nextTetrominoPrefab == null) return false;

        int currentBlockCount = nextTetrominoPrefab.GetComponentsInChildren<BlockUnit>().Length;

        var potentialTransformations = masterTetrominoPrefabs
            .Where(p => p.GetComponentsInChildren<BlockUnit>().Length == currentBlockCount && p.name != nextTetrominoPrefab.name)
            .ToList();

        if (potentialTransformations.Count > 0)
        {
            nextTetrominoPrefab = potentialTransformations[Random.Range(0, potentialTransformations.Count)];
            GameEvents.TriggerNextBlockReady(nextTetrominoPrefab, nextTileIds);
            Debug.Log($"变形成功，下一个方块已变为: {nextTetrominoPrefab.name}");
            return true;
        }

        Debug.Log("变形失败，没有找到相同块数的其他形状。");
        return false;
    }

    public bool ForceNextBlock(string prefabName)
    {
        var prefab = masterTetrominoPrefabs.FirstOrDefault(p => p.name == prefabName);

        if (prefab == null)
        {
            Debug.LogError($"[Spawner] 无法找到名为 '{prefabName}' 的预制件！");
            return false;
        }

        forcedNextBlock = prefab;
        PrepareNextTetromino();
        return true;
    }

    public void ForceRerollToLevel(int levelIndex)
    {
        PrepareNextTetromino();
    }

    public void AddRandomLevel3Block()
    {
        var lv3 = masterTetrominoPrefabs.Where(p => p.name.StartsWith("T5-")).OrderBy(x => Random.value).FirstOrDefault();
        if (lv3 != null) AddTetrominoToPool(lv3);
    }

    public void RandomizeActivePool()
    {
        if (activeTetrominoPool == null || masterTetrominoPrefabs == null) return;

        int count = activeTetrominoPool.Count;
        activeTetrominoPool.Clear();

        var masterList = masterTetrominoPrefabs.ToList();
        if (masterList.Count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                activeTetrominoPool.Add(masterList[Random.Range(0, masterList.Count)]);
            }
        }
        GameManager.Instance.UpdateActiveBlockListUI();
    }

    public void ForceRerollIfLevel3()
    {
        if (nextTetrominoPrefab == null) return;

        if (nextTetrominoPrefab.name.StartsWith("T5-"))
        {
            var nonLv3Pool = activeTetrominoPool.Where(p => !p.name.StartsWith("T5-")).ToList();

            if (nonLv3Pool.Count == 0)
            {
                nonLv3Pool = masterTetrominoPrefabs.Where(p => !p.name.StartsWith("T5-")).ToList();
            }

            if (nonLv3Pool.Count > 0)
            {
                nextTetrominoPrefab = nonLv3Pool[Random.Range(0, nonLv3Pool.Count)];
                int tilesNeeded = nextTetrominoPrefab.GetComponentsInChildren<BlockUnit>(true).Length;
                if (blockPool != null)
                {
                    nextTileIds = blockPool.GetBlockIds(tilesNeeded);
                }
                GameEvents.TriggerNextBlockReady(nextTetrominoPrefab, nextTileIds);
                Debug.Log("漏斗生效：已将当前预览的 Lv3 方块替换为 " + nextTetrominoPrefab.name);
            }
        }
    }

    public void RefreshPreviewUI()
    {
        if (nextTetrominoPrefab == null) return;
        int tilesNeeded = nextTetrominoPrefab.GetComponentsInChildren<BlockUnit>(true).Length;
        nextTileIds = blockPool.PeekBlockIDs(tilesNeeded);
        GameEvents.TriggerNextBlockReady(nextTetrominoPrefab, nextTileIds);
    }

    public List<int> GetNextTetrominoTileIDs()
    {
        if (nextTileIds != null)
        {
            return new List<int>(nextTileIds);
        }
        return new List<int>();
    }

    public int GetNextBlockRequiredTileCount()
    {
        if (nextTetrominoPrefab == null) return 0;
        return nextTetrominoPrefab.GetComponentsInChildren<BlockUnit>(true).Length;
    }

    public bool RemoveHighestMultiplierBlock()
    {
        if (activeTetrominoPool == null || activeTetrominoPool.Count <= 1)
        {
            Debug.Log("剪刀失效：池中方块不足（至少保留一个）。");
            return false;
        }

        float maxMult = -1f;
        GameObject target = null;

        foreach (var prefab in activeTetrominoPool)
        {
            var tet = prefab.GetComponent<Tetromino>();
            if (tet != null)
            {
                if (tet.extraMultiplier > maxMult)
                {
                    maxMult = tet.extraMultiplier;
                    target = prefab;
                }
            }
        }

        if (target != null)
        {
            activeTetrominoPool.Remove(target);
            Debug.Log($"剪刀生效：移除了 {target.name} (倍率: {maxMult})");
            GameManager.Instance.UpdateActiveBlockListUI();

            if (nextTetrominoPrefab == target)
            {
                Debug.Log("剪刀：预览方块即为被删方块，正在替换...");
                PrepareNextTetromino();
                GameEvents.TriggerNextBlockReady(nextTetrominoPrefab, nextTileIds);
            }
            return true;
        }

        return false;
    }

    public int GetUniqueBlockCount()
    {
        if (activeTetrominoPool == null) return 0;
        return activeTetrominoPool.Distinct().Count();
    }

    public void RefreshPreviewTilesOnly()
    {
        if (nextTetrominoPrefab == null) return;

        int tilesNeeded = nextTetrominoPrefab.GetComponentsInChildren<BlockUnit>(true).Length;
        int passportSuit = GameManager.Instance.GetActivePassportSuit();

        if (passportSuit != -1)
        {
            nextTileIds = blockPool.PeekPreferredSuitIDs(tilesNeeded, passportSuit);
        }
        else
        {
            nextTileIds = blockPool.PeekBlockIDs(tilesNeeded);
        }
        GameEvents.TriggerNextBlockReady(nextTetrominoPrefab, nextTileIds);
    }
}