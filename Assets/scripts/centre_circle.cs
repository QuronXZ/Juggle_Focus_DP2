using UnityEngine;

public class centre_circle : MonoBehaviour
{
    [Range(0.1f, 10f)]
    public float radius = 3f;
    [Range(10, 200)]
    public int segments = 100;
    public Color lineColor = Color.white;
    public float lineWidth = 0.05f;

    private LineRenderer lineRenderer;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.loop = true;
        lineRenderer.useWorldSpace = false;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.positionCount = segments;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;

        DrawCircle();
    }

    void OnValidate()
    {
        if (lineRenderer != null)
            DrawCircle();
    }

    void DrawCircle()
    {
        Vector3[] points = new Vector3[segments];
        for (int i = 0; i < segments; i++)
        {
            float angle = 2 * Mathf.PI * i / segments;
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;
            points[i] = new Vector3(x, y, 0);
        }
        lineRenderer.positionCount = segments;
        lineRenderer.SetPositions(points);
    }
}
