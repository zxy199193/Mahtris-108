// FileName: MahjongCore.cs
using System.Collections.Generic;
using System.Linq;

// 用来封装检测结果的类，保持不变
public class DetectionResult
{
    public List<List<int>> Kongs { get; set; } = new List<List<int>>();
    public List<List<int>> Pungs { get; set; } = new List<List<int>>();
    public List<List<int>> Chows { get; set; } = new List<List<int>>();
    public List<int> RemainingIds { get; set; } = new List<int>();
}

public class MahjongCore
{
    // FindKongs 方法保持不变
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

    // FindPungs 方法保持不变
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

    // ---【重大修正点】---
    // 以下是重写后的 FindChows 方法，逻辑更可靠
    private void FindChows(List<int> ids, DetectionResult result)
    {
        // 1. 按花色分组，逐个处理
        var tilesBySuit = ids.GroupBy(id => (id % 27) / 9)
                             .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var suitGroup in tilesBySuit.Values)
        {
            // 2. 只要当前花色的牌还够组成顺子，就一直循环寻找
            while (true)
            {
                if (suitGroup.Count < 3) break;

                // 创建一个从牌面值(0-8)到实际Block ID列表的映射
                var valueToIds = suitGroup.GroupBy(id => id % 27 % 9)
                                          .ToDictionary(g => g.Key, g => g.ToList());

                bool foundChowThisPass = false;

                // 3. 顺子只能从1开始到7结束（牌面值0-6）
                for (int i = 0; i <= 6; i++)
                {
                    // 检查是否存在连续的三张牌 (i, i+1, i+2)
                    if (valueToIds.ContainsKey(i) && valueToIds.ContainsKey(i + 1) && valueToIds.ContainsKey(i + 2))
                    {
                        // 找到了一个顺子
                        int id1 = valueToIds[i][0];
                        int id2 = valueToIds[i + 1][0];
                        int id3 = valueToIds[i + 2][0];

                        var chow = new List<int> { id1, id2, id3 };
                        result.Chows.Add(chow);

                        // 从当前花色牌组中移除这三张已被使用的牌
                        suitGroup.Remove(id1);
                        suitGroup.Remove(id2);
                        suitGroup.Remove(id3);

                        // 从主列表（将被作为“剩余牌”返回）中也移除
                        ids.Remove(id1);
                        ids.Remove(id2);
                        ids.Remove(id3);

                        foundChowThisPass = true;
                        // 成功找到一组，跳出for循环，重新开始while循环，扫描剩余的牌
                        break;
                    }
                }

                // 4. 如果完整扫描一轮后都没找到新的顺子，说明此花色已没有顺子可组
                if (!foundChowThisPass)
                {
                    break;
                }
            }
        }
    }

    // 主入口 DetectSets 方法保持不变，它调用各个Find方法的顺序是正确的
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

    // FindPair 方法保持不变
    public List<int> FindPair(List<int> ids)
    {
        return ids.GroupBy(id => id % 27)
                  .FirstOrDefault(g => g.Count() >= 2)?
                  .Take(2).ToList();
    }
}
