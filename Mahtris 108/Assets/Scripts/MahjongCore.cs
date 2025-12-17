// FileName: MahjongCore.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine; // 【修正点】增加了对Unity引擎核心库的引用

public class DetectionResult
{
    public List<List<int>> Kongs { get; set; } = new List<List<int>>();
    public List<List<int>> Pungs { get; set; } = new List<List<int>>();
    public List<List<int>> Chows { get; set; } = new List<List<int>>();
    public List<int> RemainingIds { get; set; } = new List<int>();
}

public class HandAnalysisResult
{
    public string PatternName { get; set; } = "未知牌型";
    public int TotalFan { get; set; } = 0;

    // 【修改】增加底数变量，默认为2
    public float BaseMultiplier { get; set; } = 2f;

    public float FanMultiplier => Mathf.Pow(BaseMultiplier, TotalFan);
}

public class MahjongCore
{
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

                        suitGroup.Remove(id1);
                        suitGroup.Remove(id2);
                        suitGroup.Remove(id3);
                        ids.Remove(id1);
                        ids.Remove(id2);
                        ids.Remove(id3);

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
    private bool IsQingLaoTou(List<List<int>> sets, List<int> pair)
    {
        // 1. 必须全是对对胡结构 (不能有顺子)
        if (!sets.All(s => IsPungOrKong(s))) return false;

        // 2. 检查雀头是否为 1 或 9
        if ((pair[0] % 27 % 9) != 0 && (pair[0] % 27 % 9) != 8) return false;

        // 3. 检查所有面子是否为 1 或 9
        foreach (var set in sets)
        {
            int val = (set[0] % 27) % 9;
            if (val != 0 && val != 8) return false;
        }
        return true;
    }
    private bool IsJiuLianBaoDeng(List<int> allTiles)
    {
        // 1. 必须是清一色 (外部已判断花色，这里再次校验更安全)
        int firstSuit = (allTiles[0] % 27) / 9;
        if (allTiles.Any(id => (id % 27) / 9 != firstSuit)) return false;

        // 2. 统计每个数字 (0-8) 的数量
        int[] counts = new int[9];
        foreach (int id in allTiles)
        {
            counts[(id % 27) % 9]++;
        }

        // 3. 核心检查：
        // 1(索引0) 和 9(索引8) 必须 >= 3张
        if (counts[0] < 3 || counts[8] < 3) return false;

        // 2-8 (索引1-7) 必须 >= 1张
        for (int i = 1; i <= 7; i++)
        {
            if (counts[i] < 1) return false;
        }

        return true;
    }
    private bool IsIttsu(List<List<int>> sets)
    {
        // 标记：[花色, 顺子类型]
        // 顺子类型 0: 123, 1: 456, 2: 789
        bool[,] flags = new bool[3, 3];

        foreach (var set in sets)
        {
            // 过滤掉刻子和杠，只看顺子 (Chow)
            // 顺子的特征：由3张牌组成，且去重后的数值有3个 (因为是连续的，所以数值肯定不同)
            // 注意：IsPungOrKong 已经有了，反之即为潜在的 Chow (前提是 set.Count==3)
            if (set.Count != 3 || IsPungOrKong(set)) continue;

            // 排序以确定起始值
            var sortedValues = set.Select(id => id % 27).OrderBy(v => v).ToList();

            // 再次确认是连续顺子 (1,2,3)
            if (sortedValues[1] == sortedValues[0] + 1 && sortedValues[2] == sortedValues[0] + 1 + 1)
            {
                int suit = sortedValues[0] / 9;      // 0=筒, 1=条, 2=万
                int startNum = sortedValues[0] % 9;  // 0~8

                if (startNum == 0) flags[suit, 0] = true; // 1-2-3
                if (startNum == 3) flags[suit, 1] = true; // 4-5-6
                if (startNum == 6) flags[suit, 2] = true; // 7-8-9
            }
        }

        // 检查是否有任意一个花色集齐了3种顺子
        for (int s = 0; s < 3; s++)
        {
            if (flags[s, 0] && flags[s, 1] && flags[s, 2]) return true;
        }

        return false;
    }
    public HandAnalysisResult CalculateHandFan(List<List<int>> huHand, GameSettings settings, bool isTianHu = false, bool isDiHu = false)
    {
        var result = new HandAnalysisResult();
        if (huHand == null || huHand.Count == 0) return result;

        var sets = huHand.Where(s => s.Count > 2).ToList();
        var pair = huHand.FirstOrDefault(s => s.Count == 2);
        if (sets.Count < settings.setsForHu || pair == null) return result;

        var allTileIds = huHand.SelectMany(s => s).ToList();

        // 基础特征
        bool isDuiDuiHu = sets.All(s => IsPungOrKong(s));

        // 新增特征检测
        bool isQingLaoTou = IsQingLaoTou(sets, pair);
        bool isIttsu = IsIttsu(sets);
        // 清一色判断
        bool isQingYiSe = false;
        int suitCount = allTileIds.Select(id => ((id % 27) / 9)).Distinct().Count();

        if (suitCount == 1) isQingYiSe = true;
        else if (GameManager.Instance != null && GameManager.Instance.isHunYaoShiTingActive && suitCount == 2) isQingYiSe = true;

        bool isJiuLian = false;
        if (suitCount == 1) // 九莲宝灯要求严格同花色
        {
            isJiuLian = IsJiuLianBaoDeng(allTileIds);
        }

        // --- 优先级判定 ---

        int patternFan = 0;
        string name = "";

        // 1. 特殊事件胡牌 (天胡/地胡) - 优先级最高
        if (isTianHu)
        {
            patternFan = 9;
            name = "天胡";
            if (isJiuLian) { patternFan += 12; name += "·九莲宝灯"; } // 可选叠加
        }
        else if (isDiHu)
        {
            patternFan = 6;
            name = "地胡";
            if (isJiuLian) { patternFan += 12; name += "·九莲宝灯"; } // 可选叠加
        }
        // 2. 牌型胡牌
        else if (isJiuLian)
        {
            patternFan = 12;
            name = "九莲宝灯";
        }
        else if (isQingLaoTou)
        {
            patternFan = 8;
            name = "清老头";
        }
        else if (isQingYiSe && isDuiDuiHu)
        {
            patternFan = 8;
            name = "清大对";
        }
        else if (isQingYiSe)
        {
            patternFan = 4;
            name = "清一色";
        }
        else if (isDuiDuiHu)
        {
            patternFan = 3;
            name = "对对胡";
        }
        else
        {
            patternFan = 1;
            name = "平胡";
        }

        result.PatternName = name;
        int kongFan = sets.Count(s => s.Count == 4) * settings.fanBonusPerKong;
        result.TotalFan = patternFan + kongFan;

        return result;
    }
}
