// FileName: Tetromino.cs
using UnityEngine;

public class Tetromino : MonoBehaviour
{
    [Header("�淨����")]
    [Tooltip("�÷������Ͷ�Ӧ�ġ����ⱶ�ʡ�ֵ")]
    public float extraMultiplier = 1f;

    [Header("UI��ʾ")]
    [Tooltip("������Tetromino�б�����ʾ�ġ�UI��Ԥ�Ƽ���")]
    public GameObject uiPrefab;

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
        // ---�������㡿---
        // ����Ч�����Ƶ���ǰ�棬ȷ��������ת�Ƿ�ɹ�������Ч����
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

