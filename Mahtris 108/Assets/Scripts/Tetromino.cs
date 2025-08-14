// FileName: Tetromino.cs
using UnityEngine;

public class Tetromino : MonoBehaviour
{
    [Header("�淨����")]
    [Tooltip("�÷������Ͷ�Ӧ�ġ����ⱶ�ʡ�ֵ")]
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
            // �����㱻ռ��ֱ����Ϸ����
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

        // ---�������߼���---
        // ����Ƿ�����������
        foreach (Transform child in transform)
        {
            if (Mathf.RoundToInt(child.position.y) >= settings.deadlineHeight)
            {
                GameEvents.TriggerGameOver();
                return; // ��Ϸ�ѽ��������ٽ��������ж�
            }
        }

        tetrisGrid.CheckForFullRows();
    }

    // Movement Methods (HandleMovementInput, Move, Rotate) ���ֲ���...
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