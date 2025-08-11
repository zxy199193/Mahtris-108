using System;
using System.Collections.Generic;
using UnityEngine;

public class BlockPool : MonoBehaviour
{
    public static BlockPool Instance { get; private set; }

    [Tooltip("��С����������Ĭ��108��")]
    public int totalBlocks = 108;

    // ���õ� blockId �б�
    private List<int> availableBlocks = new List<int>();

    // �齫ͼƬ������ Resources/MahjongTiles��
    private Sprite[] mahjongSprites;

    // �ƿ������仯�¼�
    public event Action<int> OnPoolCountChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        // ���Լ��� Sprites����ѡ��
        mahjongSprites = Resources.LoadAll<Sprite>("MahjongTiles");
        if (mahjongSprites != null && mahjongSprites.Length > 0)
            Array.Sort(mahjongSprites, (a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));
        else
            Debug.LogWarning("BlockPool: δ���ص� MahjongTiles Sprite��Resources/MahjongTiles��");

        ResetFullDeck();
    }

    public void ResetFullDeck()
    {
        availableBlocks.Clear();
        for (int i = 0; i < totalBlocks; i++) availableBlocks.Add(i);
        Shuffle(availableBlocks);

        Debug.Log($"[BlockPool] ResetFullDeck -> {availableBlocks.Count}");
        OnPoolCountChanged?.Invoke(availableBlocks.Count);
    }

    private void Shuffle(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = UnityEngine.Random.Range(i, list.Count);
            int tmp = list[i]; list[i] = list[j]; list[j] = tmp;
        }
    }

    public int GetBlockId()
    {
        if (availableBlocks.Count == 0) return -1;
        int idx = UnityEngine.Random.Range(0, availableBlocks.Count);
        int id = availableBlocks[idx];
        availableBlocks.RemoveAt(idx);
        OnPoolCountChanged?.Invoke(availableBlocks.Count);
        Debug.Log($"[BlockPool] GetBlockId -> {id}  left={availableBlocks.Count}");
        return id;
    }

    // ����ȡ�ƣ����� null ��ʾ���㣩
    public List<int> GetBlockIds(int count)
    {
        if (count <= 0) return new List<int>();
        if (count > availableBlocks.Count) return null;

        List<int> ids = new List<int>();
        for (int i = 0; i < count; i++)
        {
            int id = GetBlockId();
            if (id == -1)
            {
                // �ع��������ϲ����ߵ���
                foreach (var r in ids) ReturnBlock(r);
                return null;
            }
            ids.Add(id);
        }
        return ids;
    }

    public void ReturnBlock(int id)
    {
        if (id < 0 || id >= totalBlocks) { Debug.LogWarning($"[BlockPool] ReturnBlock invalid {id}"); return; }
        if (availableBlocks.Contains(id))
        {
            Debug.LogWarning($"[BlockPool] ReturnBlock duplicate {id}");
            return;
        }
        availableBlocks.Add(id);
        OnPoolCountChanged?.Invoke(availableBlocks.Count);
        Debug.Log($"[BlockPool] ReturnBlock {id} -> left={availableBlocks.Count}");
    }

    public void ReturnBlockIds(List<int> ids)
    {
        if (ids == null || ids.Count == 0) return;
        foreach (var id in ids) ReturnBlock(id);
    }

    public int GetRemainingBlocks() => availableBlocks.Count;

    // ���� blockId ӳ�䵽 Sprite���� mahjongSprites��
    public Sprite GetSpriteForBlock(int blockId)
    {
        if (mahjongSprites == null || mahjongSprites.Length == 0) return null;
        if (blockId < 0) return null;
        int idx = blockId % mahjongSprites.Length;
        return mahjongSprites[idx];
    }
}
