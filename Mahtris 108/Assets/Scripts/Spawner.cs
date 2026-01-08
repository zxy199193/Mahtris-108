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

    // 【新增】防重复机制变量
    private string lastSpawnedBlockName = "";
    private int consecutiveCount = 0;

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

        // 【新增】重置防重复计数
        lastSpawnedBlockName = "";
        consecutiveCount = 0;

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

    // 【核心修改】准备下一个方块
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

        // 4. 【常规/儿童餐/漏斗/超算力 + 防重复】逻辑
        if (!blockSelected)
        {
            if (activeTetrominoPool.Count > 0)
            {
                // A. 【儿童餐】逻辑 (优先级最高，强制 Lv1)
                if (GameManager.Instance.IsKidsMealActive())
                {
                    var lv1Options = activeTetrominoPool.Where(p => p.GetComponentsInChildren<BlockUnit>(true).Length <= 3).ToList();

                    if (lv1Options.Count > 0)
                        nextTetrominoPrefab = GetWeightedRandomBlock(lv1Options); // 使用加权随机
                    else
                        // 如果池里没有Lv1，从总表中随机补一个 (这里也可以用加权，但通常不需要)
                        nextTetrominoPrefab = masterTetrominoPrefabs.Where(p => p.name.StartsWith("T1-") || p.name.StartsWith("T2-") || p.name.StartsWith("T3-")).OrderBy(x => Random.value).FirstOrDefault();
                }
                // B. 【漏斗】逻辑 (其次，屏蔽 Lv3)
                else if (GameManager.Instance.isFilterActive)
                {
                    var filteredPool = activeTetrominoPool.Where(p => !p.name.StartsWith("T5-")).ToList();

                    if (filteredPool.Count > 0)
                    {
                        nextTetrominoPrefab = GetWeightedRandomBlock(filteredPool); // 使用加权随机
                    }
                    else
                    {
                        // 保底逻辑
                        var backupPool = masterTetrominoPrefabs.Where(p => !p.name.StartsWith("T5-")).ToList();
                        if (backupPool.Count > 0)
                            nextTetrominoPrefab = backupPool[Random.Range(0, backupPool.Count)];
                        else
                            nextTetrominoPrefab = GetWeightedRandomBlock(activeTetrominoPool);
                    }
                }
                // C. 【正常逻辑】(包含 超算力 和 防重复)
                else
                {
                    nextTetrominoPrefab = GetWeightedRandomBlock(activeTetrominoPool);
                }
            }
            else
            {
                GameManager.Instance.TriggerGameOver("GAME_OVER_NO_BLOCK");
                return;
            }
        }

        // --- 公共收尾逻辑 ---
        isFirstBlockOfRound = false;

        if (nextTetrominoPrefab == null)
        {
            GameManager.Instance.TriggerGameOver("GAME_OVER_NO_BLOCK");
            return;
        }
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

        if (nextTileIds == null)
        {
            GameManager.Instance.TriggerGameOver("GAME_OVER_NO_BLOCK");
            return;
        }
        GameEvents.TriggerNextBlockReady(nextTetrominoPrefab, nextTileIds);
    }

    // 【新增】核心加权随机方法
    // 同时处理：1. 超算力 (Lv3 x3)  2. 防重复 (1/2, 1/4...)
    private GameObject GetWeightedRandomBlock(List<GameObject> candidates)
    {
        if (candidates == null || candidates.Count == 0) return null;
        if (candidates.Count == 1) return candidates[0];

        Dictionary<GameObject, float> weights = new Dictionary<GameObject, float>();
        float totalWeight = 0f;

        bool isChaoSuanLi = GameManager.Instance.isChaoSuanLiActive;

        foreach (var block in candidates)
        {
            float w = 1f;

            // 1. 应用【超算力】权重 (Lv3 权重翻3倍)
            if (isChaoSuanLi && block.name.StartsWith("T5-"))
            {
                w *= 3f;
            }

            // 2. 应用【防重复】权重 (连续出现 N 次，权重乘 0.5^N)
            if (block.name == lastSpawnedBlockName)
            {
                w *= Mathf.Pow(0.5f, consecutiveCount);
            }

            // 【核心修复】累加权重，而不是覆盖！
            // 之前是 weights[block] = w; 导致多个相同方块被视为1个
            if (weights.ContainsKey(block))
            {
                weights[block] += w;
            }
            else
            {
                weights[block] = w;
            }

            totalWeight += w;
        }

        // 加权抽取
        float randomPoint = Random.Range(0f, totalWeight);
        foreach (var kvp in weights)
        {
            if (randomPoint < kvp.Value)
            {
                return kvp.Key;
            }
            randomPoint -= kvp.Value;
        }

        return candidates.Last(); // 浮点误差保底
    }

    public void SpawnBlock()
    {
        GameManager.Instance.NotifyBlockSpawned();
        if (nextTetrominoPrefab == null)
        {
            GameManager.Instance.TriggerGameOver("GAME_OVER_NO_BLOCK");
            return;
        }
        // 【新增】在生成瞬间，更新历史记录
        if (nextTetrominoPrefab.name == lastSpawnedBlockName)
        {
            consecutiveCount++;
            // Debug.Log($"连续生成检测: {lastSpawnedBlockName} 已连续出现 {consecutiveCount} 次，下次权重将降低。");
        }
        else
        {
            lastSpawnedBlockName = nextTetrominoPrefab.name;
            consecutiveCount = 1;
        }

        int tilesNeeded = nextTetrominoPrefab.GetComponentsInChildren<BlockUnit>(true).Length;
        if (!blockPool.HasEnoughBlocks(tilesNeeded))
        {
            Debug.Log("【Game Over】牌库枯竭！无法生成下一个方块。");
            GameManager.Instance.TriggerGameOver("GAME_OVER_NO_BLOCK");
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
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TryExecutePendingPassportShuffle();
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
                // 【修改】漏斗重随也应用加权随机（防止漏斗也一直出同一个）
                nextTetrominoPrefab = GetWeightedRandomBlock(nonLv3Pool);

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
    public bool RemoveRandomBlock()
    {
        // 1. 保底检查：必须至少有2个方块才能移除 (防止死局)
        if (activeTetrominoPool == null || activeTetrominoPool.Count <= 1)
        {
            Debug.Log("平底锅失效：池中方块不足（至少保留一个）。");
            return false;
        }

        // 2. 随机选择一个索引
        int index = Random.Range(0, activeTetrominoPool.Count);
        GameObject target = activeTetrominoPool[index];

        // 3. 移除
        activeTetrominoPool.RemoveAt(index);
        Debug.Log($"平底锅生效：随机移除了 {target.name}");

        // 4. 刷新 UI 列表
        GameManager.Instance.UpdateActiveBlockListUI();

        // 5. 【关键】如果移除的正好是当前“预览中”的方块，必须立刻重随一个新的
        if (nextTetrominoPrefab == target)
        {
            Debug.Log("平底锅：预览方块即为被删方块，正在重随...");
            PrepareNextTetromino();
            // PrepareNextTetromino 内部会自动调用 TriggerNextBlockReady 更新预览 UI
        }

        return true;
    }
}