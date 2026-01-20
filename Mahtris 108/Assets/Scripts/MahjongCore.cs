// FileName: MahjongCore.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DetectionResult
{
    public List<List<int>> Kongs { get; set; } = new List<List<int>>();
    public List<List<int>> Pungs { get; set; } = new List<List<int>>();
    public List<List<int>> Chows { get; set; } = new List<List<int>>();
    public List<int> RemainingIds { get; set; } = new List<int>();
}

public class HandAnalysisResult
{
    public string PatternName { get; set; } = "HU_TYPE_UNKNOWN";
    public int TotalFan { get; set; } = 0;
    public float BaseMultiplier { get; set; } = 2f;
    public float FanMultiplier => Mathf.Pow(BaseMultiplier, TotalFan);
}

public class MahjongCore
{
    #region 基础拆解逻辑 (保持不变)

    private void FindKongs(List<int> ids, DetectionResult result)
    {
        var counts = ids.GroupBy(id => id % 27).ToDictionary(g => g.Key, g => g.ToList());
        foreach (var group in counts.Values.Where(g => g.Count >= 4))
        {
            var kong = group.Take(4).ToList();
            result.Kongs.Add(kong);
            foreach (var id in kong) ids.Remove(id);
        }
    }

    private void FindPungs(List<int> ids, DetectionResult result)
    {
        var counts = ids.GroupBy(id => id % 27).ToDictionary(g => g.Key, g => g.ToList());
        foreach (var group in counts.Values.Where(g => g.Count >= 3))
        {
            var pung = group.Take(3).ToList();
            result.Pungs.Add(pung);
            foreach (var id in pung) ids.Remove(id);
        }
    }

