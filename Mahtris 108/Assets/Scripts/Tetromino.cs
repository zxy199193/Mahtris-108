// FileName: Tetromino.cs
using UnityEngine;

public class Tetromino : MonoBehaviour
{
    [Header("玩法配置")]
    [Tooltip("该方块类型对应的【额外倍率】值")]
    public float extraMultiplier = 1f;

    private float lastFallTime;
    private float fallSpeed;
    private float fastFallMultiplier;
    private TetrisGrid tetrisGrid;
    private GameSettings settings;

    public void Initialize(GameSettings gameSettings, TetrisGrid grid)
    {
        this.settings = gameSettings;
        this.fallSpeed = GameManager.Instance.currentFallSpeed;
        this.fastFallMultiplier = settings.fastFallMultiplier;
        this.tetrisGrid = grid;
    }

    void Start()
    {
        if (!tetrisGrid.IsValidGridPos(transform))
        {
            // 出生点被占，直接游戏结束
            GameEvents.TriggerGameOver();
            Destroy(gameObject);
        }
    }

    void Update()
    {
        HandleMovementInput();

        float currentSpeed = Input.GetKey(KeyCode.DownArrow) ? fallSpeed / fastFallMultiplier : fallSpeed;
        if (Time.time - lastFallTime >= currentSpeed)
        {
            Move(Vector3.down);
            lastFallTime = Time.time;
        }
    }

    void Landed()
    {
        enabled = false;
        tetrisGrid.UpdateGrid(transform);

        // ---【新增逻辑】---
        // 检查是否触碰到死亡线
        foreach (Transform child in transform)
        {
            if (Mathf.RoundToInt(child.position.y) >= settings.deadlineHeight)
            {
                GameEvents.TriggerGameOver();
                return; // 游戏已结束，不再进行消行判断
            }
        }

        tetrisGrid.CheckForFullRows();
    }

    // Movement Methods (HandleMovementInput, Move, Rotate) 保持不变...
    #region Unchanged Movement Methods
    void HandleMovementInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow)) Move(Vector3.left);
        else if (Input.GetKeyDown(KeyCode.RightArrow)) Move(Vector3.right);
        else if (Input.GetKeyDown(KeyCode.UpArrow)) Rotate();
    }

    void Move(Vector3 direction)
    {
        transform.position += direction;
        if (!tetrisGrid.IsValidGridPos(transform))
        {
            transform.position -= direction;
            if (direction == Vector3.down)
            {
                Landed();
            }
        }
        else
        {
            tetrisGrid.UpdateGrid(transform);
        }
    }

    void Rotate()
    {
        transform.Rotate(0, 0, -90);
        if (!tetrisGrid.IsValidGridPos(transform))
        {
            transform.Rotate(0, 0, 90);
        }
        else
        {
            tetrisGrid.UpdateGrid(transform);
        }
    }
    #endregion
}