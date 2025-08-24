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
    [SerializeField] private float tileSpacing = 1.1f;

    private List<List<int>> huPaiSets = new List<List<int>>();

    public void AddSets(List<List<int>> sets)
    {
        huPaiSets.AddRange(sets);
        RefreshDisplay();
    }

    // ����Ƥ�������������������
    public bool RemoveLastSet()
    {
        if (huPaiSets.Count > 0)
        {
            // ��ȡ���һ����
            var lastSet = huPaiSets.Last();
            // ���Ƶ�ID���ص��ƿ�
            blockPool.ReturnBlockIds(lastSet);
            // �Ӻ������б����Ƴ�
            huPaiSets.RemoveAt(huPaiSets.Count - 1);
            // ˢ��UI��ʾ
            RefreshDisplay();
            Debug.Log("��ʹ����Ƥ���Ƴ��������һ���ơ�");
            return true; // ��ʾ�ɹ��Ƴ�
        }
        Debug.Log("������Ϊ�գ���Ƥʹ��ʧ�ܡ�");
        return false; // ��ʾû���ƿ����Ƴ�
    }

    public int GetSetCount()
    {
        return huPaiSets.Count;
    }

    public List<List<int>> GetAllSets()
    {
        // ����һ����������ֹ�ⲿ�޸�
        return new List<List<int>>(huPaiSets);
    }



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

        // �������еĲ����߼�
        for (int rowIndex = 0; rowIndex < huPaiSets.Count; rowIndex++)
        {
            var set = huPaiSets[rowIndex];
            float yPos = -rowIndex * rowSpacing;

            for (int tileIndex = 0; tileIndex < set.Count; tileIndex++)
            {
                int blockId = set[tileIndex];
                float xPos = tileIndex * tileSpacing;

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
