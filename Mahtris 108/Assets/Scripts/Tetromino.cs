using UnityEngine;

public class Tetromino : MonoBehaviour
{
    private float lastFallTime;
    private float fallSpeed;
    private float fastFallMultiplier;
    private TetrisGrid tetrisGrid;

    public void Initialize(GameSettings settings, TetrisGrid grid)
    {
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

    #region Movement Methods
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

    void Landed()
    {
        enabled = false;
        tetrisGrid.UpdateGrid(transform); // Final grid update after landing
        tetrisGrid.CheckForFullRows();
    }
    #endregion
}
