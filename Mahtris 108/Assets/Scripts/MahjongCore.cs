// FileName: MahjongCore.cs
using System.Collections.Generic;
using System.Linq;

// ������װ��������࣬���ֲ���
public class DetectionResult
{
    public List<List<int>> Kongs { get; set; } = new List<List<int>>();
    public List<List<int>> Pungs { get; set; } = new List<List<int>>();
    public List<List<int>> Chows { get; set; } = new List<List<int>>();
    public List<int> RemainingIds { get; set; } = new List<int>();
}

public class MahjongCore
{
    // FindKongs �������ֲ���
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

    // FindPungs �������ֲ���
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

    // ---���ش������㡿---
    // ��������д��� FindChows �������߼����ɿ�
    private void FindChows(List<int> ids, DetectionResult result)
    {
        // 1. ����ɫ���飬�������
        var tilesBySuit = ids.GroupBy(id => (id % 27) / 9)
                             .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var suitGroup in tilesBySuit.Values)
        {
            // 2. ֻҪ��ǰ��ɫ���ƻ������˳�ӣ���һֱѭ��Ѱ��
            while (true)
            {
                if (suitGroup.Count < 3) break;

                // ����һ��������ֵ(0-8)��ʵ��Block ID�б��ӳ��
                var valueToIds = suitGroup.GroupBy(id => id % 27 % 9)
                                          .ToDictionary(g => g.Key, g => g.ToList());

                bool foundChowThisPass = false;

                // 3. ˳��ֻ�ܴ�1��ʼ��7����������ֵ0-6��
                for (int i = 0; i <= 6; i++)
                {
                    // ����Ƿ���������������� (i, i+1, i+2)
                    if (valueToIds.ContainsKey(i) && valueToIds.ContainsKey(i + 1) && valueToIds.ContainsKey(i + 2))
                    {
                        // �ҵ���һ��˳��
                        int id1 = valueToIds[i][0];
                        int id2 = valueToIds[i + 1][0];
                        int id3 = valueToIds[i + 2][0];

                        var chow = new List<int> { id1, id2, id3 };
                        result.Chows.Add(chow);

                        // �ӵ�ǰ��ɫ�������Ƴ��������ѱ�ʹ�õ���
                        suitGroup.Remove(id1);
                        suitGroup.Remove(id2);
                        suitGroup.Remove(id3);

                        // �����б�������Ϊ��ʣ���ơ����أ���Ҳ�Ƴ�
                        ids.Remove(id1);
                        ids.Remove(id2);
                        ids.Remove(id3);

                        foundChowThisPass = true;
                        // �ɹ��ҵ�һ�飬����forѭ�������¿�ʼwhileѭ����ɨ��ʣ�����
                        break;
                    }
                }

                // 4. �������ɨ��һ�ֺ�û�ҵ��µ�˳�ӣ�˵���˻�ɫ��û��˳�ӿ���
                if (!foundChowThisPass)
                {
                    break;
                }
            }
        }
    }

    // ����� DetectSets �������ֲ��䣬�����ø���Find������˳������ȷ��
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

    // FindPair �������ֲ���
    public List<int> FindPair(List<int> ids)
    {
        return ids.GroupBy(id => id % 27)
                  .FirstOrDefault(g => g.Count() >= 2)?
                  .Take(2).ToList();
    }
}
