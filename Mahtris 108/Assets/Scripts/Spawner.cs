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
        isFirstBlockOfRound = true; // 【新增】重置标记
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

        // 4. 【常规/儿童餐/漏斗】逻辑
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
                else if (GameManager.Instance.isFilterActive)
                {
                    // 【修复】将漏斗逻辑移入 pool.Count > 0 的分支内
                    var filteredPool = activeTetrominoPool.Where(p => !p.name.StartsWith("T5-")).ToList();

                    if (filteredPool.Count > 0)
                    {
                        nextTetrominoPrefab = filteredPool[Random.Range(0, filteredPool.Count)];
                    }
                    else
                    {
                        // 如果活跃池全是 Lv3，尝试从 MasterList 补充非 Lv3
                        var backupPool = masterTetrominoPrefabs.Where(p => !p.name.StartsWith("T5-")).ToList();
                        if (backupPool.Count > 0)
                            nextTetrominoPrefab = backupPool[Random.Range(0, backupPool.Count)];
                        else
                            // 实在没办法（MasterList也全是Lv3），只能随机
                            nextTetrominoPrefab = activeTetrominoPool[Random.Range(0, activeTetrominoPool.Count)];
                    }
                }
                // C. 【正常随机】
                else
                {
                    nextTetrominoPrefab = activeTetrominoPool[Random.Range(0, activeTetrominoPool.Count)];
                }
            }
            else
            {
                // 池为空，通常意味着游戏初始化失败或配置错误
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
            // 如果有护照，优先拿指定花色
            nextTileIds = blockPool.PeekPreferredSuitIDs(tilesNeeded, passportSuit);
        }
        else
        {
            // 正常逻辑
            nextTileIds = blockPool.PeekBlockIDs(tilesNeeded);
        }


        if (nextTileIds == null) { GameEvents.TriggerGameOver(); return; }

        GameEvents.TriggerNextBlockReady(nextTetrominoPrefab, nextTileIds);
    }

    public void SpawnBlock()
    {
        // 【新增】通知GameManager一个方块已被生成，用于喷气背包计数
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
            // 防御性编程：理论上不应发生，除非 Prepare 和 Spawn 之间牌库被改了
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
    public void ForceRerollToLevel(int levelIndex)
    {
        // 立即重新运行 PrepareNextTetromino，它会检测 KidsMeal 状态
        PrepareNextTetromino();
        // 如果当前方块的麻将牌数量变了，需要重新生成ID
        // PrepareNextTetromino 内部已经处理了 ID 生成，所以这里只需要通知UI
        // 但要注意：PrepareNextTetromino 会触发 TriggerNextBlockReady 事件
    }

    public void AddRandomLevel3Block()
    {
        var lv3 = masterTetrominoPrefabs.Where(p => p.name.StartsWith("T5-")).OrderBy(x => Random.value).FirstOrDefault();
        if (lv3 != null) AddTetrominoToPool(lv3);
    }
    // 【新增】大革命条约：重新随机活跃池
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
        // 如果当前没有下一个方块，直接返回
        if (nextTetrominoPrefab == null) return;

        // 检查当前预览的是否是 Lv3 (假设命名以 T5- 开头)
        if (nextTetrominoPrefab.name.StartsWith("T5-"))
        {
            // 1. 从活跃池中筛选非 Lv3 方块
            var nonLv3Pool = activeTetrominoPool.Where(p => !p.name.StartsWith("T5-")).ToList();

            // 2. 如果活跃池里全是 Lv3（极少见），则从总表 MasterList 里找非 Lv3 进行保底
            if (nonLv3Pool.Count == 0)
            {
                nonLv3Pool = masterTetrominoPrefabs.Where(p => !p.name.StartsWith("T5-")).ToList();
            }

            // 3. 执行替换
            if (nonLv3Pool.Count > 0)
            {
                // 随机选一个新的
                nextTetrominoPrefab = nonLv3Pool[Random.Range(0, nonLv3Pool.Count)];

                // 【关键步骤】重新生成麻将牌数据
                // 因为 Lv3 方块通常有5个格子，而 Lv1/Lv2 只有3或4个
                // 如果不重新生成 IDs，会导致数组越界或数据不匹配
                int tilesNeeded = nextTetrominoPrefab.GetComponentsInChildren<BlockUnit>(true).Length;

                // 注意：这里假设 blockPool 引用在 Spawner 中是可用的
                if (blockPool != null)
                {
                    nextTileIds = blockPool.GetBlockIds(tilesNeeded);
                }

                // 【关键步骤】立即通知 UI 刷新预览区域
                GameEvents.TriggerNextBlockReady(nextTetrominoPrefab, nextTileIds);

                Debug.Log("漏斗生效：已将当前预览的 Lv3 方块替换为 " + nextTetrominoPrefab.name);
            }
        }
    }
    // 【新增】供 GameManager 在消行后调用，刷新预览状态
    // 如果之前显示黑块，消行后牌够了，这里会把黑块变回正常牌
    public void RefreshPreviewUI()
    {
        if (nextTetrominoPrefab == null) return;

        int tilesNeeded = nextTetrominoPrefab.GetComponentsInChildren<BlockUnit>(true).Length;

        // 再次偷看牌库
        nextTileIds = blockPool.PeekBlockIDs(tilesNeeded);

        // 通知 UI 刷新
        GameEvents.TriggerNextBlockReady(nextTetrominoPrefab, nextTileIds);
    }
    public List<int> GetNextTetrominoTileIDs()
    {
        // 【修正】直接返回缓存的 nextTileIds 即可
        // 这个列表在 PrepareNextTetromino 中已经被赋值了
        if (nextTileIds != null)
        {
            return new List<int>(nextTileIds);
        }

        return new List<int>();
    }
    public int GetNextBlockRequiredTileCount()
    {
        // nextTetrominoPrefab 是当前预览的方块
        if (nextTetrominoPrefab == null) return 0;

        // 计算该预制体里有多少个 BlockUnit
        return nextTetrominoPrefab.GetComponentsInChildren<BlockUnit>(true).Length;
    }
    public bool RemoveHighestMultiplierBlock()
    {
        // 1. 【核心修复】如果池为空，或者只剩 1 个方块，禁止使用
        if (activeTetrominoPool == null || activeTetrominoPool.Count <= 1)
        {
            Debug.Log("剪刀失效：池中方块不足（至少保留一个）。");
            return false; // 使用失败，不消耗道具
        }

        // 2. 找到当前池子里倍率最高的方块
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

        // 3. 移除它
        if (target != null)
        {
            activeTetrominoPool.Remove(target);
            Debug.Log($"剪刀生效：移除了 {target.name} (倍率: {maxMult})");

            // 刷新 UI 列表
            GameManager.Instance.UpdateActiveBlockListUI();

            // 如果下一个预览的方块正好是被删掉的这个，必须重随
            if (nextTetrominoPrefab == target)
            {
                Debug.Log("剪刀：预览方块即为被删方块，正在替换...");
                PrepareNextTetromino();
                GameEvents.TriggerNextBlockReady(nextTetrominoPrefab, nextTileIds);
            }

            return true; // 使用成功，消耗道具
        }

        return false;
    }
    public int GetUniqueBlockCount()
    {
        if (activeTetrominoPool == null) return 0;
        // 使用 Distinct() 来统计不重复的预制件种类
        return activeTetrominoPool.Distinct().Count();
    }
    public void RefreshPreviewTilesOnly()
    {
        if (nextTetrominoPrefab == null) return;

        // 强制重新运行 ID 获取逻辑 (PrepareNextTetromino 里已经写好了护照判断)
        // 但我们需要避免重新随机形状，所以只跑 ID 获取部分

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

        // 刷新 UI
        GameEvents.TriggerNextBlockReady(nextTetrominoPrefab, nextTileIds);
    }
}