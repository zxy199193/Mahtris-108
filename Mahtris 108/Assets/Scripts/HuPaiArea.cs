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
            DontDestroyOnLoad(gameObject); // 如果需要跨场景保留，可以加这行
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
        Debug.Log($"胡牌区添加了一个牌型，当前共有 {huPaiSets.Count} 组");
    }

    public int GetHuPaiSetCount()
    {
        return huPaiSets.Count;
    }

    public void ClearAll()
    {
        huPaiSets.Clear();
        Debug.Log("胡牌区已清空");
    }
}
