// FileName: DeadlineVisualizer.cs
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class DeadlineVisualizer : MonoBehaviour
{
    [Header("配置")]
    [SerializeField] private GameSettings settings;

    [Header("外观")]
    [Tooltip("死亡线的颜色")]
    [SerializeField] private Color deadlineColor = new Color(0.6f, 0.4f, 0.2f, 1f); // 默认褐色
    [Tooltip("线条宽度")]
    [SerializeField] private float lineWidth = 0.1f;

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
        // 设置颜色
        // LineRenderer 需要材质支持颜色，如果用的是默认材质可能颜色不生效
        // 建议在 Start 里强制设置一个简单材质，或者确保 Inspector 里配好了
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = deadlineColor;
        lineRenderer.endColor = deadlineColor;

        // 设置点位
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;

        float y = settings.deadlineHeight - 0.5f;
        float x_start = -0.5f;
        float x_end = settings.gridWidth - 0.5f;

        lineRenderer.SetPosition(0, new Vector3(x_start, y, 0));
        lineRenderer.SetPosition(1, new Vector3(x_end, y, 0));
    }
}