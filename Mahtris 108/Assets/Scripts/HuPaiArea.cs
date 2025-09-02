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
        else { return; }

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
                if (bu != null) bu.Initialize(blockId, blockPool);
            }
        }
    }
}

