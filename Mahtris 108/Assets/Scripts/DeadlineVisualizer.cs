// FileName: DeadlineVisualizer.cs
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class DeadlineVisualizer : MonoBehaviour
{
    [Header("配置")]
    [SerializeField] private GameSettings settings;

    private LineRenderer lineRenderer;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Start()
    {
        if (settings == null)
        {
            Debug.LogError("DeadlineVisualizer: GameSettings 未被赋值!");
            return;
        }

        DrawDeadline();
    }

    private void DrawDeadline()
    {
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;

        // 线的Y坐标在格子的分界线上，所以是 deadlineHeight - 0.5
        float y = settings.deadlineHeight - 0.5f;

        // 线的X坐标从左边界-0.5到右边界-0.5
        float x_start = -0.5f;
        float x_end = settings.gridWidth - 0.5f;

        lineRenderer.SetPosition(0, new Vector3(x_start, y, 0));
        lineRenderer.SetPosition(1, new Vector3(x_end, y, 0));
    }
}