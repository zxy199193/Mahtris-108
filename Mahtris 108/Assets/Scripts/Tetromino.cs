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

    [Header("操作手感 (DAS/ARR)")]
    [Tooltip("长按方向键时的初始延迟时间 (DAS)，建议 0.2")]
    public float dasDelay = 0.2f;
    [Tooltip("长按方向键时的重复移动间隔 (ARR)，建议 0.05")]
    public float arrDelay = 0.05f;

    private float lastFallTime;
    private float fallSpeed; // 当前的基础下落速度
    private float fastFallSpeedValue; // 快速下落的固定速度值
    private TetrisGrid tetrisGrid;
    private GameSettings settings;
    private float typhoonTimer;

    // DAS 计时器
    private float leftHoldTimer = 0f;
    private float rightHoldTimer = 0f;

    public void Initialize(GameSettings gameSettings, TetrisGrid grid)
    {
        this.settings = gameSettings;
        this.tetrisGrid = grid;
        this.fastFallSpeedValue = settings.fastFallSpeed;
        this.typhoonTimer = 2f;

        // 【流星雨逻辑】
        if (GameManager.Instance.isMeteorShowerActive && Random.value < 0.1f)
        {
            this.fallSpeed = 20f / 25f;
        }
        else
        {
            this.fallSpeed = GameManager.Instance.currentFallSpeed;
        }
    }

    public void UpdateFallSpeedNow(float newSpeed)
    {
        this.fallSpeed = newSpeed;
    }

    void Start()
    {
        if (!tetrisGrid.IsValidGridPos(transform))
        {
            GameManager.Instance.TriggerGameOver("GAME_OVER_TOUCH_DEADLINE");
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
                typhoonTimer = 2f;
                int driftDirection = (Random.value < 0.5f) ? -1 : 1;
                int driftAmount = (Random.value < 0.5f) ? 1 : 2;
                Move(new Vector3(driftDirection * driftAmount, 0, 0));
            }
        }

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
        // “深邃黑暗幻想”逻辑
        foreach (var unit in GetComponentsInChildren<BlockUnit>())
        {
            unit.StartFadeToBlack();
        }
        foreach (Transform child in transform)
        {
            if (Mathf.RoundToInt(child.position.y) >= settings.deadlineHeight)
            {
                GameManager.Instance.TriggerGameOver("GAME_OVER_TOUCH_DEADLINE");
                return;
            }
        }

        tetrisGrid.CheckForFullRows();
    }

    // 【核心修改】重写移动输入逻辑，支持 DAS/ARR
    void HandleMovementInput()
    {
        bool trickRoom = (GameManager.Instance != null && GameManager.Instance.isTrickRoomActive);
        KeyCode leftKey = trickRoom ? KeyCode.RightArrow : KeyCode.LeftArrow;
        KeyCode rightKey = trickRoom ? KeyCode.LeftArrow : KeyCode.RightArrow;
        KeyCode rotateKey = trickRoom ? KeyCode.DownArrow : KeyCode.UpArrow;

        // 1. 旋转逻辑 (独立出来，允许在移动时旋转)
        if (Input.GetKeyDown(rotateKey))
        {
            Rotate();
        }

        // 2. 向左移动
        bool hasMovedLeft = false; // 标记本帧是否处理了左移
        if (Input.GetKeyDown(leftKey))
        {
            // 按下瞬间：立即移动，重置计时器
            Move(Vector3.left);
            leftHoldTimer = 0f;
            hasMovedLeft = true;
        }
        else if (Input.GetKey(leftKey))
        {
            // 按住期间：累计时间
            leftHoldTimer += Time.deltaTime;
            // 超过初始延迟 (DAS)
            if (leftHoldTimer >= dasDelay)
            {
                Move(Vector3.left);
                // 回退计时器，只需要再过 arrDelay 秒就会再次触发
                leftHoldTimer = dasDelay - arrDelay;
                hasMovedLeft = true;
            }
        }
        else
        {
            // 松开重置
            leftHoldTimer = 0f;
        }

        // 3. 向右移动 (与左移互斥，防止同时按住鬼畜)
        if (!hasMovedLeft)
        {
            if (Input.GetKeyDown(rightKey))
            {
                Move(Vector3.right);
                rightHoldTimer = 0f;
            }
            else if (Input.GetKey(rightKey))
            {
                rightHoldTimer += Time.deltaTime;
                if (rightHoldTimer >= dasDelay)
                {
                    Move(Vector3.right);
                    rightHoldTimer = dasDelay - arrDelay;
                }
            }
            else
            {
                rightHoldTimer = 0f;
            }
        }
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