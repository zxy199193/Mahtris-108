// FileName: MahjongCore.cs
using System.Collections.Generic;
using System.Linq;

// DetectionResult 类保持不变
public class DetectionResult
{
    public List<List<int>> Kongs { get; set; } = new List<List<int>>();
    public List<List<int>> Pungs { get; set; } = new List<List<int>>();
    public List<List<int>> Chows { get; set; } = new List<List<int>>();
    public List<int> RemainingIds { get; set; } = new List<int>();
}

public class MahjongCore
{
    // FindKongs, FindPungs, FindChows, DetectSets, FindPair 等方法保持不变...
    #region Unchanged Set Detection Methods
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
        return ids.GroupBy(id => id % 27)
                  .FirstOrDefault(g => g.Count() >= 2)?
                  .Take(2).ToList();
    }
    #endregion

    // --- 【新增方法】 ---
    // 计算胡牌牌型的总番数
    public int CalculateHandFan(List<List<int>> huHand, GameSettings settings)
    {
        if (huHand == null || huHand.Count == 0) return 0;

        var sets = huHand.Where(s => s.Count > 2).ToList(); // 4组面子
        var pair = huHand.FirstOrDefault(s => s.Count == 2); // 1组将牌
        if (sets.Count < settings.setsForHu || pair == null) return 0; // 不满足胡牌基本条件

        int patternFan = 0;

        // 1. 判断牌型番数
        bool isDuiDuiHu = sets.All(s => s.Count == 3 || s.Count == 4); // 全是刻子/杠子

        var allTileIds = huHand.SelectMany(s => s).ToList();
        int firstSuit = (allTileIds[0] % 27) / 9;
        bool isQingYiSe = allTileIds.All(id => ((id % 27) / 9) == firstSuit); // 全是一种花色

        if (isQingYiSe && isDuiDuiHu)
        {
            patternFan = 8; // 清大对
        }
        else if (isQingYiSe)
        {
            patternFan = 4; // 清一色
        }
        else if (isDuiDuiHu)
        {
            patternFan = 2; // 对对胡
        }
        else
        {
            patternFan = 1; // 平胡
        }

        // 2. 计算杠牌番数
        int kongFan = sets.Count(s => s.Count == 4) * settings.fanBonusPerKong;

        return patternFan + kongFan;
    }
}