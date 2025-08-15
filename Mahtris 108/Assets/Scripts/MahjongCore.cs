// FileName: MahjongCore.cs
using System.Collections.Generic;
using System.Linq;

// (DetectionResult, HandAnalysisResult 类以及 FindKongs/Pungs/Chows/DetectSets/FindPair 方法保持不变)
#region Unchanged Code
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
    #endregion

    private bool IsPungOrKong(List<int> set)
    {
        if (set.Count < 3) return false;
        return set.Select(id => id % 27).Distinct().Count() == 1;
    }

    public HandAnalysisResult CalculateHandFan(List<List<int>> huHand, GameSettings settings)
    {
        var result = new HandAnalysisResult();
        if (huHand == null || huHand.Count == 0) return result;

        var sets = huHand.Where(s => s.Count > 2).ToList();
        var pair = huHand.FirstOrDefault(s => s.Count == 2);
        if (sets.Count < settings.setsForHu || pair == null) return result;

        int patternFan = 0;

        bool isDuiDuiHu = sets.All(s => IsPungOrKong(s));

        var allTileIds = huHand.SelectMany(s => s).ToList();
        int firstSuit = (allTileIds[0] % 27) / 9;
        bool isQingYiSe = allTileIds.All(id => ((id % 27) / 9) == firstSuit);

        if (isQingYiSe && isDuiDuiHu)
        {
            patternFan = 8;
            result.PatternName = "清大对";
        }
        else if (isQingYiSe)
        {
            patternFan = 4;
            result.PatternName = "清一色";
        }
        else if (isDuiDuiHu)
        {
            patternFan = 2;
            result.PatternName = "对对胡";
        }
        else
        {
            patternFan = 1;
            result.PatternName = "平胡";
        }

        int kongFan = sets.Count(s => s.Count == 4) * settings.fanBonusPerKong;
        result.TotalFan = patternFan + kongFan;

        return result;
    }
}