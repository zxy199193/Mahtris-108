using System.Collections.Generic;
using UnityEngine;

public class HuPaiArea : MonoBehaviour
{
    [Header("核心引用")]
    [SerializeField] private Transform displayParent; // 用于容纳所有牌面物体的容器
    [SerializeField] private GameObject blockPrefab;   // 用于显示单个牌面的预制件
    [SerializeField] private BlockPool blockPool;     // 用于获取牌面贴图

    [Header("布局设置")]
    [Tooltip("每一组牌（行）之间的垂直间距")]
    [SerializeField] private float rowSpacing = 1.2f;
    [Tooltip("一组牌内部，每张牌之间的水平间距")]
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
        // 1. 先清空旧的显示
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

        // 2. 循环遍历每一组“面子”，将其作为单独一行来处理
        for (int rowIndex = 0; rowIndex < huPaiSets.Count; rowIndex++)
        {
            var set = huPaiSets[rowIndex];

            // 3. 计算当前行（组）的垂直位置 (Y坐标)
            // 使用负号让新的一行显示在上一行的下方
            float yPos = -rowIndex * rowSpacing;

            // 4. 循环遍历当前这组牌里的每一张牌
            for (int tileIndex = 0; tileIndex < set.Count; tileIndex++)
            {
                int blockId = set[tileIndex];

                // 5. 计算当前牌的水平位置 (X坐标)
                float xPos = tileIndex * tileSpacing;

                // 6. 实例化牌的显示物体，并设置其在容器内的局部位置
                GameObject go = Instantiate(blockPrefab, displayParent);
                go.transform.localPosition = new Vector3(xPos, yPos, 0);

                // 7. 初始化牌面的显示
                var bu = go.GetComponent<BlockUnit>();
                if (bu != null)
                {
                    // 注意：这里的Initialize方法是BlockUnit上的，用于设置牌面
                    bu.Initialize(blockId, blockPool);
                }
            }
        }
    }
}
