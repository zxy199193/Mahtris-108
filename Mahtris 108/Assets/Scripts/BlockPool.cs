// BlockPool.cs
using System;
using System.Collections.Generic;
using UnityEngine;

public class BlockPool : MonoBehaviour
{
    public static BlockPool Instance;

    [Tooltip("��С����������Ĭ��108��")]
    public int totalBlocks = 108;

    // ���õ� blockId �б�0..totalBlocks-1��
    private List<int> availableBlocks = new List<int>();

    // 27 ���齫ͼƬ��Ӧ���� Resources/MahjongTiles �£�
    private Sprite[] mahjongSprites;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        // �����齫ͼƬ������ Assets/Resources/MahjongTiles��
        mahjongSprites = Resources.LoadAll<Sprite>("MahjongTiles");
        if (mahjongSprites == null || mahjongSprites.Length == 0)
        {
            Debug.LogError("BlockPool: û�м��ص� MahjongTiles �µ� Sprite����� 27 ��ͼƬ�ŵ� Assets/Resources/MahjongTiles/ �С�");
        }
        else
        {
            // Ϊ��ȷ��ӳ��˳���ȶ������ļ�������Name Ӧ������������ Tong1, Tong2 ... Wan1 ... Tiao1 ...��
            Array.Sort(mahjongSprites, (a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));
            Debug.Log($"BlockPool: ���ص� {mahjongSprites.Length} ���齫ͼƬ���Ѱ��������򣩡�");
        }

        // ��ʼ�� ID ��
        availableBlocks.Clear();
        for (int i = 0; i < totalBlocks; i++)
            availableBlocks.Add(i);

        Debug.Log($"BlockPool: ��ʼ����ɣ�����С������ = {availableBlocks.Count}");
    }

    // ����ȡ���������FIFO�����ԣ������������
    public int GetBlockId()
    {
        if (availableBlocks.Count == 0) return -1;
        int idx = UnityEngine.Random.Range(0, availableBlocks.Count);
        int id = availableBlocks[idx];
        availableBlocks.RemoveAt(idx);
        return id;
    }

    // ��������
    public void ReturnBlock(int blockId)
    {
        if (blockId < 0 || blockId >= totalBlocks)
        {
            Debug.LogWarning($"BlockPool.ReturnBlock: ��Ч blockId {blockId}");
            return;
        }
        availableBlocks.Add(blockId);
    }

    // ������ȡ������ null ��ʾ���㣩
    public List<int> GetBlockIds(int count)
    {
        List<int> ids = new List<int>();
        if (count <= 0) return ids;

        for (int i = 0; i < count; i++)
        {
            int id = GetBlockId();
            if (id == -1)
            {
                // �������ع��Ѿ�ȡ���� id ������ null
                foreach (var r in ids) ReturnBlock(r);
                return null;
            }
            ids.Add(id);
        }
        return ids;
    }

    // ��������
    public void ReturnBlockIds(List<int> ids)
    {
        if (ids == null) return;
        foreach (var id in ids)
            ReturnBlock(id);
    }

    // ʣ���������ⲿ���ã�
    public int GetRemainingBlocks()
    {
        return availableBlocks.Count;
    }

    // ���� blockId ���ض�Ӧ���齫 Sprite��27 ��ѭ����
    public Sprite GetSpriteForBlock(int blockId)
    {
        if (mahjongSprites == null || mahjongSprites.Length == 0)
        {
            Debug.LogWarning("BlockPool.GetSpriteForBlock: mahjongSprites δ���ػ�Ϊ�ա�");
            return null;
        }

        if (blockId < 0 || blockId >= totalBlocks)
        {
            Debug.LogWarning($"BlockPool.GetSpriteForBlock: ��Ч�� blockId {blockId}");
            return null;
        }

        int tileType = blockId % mahjongSprites.Length; // 0..26
        return mahjongSprites[tileType];
    }
}
