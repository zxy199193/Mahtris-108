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
    private float typhoonTimer; // 【新增】

    public void Initialize(GameSettings gameSettings, TetrisGrid grid)
    {
        this.settings = gameSettings;
        this.tetrisGrid = grid;
        this.fastFallSpeedValue = settings.fastFallSpeed;
        this.typhoonTimer = 2f; // 【新增】初始化台风计时器

        // 【新增】“流星雨”逻辑
        if (GameManager.Instance.isMeteorShowerActive && Random.value < 0.1f)
        {
            // 速度25 (时间 = 20 / 25)
            this.fallSpeed = 20f / 25f;
            // 临时更新UI
            GameManager.Instance.UpdateSpeedUITemp(25);
        }
        else
        {
            // 获取常规速度
            this.fallSpeed = GameManager.Instance.currentFallSpeed;
        }
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

        // 【新增】“台风天气”逻辑
        if (GameManager.Instance != null && GameManager.Instance.isTyphoonActive)
        {
            typhoonTimer -= Time.deltaTime;
            if (typhoonTimer <= 0)
            {
                typhoonTimer = 2f; // 重置计时器
                int driftDirection = (Random.value < 0.5f) ? -1 : 1;
                int driftAmount = (Random.value < 0.5f) ? 1 : 2;
                Move(new Vector3(driftDirection * driftAmount, 0, 0));
            }
        }
        // --- 台风逻辑结束 ---

        // 【修改】“戏法空间”逻辑 (快速下落)
        bool trickRoom = (GameManager.Instance != null && GameManager.Instance.isTrickRoomActive);
        KeyCode fastFallKey = trickRoom ? KeyCode.UpArrow : KeyCode.DownArrow;

        float currentSpeed = Input.GetKey(fastFallKey) ? fastFallSpeedValue : fallSpeed;
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
        // 【新增】“深邃黑暗幻想”逻辑
        foreach (var unit in GetComponentsInChildren<BlockUnit>())
        {
            unit.StartFadeToBlack();
        }
        // --- 修改结束 ---
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
        // 【修改】“戏法空间”逻辑
        bool trickRoom = (GameManager.Instance != null && GameManager.Instance.isTrickRoomActive);
        KeyCode leftKey = trickRoom ? KeyCode.RightArrow : KeyCode.LeftArrow;
        KeyCode rightKey = trickRoom ? KeyCode.LeftArrow : KeyCode.RightArrow;
        KeyCode rotateKey = trickRoom ? KeyCode.DownArrow : KeyCode.UpArrow;

        if (Input.GetKeyDown(leftKey)) Move(Vector3.left);
        else if (Input.GetKeyDown(rightKey)) Move(Vector3.right);
        else if (Input.GetKeyDown(rotateKey)) Rotate();
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