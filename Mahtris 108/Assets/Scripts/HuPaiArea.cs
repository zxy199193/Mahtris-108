using System.Collections.Generic;
using UnityEngine;

public class HuPaiArea : MonoBehaviour
{
    public static HuPaiArea Instance { get; private set; }

    private List<List<int>> huPaiSets = new List<List<int>>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // �����Ҫ�糡�����������Լ�����
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddHuPaiSet(List<int> set)
    {
        if (set == null || set.Count != 3) return;
        huPaiSets.Add(set);
        Debug.Log($"�����������һ�����ͣ���ǰ���� {huPaiSets.Count} ��");
    }

    public int GetHuPaiSetCount()
    {
        return huPaiSets.Count;
    }

    public void ClearAll()
    {
        huPaiSets.Clear();
        Debug.Log("�����������");
    }
}