    private void FindChows(List<int> ids, DetectionResult result)
    {
        var tilesBySuit = ids.GroupBy(id => (id % 27) / 9)
                             .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var suitGroup in tilesBySuit.Values)
        {
            while (true)
            {
                if (suitGroup.Count < 3) break;
                var valueToIds = suitGroup.GroupBy(id => id % 27 % 9)
                                          .ToDictionary(g => g.Key, g => g.ToList());
                bool foundChowThisPass = false;
                for (int i = 0; i <= 6; i++)
                {
                    if (valueToIds.ContainsKey(i) && valueToIds.ContainsKey(i + 1) && valueToIds.ContainsKey(i + 2))
                    {
                        int id1 = valueToIds[i][0];
                        int id2 = valueToIds[i + 1][0];
                        int id3 = valueToIds[i + 2][0];
                        var chow = new List<int> { id1, id2, id3 };
                        result.Chows.Add(chow);
                        suitGroup.Remove(id1); suitGroup.Remove(id2); suitGroup.Remove(id3);
                        ids.Remove(id1); ids.Remove(id2); ids.Remove(id3);
                        foundChowThisPass = true;
                        break;
                    }
                }
                if (!foundChowThisPass) break;
            }
        }
    }

    public DetectionResult DetectSets(List<int> rowIds)
    {
        var result = new DetectionResult();
        var mutableIds = new List<int>(rowIds);
        FindKongs(mutableIds, result);
        FindPungs(mutableIds, result);
        FindChows(mutableIds, result);
        result.RemainingIds = mutableIds;
        return result;
    }

    public List<int> FindPair(List<int> ids)
    {
        return ids.GroupBy(id => id % 27).FirstOrDefault(g => g.Count() >= 2)?.Take(2).ToList();
    }

    private bool IsPungOrKong(List<int> set)
    {
        if (set.Count < 3) return false;
        return set.Select(id => id % 27).Distinct().Count() == 1;
    }

    #endregion

    #region 新增牌型判定方法

    // 【新增】一气通贯：同花色 123, 456, 789
    private bool IsIttsu(List<List<int>> sets)
    {
        bool[,] flags = new bool[3, 3]; // [花色, 顺子类型(0:123, 1:456, 2:789)]

        foreach (var set in sets)
        {
            // 只检查顺子
            if (set.Count != 3 || IsPungOrKong(set)) continue;

            var sorted = set.Select(id => id % 27).OrderBy(v => v).ToList();

            // 检查是否连续
            if (sorted[1] == sorted[0] + 1 && sorted[2] == sorted[0] + 1 + 1)
            {
                int suit = sorted[0] / 9;
                int startNum = sorted[0] % 9;

                if (startNum == 0) flags[suit, 0] = true; // 1-2-3
                if (startNum == 3) flags[suit, 1] = true; // 4-5-6
                if (startNum == 6) flags[suit, 2] = true; // 7-8-9
            }
        }

        // 检查任一花色是否集齐 3 组
        for (int s = 0; s < 3; s++)
        {
            if (flags[s, 0] && flags[s, 1] && flags[s, 2]) return true;
        }
        return false;
    }

    // 【新增】三色同顺：三种花色都有相同的顺子 (如 123筒, 123条, 123万)
    private bool IsSanSeTongShun(List<List<int>> sets)
    {
        // 字典：起始数值(0-8) -> 拥有的花色集合
        Dictionary<int, HashSet<int>> straightStarts = new Dictionary<int, HashSet<int>>();

        foreach (var set in sets)
        {
            // 只检查顺子
            if (set.Count != 3 || IsPungOrKong(set)) continue;

            var sorted = set.Select(id => id % 27).OrderBy(v => v).ToList();
            if (sorted[1] == sorted[0] + 1 && sorted[2] == sorted[0] + 1 + 1)
            {
                int suit = sorted[0] / 9;
                int startNum = sorted[0] % 9;

                if (!straightStarts.ContainsKey(startNum))
                {
                    straightStarts[startNum] = new HashSet<int>();
                }
                straightStarts[startNum].Add(suit);
            }
        }

        // 检查是否有某个起始数值凑齐了 3 种花色 (0, 1, 2)
        foreach (var kvp in straightStarts)
        {
            if (kvp.Value.Contains(0) && kvp.Value.Contains(1) && kvp.Value.Contains(2))
            {
                return true;
            }
        }
        return false;
    }
    // 【新增】三色同刻：三种花色都有相同的刻子 (如 222筒, 222条, 222万)
    private bool IsSanSeTongKe(List<List<int>> sets)
    {
        // 字典：刻子数值(0-8) -> 拥有的花色集合
        Dictionary<int, HashSet<int>> pungValues = new Dictionary<int, HashSet<int>>();

        foreach (var set in sets)
        {
            // 只检查刻子或杠
            if (!IsPungOrKong(set)) continue;

            // 获取这张牌的 ID (因为是刻子，取第一个就行)
            int id = set[0] % 27;
            int suit = id / 9;
            int num = id % 9;

            if (!pungValues.ContainsKey(num))
            {
                pungValues[num] = new HashSet<int>();
            }
            pungValues[num].Add(suit);
        }

        // 检查是否有某个数值凑齐了 3 种花色
        foreach (var kvp in pungValues)
        {
            if (kvp.Value.Contains(0) && kvp.Value.Contains(1) && kvp.Value.Contains(2))
            {
                return true;
            }
        }
        return false;
    }
    // 【新增】老头 (老头牌)：所有牌都是 1 或 9
    // 注：由于全是 1 和 9，必然是对对胡结构，所以会与 IsDuiDuiHu 叠加
    private bool IsLaoTou(List<int> allTiles)
    {
        foreach (var id in allTiles)
        {
            int val = (id % 27) % 9;
            // 如果有任何一张牌不是 1(0) 或 9(8)，则不是老头
            if (val != 0 && val != 8) return false;
        }
        return true;
    }

    #endregion

    #region 核心结算逻辑 (叠加制)

    public HandAnalysisResult CalculateHandFan(List<List<int>> huHand, GameSettings settings, bool isTianHu = false, bool isDiHu = false)
    {
        var result = new HandAnalysisResult();
        if (huHand == null || huHand.Count == 0) return result;

        var sets = huHand.Where(s => s.Count > 2).ToList();
        var pair = huHand.FirstOrDefault(s => s.Count == 2);
        if (sets.Count < settings.setsForHu || pair == null) return result;

        var allTileIds = huHand.SelectMany(s => s).ToList();

        // ----------------------------------------------------
        // 1. 基础特征判定
        // ----------------------------------------------------
        bool isDuiDuiHu = sets.All(s => IsPungOrKong(s));

        // 清一色判定
        bool isQingYiSe = false;
        int suitCount = allTileIds.Select(id => ((id % 27) / 9)).Distinct().Count();
        if (suitCount == 1) isQingYiSe = true;
        // 支持混淆视听条约 (2种花色视为清一色)
        else if (GameManager.Instance != null && GameManager.Instance.isHunYaoShiTingActive && suitCount == 2) isQingYiSe = true;

        bool isLaoTou = IsLaoTou(allTileIds);
        bool isIttsu = IsIttsu(sets);
        bool isSanSe = IsSanSeTongShun(sets);
        bool isSanSeKe = IsSanSeTongKe(sets);

        // ----------------------------------------------------
        // 2. 番数叠加计算
        // ----------------------------------------------------
        int totalFan = 0;
        List<string> activePatterns = new List<string>();

        // (1) 天胡 / 地胡
        if (isTianHu)
        {
            totalFan += 6;
            activePatterns.Add("HU_TYPE_TIAN");
        }
        else if (isDiHu)
        {
            totalFan += 4;
            activePatterns.Add("HU_TYPE_DI");
        }

        // (2) 清一色 (4番)
        if (isQingYiSe)
        {
            totalFan += 4;
            activePatterns.Add("HU_TYPE_QINGYISE");
        }

        // (3) 对对 (5番)
        if (isDuiDuiHu)
        {
            totalFan += 5;
            activePatterns.Add("HU_TYPE_DUIDUI");
        }

        // (4) 老头 (3番)
        // 叠加示例：如果全是1和9 -> 是老头(3) + 必然是对对(3) = 6番 (即清老头效果)
        if (isLaoTou)
        {
            totalFan += 3;
            activePatterns.Add("HU_TYPE_LAOTOU");
        }

        // (5) 一气通贯 (4番)
        if (isIttsu)
        {
            totalFan += 4;
            activePatterns.Add("HU_TYPE_YIQITONGGUAN");
        }

        // (6) 三色同顺 (4番)
        if (isSanSe)
        {
            totalFan += 4;
            activePatterns.Add("HU_TYPE_SANSETONGSHUN");
        }

        // (7) 【新增】三色同刻 (4番)
        if (isSanSeKe)
        {
            totalFan += 4;
            activePatterns.Add("HU_TYPE_SANSETONGKE");
        }

        // (8) 平胡 (1番)
        // 只有当没有任何其他番数时，平胡才生效
        if (totalFan == 0)
        {
            totalFan = 1;
            activePatterns.Add("HU_TYPE_PING");
        }

        // ----------------------------------------------------
        // 3. 结果构建
        // ----------------------------------------------------

        // 牌型名称用 " · " 连接
        result.PatternName = string.Join(" · ", activePatterns);

        // 加上杠牌的额外番数
        int kongFan = sets.Count(s => s.Count == 4) * settings.fanBonusPerKong;
        result.TotalFan = totalFan + kongFan;

        return result;
    }

    #endregion
}