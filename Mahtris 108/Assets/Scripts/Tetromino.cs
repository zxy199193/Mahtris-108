using UnityEngine;

public class Tetromino : MonoBehaviour
{
    float lastFall = 0f;

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
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            transform.position += new Vector3(-1, 0, 0);
            if (IsValidGridPos()) UpdateGrid(); else transform.position += new Vector3(1, 0, 0);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            transform.position += new Vector3(1, 0, 0);
            if (IsValidGridPos()) UpdateGrid(); else transform.position += new Vector3(-1, 0, 0);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            transform.Rotate(0, 0, -90);
            if (IsValidGridPos()) UpdateGrid(); else transform.Rotate(0, 0, 90);
        }

        float fallSpeed = Input.GetKey(KeyCode.DownArrow) ? 0.05f : 1f;
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
                UpdateGrid();

                int cleared = TetrisGrid.DeleteFullRows();
                if (cleared > 0 && ScoreManager.Instance != null)
                    ScoreManager.Instance.AddScore(cleared);

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
            if (!TetrisGrid.InsideBorder(v)) return false;
            if (TetrisGrid.grid[(int)v.x, (int)v.y] != null &&
                TetrisGrid.grid[(int)v.x, (int)v.y].parent != transform)
                return false;
        }
        return true;
    }

    void UpdateGrid()
    {
        // 清理旧位置
        for (int y = 0; y < TetrisGrid.height; ++y)
            for (int x = 0; x < TetrisGrid.width; ++x)
                if (TetrisGrid.grid[x, y] != null && TetrisGrid.grid[x, y].parent == transform)
                    TetrisGrid.grid[x, y] = null;

        // 写入新位置
        foreach (Transform child in transform)
        {
            Vector2 v = TetrisGrid.RoundVector2(child.position);
            TetrisGrid.grid[(int)v.x, (int)v.y] = child;
        }
    }
}
