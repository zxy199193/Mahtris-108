using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    public static int width = 10;
    public static int height = 20;
    public static Transform[,] map = new Transform[width, height];

    public GameObject[] blocks;
    private static GameObject[] _blocks;
    private static Transform _transform;
    void Start()
    {
        _blocks = blocks;
        _transform = transform;
        BirthBlock();
    }

    public static void BirthBlock()
    {
        //???
        int blockIndex = Random.Range(0, _blocks.Length);
        Instantiate(_blocks[blockIndex], _transform.position, Quaternion.identity);
    }

    public static void DeleteBlockFromMap(Transform transform)
    {
        //???
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (map[i, j] != null)
                {
                    if (map[i, j].parent == transform)
                    {
                        map[i, j] = null;
                    }
                }
            }
        }
    }
}