// FileName: HuPaiArea.cs
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class HuPaiArea : MonoBehaviour
{
    [Header("��������")]
    [SerializeField] private Transform displayParent;
    [SerializeField] private GameObject blockPrefab;
    [SerializeField] private BlockPool blockPool;

    [Header("��������")]
    [SerializeField] private float rowSpacing = 1.2f;
    [SerializeField] private float columnSpacing = 4.0f; // ���������ڿ�������֮��ļ��
    [SerializeField] private float tileSpacing = 1.1f;

    private List<List<int>> huPaiSets = new List<List<int>>();

    public void AddSets(List<List<int>> sets)
    {
        huPaiSets.AddRange(sets);
        RefreshDisplay();
    }

    public bool RemoveLastSet()
    {
        if (huPaiSets.Count > 0)
        {
            var lastSet = huPaiSets.Last();
            blockPool.ReturnBlockIds(lastSet);
            huPaiSets.RemoveAt(huPaiSets.Count - 1);
            RefreshDisplay();
            return true;
        }
        return false;
    }

    public int GetSetCount() => huPaiSets.Count;

    public List<List<int>> GetAllSets() => new List<List<int>>(huPaiSets);

    public void ClearAll()
    {
        huPaiSets.Clear();
        if (displayParent != null)
        {
            foreach (Transform child in displayParent) Destroy(child.gameObject);
        }
    }

    private void RefreshDisplay()
    {
        if (displayParent != null)
        {
            foreach (Transform child in displayParent) Destroy(child.gameObject);
        }
        else
        {
            Debug.LogError("HuPaiArea �� displayParent ����δ����!");
            return;
        }

        if (blockPrefab == null || blockPool == null)
        {
            Debug.LogError("HuPaiArea �� blockPrefab �� blockPool ����δ����!");
            return;
        }

        // �������㡿��д�˲����߼���ʵ��˫����������
        for (int i = 0; i < huPaiSets.Count; i++)
        {
            var set = huPaiSets[i];

            // ���㵱ǰ��������һ�� (0 or 1) ����һ�� (0, 1, 2...)
            int columnIndex = i % 2;
            int rowIndex = i / 2;

            // �������м������һ���Ƶ���ʼλ��
            float startX = columnIndex * columnSpacing;
            float yPos = -rowIndex * rowSpacing;

            // ����ʼλ�õĻ����ϣ��������ڵ�ÿһ����
            for (int tileIndex = 0; tileIndex < set.Count; tileIndex++)
            {
                int blockId = set[tileIndex];
                float xPos = startX + (tileIndex * tileSpacing);

                GameObject go = Instantiate(blockPrefab, displayParent);
                go.transform.localPosition = new Vector3(xPos, yPos, 0);

                var bu = go.GetComponent<BlockUnit>();
                if (bu != null)
                {
                    bu.Initialize(blockId, blockPool);
                }
            }
        }
    }
}
