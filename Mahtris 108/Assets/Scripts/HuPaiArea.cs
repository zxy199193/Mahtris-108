using System.Collections.Generic;
using UnityEngine;

public class HuPaiArea : MonoBehaviour
{
    [Header("��������")]
    [SerializeField] private Transform displayParent; // �������������������������
    [SerializeField] private GameObject blockPrefab;   // ������ʾ���������Ԥ�Ƽ�
    [SerializeField] private BlockPool blockPool;     // ���ڻ�ȡ������ͼ

    [Header("��������")]
    [Tooltip("ÿһ���ƣ��У�֮��Ĵ�ֱ���")]
    [SerializeField] private float rowSpacing = 1.2f;
    [Tooltip("һ�����ڲ���ÿ����֮���ˮƽ���")]
    [SerializeField] private float tileSpacing = 1.1f;

    private List<List<int>> huPaiSets = new List<List<int>>();

    public void AddSets(List<List<int>> sets)
    {
        huPaiSets.AddRange(sets);
        RefreshDisplay();
    }

    public int GetSetCount() => huPaiSets.Count;
    public List<List<int>> GetAllSets() => new List<List<int>>(huPaiSets);

    public void ClearAll()
    {
        huPaiSets.Clear();
        if (displayParent != null)
        {
            foreach (Transform child in displayParent)
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void RefreshDisplay()
    {
        // 1. ����վɵ���ʾ
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

        // 2. ѭ������ÿһ�顰���ӡ���������Ϊ����һ��������
        for (int rowIndex = 0; rowIndex < huPaiSets.Count; rowIndex++)
        {
            var set = huPaiSets[rowIndex];

            // 3. ���㵱ǰ�У��飩�Ĵ�ֱλ�� (Y����)
            // ʹ�ø������µ�һ����ʾ����һ�е��·�
            float yPos = -rowIndex * rowSpacing;

            // 4. ѭ��������ǰ���������ÿһ����
            for (int tileIndex = 0; tileIndex < set.Count; tileIndex++)
            {
                int blockId = set[tileIndex];

                // 5. ���㵱ǰ�Ƶ�ˮƽλ�� (X����)
                float xPos = tileIndex * tileSpacing;

                // 6. ʵ�����Ƶ���ʾ���壬���������������ڵľֲ�λ��
                GameObject go = Instantiate(blockPrefab, displayParent);
                go.transform.localPosition = new Vector3(xPos, yPos, 0);

                // 7. ��ʼ���������ʾ
                var bu = go.GetComponent<BlockUnit>();
                if (bu != null)
                {
                    // ע�⣺�����Initialize������BlockUnit�ϵģ�������������
                    bu.Initialize(blockId, blockPool);
                }
            }
        }
    }
}
