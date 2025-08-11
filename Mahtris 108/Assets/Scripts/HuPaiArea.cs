using System.Collections.Generic;
using UnityEngine;

public class HuPaiArea : MonoBehaviour
{
    public static HuPaiArea Instance { get; private set; }

    [Header("��ʾ��أ��� Inspector ��ֵ��")]
    [SerializeField] private Transform displayParent;   // �����������������壩
    [SerializeField] private GameObject blockPrefab;    // ��ѡ������ϣ��������ʾ�������ƶ������ô� prefab

    // ÿһ���� List<int> ���� blockId�����ӻ�˳�ӣ�
    private List<List<int>> huPaiSets = new List<List<int>>();

    private void Awake()
    {
        if (Instance == null) Instance = this; else Destroy(gameObject);
    }

    // ���һ�� ID��������ֱ���ƶ������ϵ� Transform �� displayParent ʱҲӦ���ô˷�����¼ID��
    public void AddHuPaiSetFromIds(List<int> setIds)
    {
        if (setIds == null || setIds.Count != 3) return;
        huPaiSets.Add(new List<int>(setIds));
        Debug.Log($"[HuPaiArea] AddHuPaiSetFromIds total={huPaiSets.Count}");
        // �������Ҫ���ӻ������������ƶ�ԭ���壬�������� Instantiate blockPrefab
        RefreshDisplay();
    }

    // ���������� Transform ���루����ʹ�ã���transforms.Count ==3
    // �����Щ transforms reparent �� displayParent�������Ϊ display-only
    // ����Ҳ��� displayParent����� fallback �� AddHuPaiSetFromIds + ���ƶ�
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

        // ��� displayParent �����ã���� transforms ���븸���󲢱��Ϊչʾ
        if (displayParent != null)
        {
            // �����У���������
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
                    // ���û�� BlockUnit����� reparent
                    t.SetParent(displayParent, false);
                }
            }
        }
        else
        {
            // fallback: ֱ��ˢ����ʾ����� blockPrefab ���ã����ø�����ʾ��
            RefreshDisplay();
        }
    }

    public int GetHuPaiSetCount() => huPaiSets.Count;
    public List<List<int>> GetAllSets() => huPaiSets;

    public void ClearAll()
    {
        // �����ʾ parent �µ������壨��������չʾ�
        if (displayParent != null)
        {
            foreach (Transform c in displayParent)
                Destroy(c.gameObject);
        }
        huPaiSets.Clear();
        Debug.Log("[HuPaiArea] ClearAll");
    }

    // �������� prefab ��ʾ���������ƶ������ϵ�ԭ������RefreshDisplay ��� huPaiSets ��ʾ��������ɾ�� displayParent �����������壩
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
