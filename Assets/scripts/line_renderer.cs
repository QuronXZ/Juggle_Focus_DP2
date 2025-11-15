using System.Collections.Generic;
using UnityEngine;

public class line_renderer : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public Camera mainCamera;

    private List<Vector3> drawnPoints = new List<Vector3>();
    private bool isDrawing = false;

    private ButtonTimeTracker timeTracker;

    void Start()
    {
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        if (mainCamera == null)
            mainCamera = Camera.main;

        timeTracker = FindObjectOfType<ButtonTimeTracker>();
        // Set appearance
        lineRenderer.positionCount = 0;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;
    }

    void Update()
    {
        // Start drawing when left mouse is held
        if (Input.GetMouseButtonDown(0))
        {
            StartDrawing();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            StopDrawing();
        }

        if (isDrawing)
        {
            DrawLine();
        }
    }

    void StartDrawing()
    {
        isDrawing = true;
        drawnPoints.Clear();
        lineRenderer.positionCount = 0;

        timeTracker.OnDrawingStarted();
    }

    void StopDrawing()
    {
        isDrawing = false;

        if (timeTracker != null)
        {
            // You can pass whether the drawing was successful here
            // For now, we'll assume all drawings are attempts (success determined later)
            timeTracker.OnDrawingEnded(false); // Set to true if pattern was correct
        }

        // here you can save drawnPoints as a pattern for comparison
        Debug.Log("Pattern drawn with " + drawnPoints.Count + " points");
    }
    // Call this method when you know if the drawing was successful
    public void MarkDrawingAsSuccessful(bool successful)
    {
        if (timeTracker != null)
        {
            timeTracker.OnDrawingEnded(successful);
        }
    }

    void DrawLine()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10f; // distance from camera
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);

        if (drawnPoints.Count == 0 || Vector3.Distance(worldPos, drawnPoints[drawnPoints.Count - 1]) > 0.05f)
        {
            drawnPoints.Add(worldPos);
            lineRenderer.positionCount = drawnPoints.Count;
            lineRenderer.SetPositions(drawnPoints.ToArray());
        }
    }

    public List<Vector3> GetDrawnPoints()
    {
        return drawnPoints;
    }

    public void ClearDrawing()
    {
        drawnPoints.Clear();
        lineRenderer.positionCount = 0;
    }
}
