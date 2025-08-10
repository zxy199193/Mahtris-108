using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Tetromino : MonoBehaviour
{
    float lastFall = 0;

    void Start()
    {
        if (!IsValidGridPos())
        {
            Debug.Log("GAME OVER");
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // ����
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            transform.position += new Vector3(-1, 0, 0);
            if (IsValidGridPos())
                UpdateGrid();
            else
                transform.position += new Vector3(1, 0, 0);
        }
        // ����
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            transform.position += new Vector3(1, 0, 0);
            if (IsValidGridPos())
                UpdateGrid();
            else
                transform.position += new Vector3(-1, 0, 0);
        }
        // ��ת
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            transform.Rotate(0, 0, -90);
            if (IsValidGridPos())
                UpdateGrid();
            else
                transform.Rotate(0, 0, 90);
        }

        // ����
        float fallSpeed = Input.GetKey(KeyCode.DownArrow) ? 0.05f : 1f; // �� 0.05 ��
        if (Time.time - lastFall >= fallSpeed)
        {
            transform.position += new Vector3(0, -1, 0);

            if (IsValidGridPos())
            {
                UpdateGrid();
            }
            else
            {
                transform.position += new Vector3(0, 1, 0);
                UpdateGrid(); // ȷ������λ�ø��µ�����

                int clearedLines = TetrisGrid.DeleteFullRows();
                if (clearedLines > 0 && ScoreManager.Instance != null)
                {
                    ScoreManager.Instance.AddScore(clearedLines);
                }

                FindObjectOfType<Spawner>().SpawnBlock();
                enabled = false;
            }
            lastFall = Time.time;
        }
    }

    bool IsValidGridPos()
    {
        foreach (Transform child in transform)
        {
            Vector2 v = TetrisGrid.RoundVector2(child.position);
            if (!TetrisGrid.InsideBorder(v))
                return false;
            if (TetrisGrid.grid[(int)v.x, (int)v.y] != null &&
                TetrisGrid.grid[(int)v.x, (int)v.y].parent != transform)
                return false;
        }
        return true;
    }

    void UpdateGrid()
    {
        for (int y = 0; y < TetrisGrid.height; ++y)
            for (int x = 0; x < TetrisGrid.width; ++x)
                if (TetrisGrid.grid[x, y] != null &&
                    TetrisGrid.grid[x, y].parent == transform)
                    TetrisGrid.grid[x, y] = null;

        foreach (Transform child in transform)
        {
            Vector2 v = TetrisGrid.RoundVector2(child.position);
            TetrisGrid.grid[(int)v.x, (int)v.y] = child;
        }
    }
}