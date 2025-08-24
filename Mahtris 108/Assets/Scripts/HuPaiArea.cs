// FileName: HuPaiArea.cs
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class HuPaiArea : MonoBehaviour
{
    [Header("核心引用")]
    [SerializeField] private Transform displayParent;
    [SerializeField] private GameObject blockPrefab;
    [SerializeField] private BlockPool blockPool;

    [Header("布局设置")]
    [SerializeField] private float rowSpacing = 1.2f;
    [SerializeField] private float tileSpacing = 1.1f;

    private List<List<int>> huPaiSets = new List<List<int>>();

    public void AddSets(List<List<int>> sets)
    {
        huPaiSets.AddRange(sets);
        RefreshDisplay();
    }

    // “橡皮”道具所需的新增方法
    public bool RemoveLastSet()
    {
        if (huPaiSets.Count > 0)
        {
            // 获取最后一组牌
            var lastSet = huPaiSets.Last();
            // 将牌的ID返回到牌库
            blockPool.ReturnBlockIds(lastSet);
            // 从和牌区列表中移除
            huPaiSets.RemoveAt(huPaiSets.Count - 1);
            // 刷新UI显示
            RefreshDisplay();
            Debug.Log("已使用橡皮，移除了最近的一组牌。");
            return true; // 表示成功移除
        }
        Debug.Log("和牌区为空，橡皮使用失败。");
        return false; // 表示没有牌可以移除
    }

    public int GetSetCount()
    {
        return huPaiSets.Count;
    }

    public List<List<int>> GetAllSets()
    {
        // 返回一个副本，防止外部修改
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
            Debug.LogError("HuPaiArea 的 displayParent 引用未设置!");
            return;
        }

        if (blockPrefab == null || blockPool == null)
        {
            Debug.LogError("HuPaiArea 的 blockPrefab 或 blockPool 引用未设置!");
            return;
        }

        // 纵向排列的布局逻辑
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
