// FileName: Tetromino.cs
using UnityEngine;

public class Tetromino : MonoBehaviour
{
    [Header("玩法配置")]
    [Tooltip("该方块类型对应的【额外倍率】值")]
    public float extraMultiplier = 1f;

    [Header("UI显示")]
    [Tooltip("用于在UI中显示的【单张形状图片】")]
    public Sprite shapeUISprite;
    [Tooltip("用于在UI中显示的【UI版预制件】")]
    public GameObject uiPrefab;

    private float lastFallTime;
    private float fallSpeed; // 当前的基础下落速度
    private float fastFallSpeedValue; // 快速下落的固定速度值
    private TetrisGrid tetrisGrid;
    private GameSettings settings;

    public void Initialize(GameSettings gameSettings, TetrisGrid grid)
    {
        this.settings = gameSettings;
        this.fallSpeed = GameManager.Instance.currentFallSpeed;
        this.fastFallSpeedValue = settings.fastFallSpeed; // 从GameSettings读取
        this.tetrisGrid = grid;
    }

    // 【新增方法】允许GameManager在外部即时更新速度
    public void UpdateFallSpeedNow(float newSpeed)
    {
        this.fallSpeed = newSpeed;
    }

    void Start()
    {
        if (!tetrisGrid.IsValidGridPos(transform))
        {
            GameEvents.TriggerGameOver();
            Destroy(gameObject);
        }
    }

    void Update()
    {
        HandleMovementInput();

        // 【修正点】快速下落使用独立的恒定速度
        float currentSpeed = Input.GetKey(KeyCode.DownArrow) ? fastFallSpeedValue : fallSpeed;
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

        foreach (Transform child in transform)
        {
            if (Mathf.RoundToInt(child.position.y) >= settings.deadlineHeight)
            {
                GameEvents.TriggerGameOver();
                return;
            }
        }

        tetrisGrid.CheckForFullRows();
    }

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
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayRotateSound();

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
}