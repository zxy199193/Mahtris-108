using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public bool IsPaused { get; set; }

    private void Awake()
    {
        Instance = this;
        IsPaused = false;
    }

    public void ResetGame()
    {
        // �������
        for (int y = 0; y < TetrisGrid.height; y++)
            for (int x = 0; x < TetrisGrid.width; x++)
                if (TetrisGrid.grid[x, y] != null)
                {
                    Destroy(TetrisGrid.grid[x, y].gameObject);
                    TetrisGrid.grid[x, y] = null;
                }

        // �����ƿ��
        // TODO

        IsPaused = false;
        FindObjectOfType<Spawner>().SpawnBlock();
    }
}
