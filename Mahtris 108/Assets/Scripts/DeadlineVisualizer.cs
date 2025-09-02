// FileName: DeadlineVisualizer.cs
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class DeadlineVisualizer : MonoBehaviour
{
    [Header("≈‰÷√")]
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
            Debug.LogError("DeadlineVisualizer: GameSettings Œ¥±ª∏≥÷µ!");
            return;
        }
        DrawDeadline();
    }

    private void DrawDeadline()
    {
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;

        float y = settings.deadlineHeight - 0.5f;
        float x_start = -0.5f;
        float x_end = settings.gridWidth - 0.5f;

        lineRenderer.SetPosition(0, new Vector3(x_start, y, 0));
        lineRenderer.SetPosition(1, new Vector3(x_end, y, 0));
    }
}