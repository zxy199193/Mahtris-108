using System.Collections.Generic;
using UnityEngine;

public class HuPaiArea : MonoBehaviour
{
    public static HuPaiArea Instance { get; private set; }

    [Header("显示相关（在 Inspector 赋值）")]
    [SerializeField] private Transform displayParent;   // 胡牌区容器（空物体）
    [SerializeField] private GameObject blockPrefab;    // 可选：若你希望复制显示而不是移动，可用此 prefab

    // 每一组是 List<int> 三个 blockId（刻子或顺子）
    private List<List<int>> huPaiSets = new List<List<int>>();

    private void Awake()
    {
        if (Instance == null) Instance = this; else Destroy(gameObject);
    }

    // 添加一组 ID（当我们直接移动棋盘上的 Transform 到 displayParent 时也应调用此方法记录ID）
    public void AddHuPaiSetFromIds(List<int> setIds)
    {
        if (setIds == null || setIds.Count != 3) return;
        huPaiSets.Add(new List<int>(setIds));
        Debug.Log($"[HuPaiArea] AddHuPaiSetFromIds total={huPaiSets.Count}");
        // 如果你需要可视化副本而不是移动原物体，可在这里 Instantiate blockPrefab
        RefreshDisplay();
    }

    // 从已有棋盘 Transform 移入（优先使用）。transforms.Count ==3
    // 会把这些 transforms reparent 到 displayParent，并标记为 display-only
    // 如果找不到 displayParent，则会 fallback 到 AddHuPaiSetFromIds + 不移动
    public void AddHuPaiSetFromBoardTransforms(List<Transform> transforms)
    {
        if (transforms == null || transforms.Count != 3) return;
        List<int> ids = new List<int>();
        foreach (var t in transforms)
        {
            var bu = t.GetComponent<BlockUnit>();
            if (bu != null) ids.Add(bu.blockId);
            else ids.Add(-1);
        }

        huPaiSets.Add(new List<int>(ids));
        Debug.Log($"[HuPaiArea] AddHuPaiSetFromBoardTransforms total={huPaiSets.Count}");

        // 如果 displayParent 有设置，则把 transforms 移入父对象并标记为展示
        if (displayParent != null)
        {
            // 简单排列：三张竖排
            float tileSpacing = 1.1f;
            int colIndex = huPaiSets.Count - 1;
            for (int i = 0; i < transforms.Count; i++)
            {
                var t = transforms[i];
                if (t == null) continue;
                var bu = t.GetComponent<BlockUnit>();
                if (bu != null)
                {
                    Vector3 localPos = new Vector3(colIndex * tileSpacing, -i * tileSpacing, 0f);
                    bu.MakeDisplayAndReparent(displayParent, localPos);
                }
                else
                {
                    // 如果没有 BlockUnit，则简单 reparent
                    t.SetParent(displayParent, false);
                }
            }
        }
        else
        {
            // fallback: 直接刷新显示（如果 blockPrefab 可用，会用复制显示）
            RefreshDisplay();
        }
    }

    public int GetHuPaiSetCount() => huPaiSets.Count;
    public List<List<int>> GetAllSets() => huPaiSets;

    public void ClearAll()
    {
        // 清空显示 parent 下的子物体（胡牌区的展示物）
        if (displayParent != null)
        {
            foreach (Transform c in displayParent)
                Destroy(c.gameObject);
        }
        huPaiSets.Clear();
        Debug.Log("[HuPaiArea] ClearAll");
    }

    // 若你想用 prefab 显示（而不是移动棋盘上的原件），RefreshDisplay 会把 huPaiSets 显示出来（会删除 displayParent 下所有子物体）
    public void RefreshDisplay()
    {
        if (displayParent == null || blockPrefab == null) return;

        foreach (Transform c in displayParent) Destroy(c.gameObject);

        int col = 0;
        float tileSpacing = 1.1f;
        foreach (var set in huPaiSets)
        {
            for (int i = 0; i < set.Count; i++)
            {
                GameObject go = Instantiate(blockPrefab, displayParent);
                go.transform.localPosition = new Vector3(col * tileSpacing, -i * tileSpacing, 0f);
                var bu = go.GetComponent<BlockUnit>();
                if (bu != null) bu.SetAsDisplay(set[i]);
            }
            col++;
        }
    }
}
