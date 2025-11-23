using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(LineRenderer))]
public class line_input : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Line Settings")]
    public LineRenderer lineRenderer;           // assign in inspector or will get one on same object
    public float lineWidth = 0.025f;
    public Color lineColor = Color.white;
    public float minPointDistance = 0.01f;      // in world units for line renderer

    [Header("UI / Camera")]
    public RectTransform inputRect;             // the UI rect that should receive input (assign same object or parent)
    public Canvas canvas;                       // assign canvas (optional). if null we'll find one
    public Camera uiCamera;                     // required only if canvas is ScreenSpace - Camera

    [Header("State (read-only)")]
    public bool isDrawing { get; private set; }
    public List<Vector2> currentStroke = new List<Vector2>(); // normalized screen space (0..1)
    List<Vector3> worldPoints = new List<Vector3>();         // world positions for lineRenderer

    // fallback option: allow mouse input even if pointer events don't come through
    public bool allowFallbackMouseInput = true;

    void Awake()
    {
        if (lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            Debug.LogError("LineInput requires a LineRenderer. Add one to the GameObject.", this);
            enabled = false;
            return;
        }

        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
        }

        if (inputRect == null)
        {
            // try to use this object's RectTransform
            var rt = GetComponent<RectTransform>();
            if (rt != null) inputRect = rt;
        }

        // set up visual
        lineRenderer.positionCount = 0;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;

        // If canvas is ScreenSpace-Camera, find camera
        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera && uiCamera == null)
            uiCamera = canvas.worldCamera;
    }

    void Update()
    {
        // fallback mouse drawing (for testing in case EventSystem isn't hitting or for non-UI usage)
        if (allowFallbackMouseInput && inputRect != null)
        {
            if (!isDrawing && Input.GetMouseButtonDown(0))
            {
                // check if mouse is over inputRect
                Vector2 local;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(inputRect, Input.mousePosition, (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? uiCamera : null, out local))
                {
                    OnPointerDown_Fallback(Input.mousePosition);
                }
            }
            else if (isDrawing && Input.GetMouseButton(0))
            {
                OnDrag_Fallback(Input.mousePosition);
            }
            else if (isDrawing && Input.GetMouseButtonUp(0))
            {
                OnPointerUp_Fallback(Input.mousePosition);
            }
        }
    }

    // IPointerDownHandler
    public void OnPointerDown(PointerEventData eventData)
    {
        // Only handle if pointer is inside inputRect (should be true)
        StartStroke(eventData.position);
    }

    // IDragHandler
    public void OnDrag(PointerEventData eventData)
    {
        AddPoint(eventData.position);
    }

    // IPointerUpHandler
    public void OnPointerUp(PointerEventData eventData)
    {
        EndStroke();
    }

    // Fallback handlers use same internals
    void OnPointerDown_Fallback(Vector2 screenPos) => StartStroke(screenPos);
    void OnDrag_Fallback(Vector2 screenPos) => AddPoint(screenPos);
    void OnPointerUp_Fallback(Vector2 screenPos) => EndStroke();

    void StartStroke(Vector2 screenPos)
    {
        isDrawing = true;
        currentStroke.Clear();
        worldPoints.Clear();
        lineRenderer.positionCount = 0;
        AddPoint(screenPos);
    }

    void EndStroke()
    {
        isDrawing = false;
        // stroke finished — user of this script (UIManager_DrawVerify) should read currentStroke and process it
        // Keep the line visible until ClearLine() is called
    }

    void AddPoint(Vector2 screenPosition)
    {
        if (inputRect == null)
        {
            Debug.LogWarning("LineInput: inputRect is null. Assign a RectTransform to use UI hit testing.", this);
            return;
        }

        // Convert to local point in the rect to check if inside bounds
        Vector2 localPoint;
        bool inside = RectTransformUtility.ScreenPointToLocalPointInRectangle(inputRect, screenPosition,
            (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? uiCamera : null, out localPoint);

        if (!inside)
        {
            // outside the rect, ignore (helps when dragging outside the input area)
            return;
        }

        // Compute normalized 0..1 coordinate relative to rect
        Rect r = inputRect.rect;
        float nx = (localPoint.x - r.x) / r.width;
        float ny = (localPoint.y - r.y) / r.height;
        Vector2 normalized = new Vector2(Mathf.Clamp01(nx), Mathf.Clamp01(ny));

        // If this is the first point or sufficiently far from last, add it
        if (currentStroke.Count == 0 || Vector2.Distance(currentStroke[currentStroke.Count - 1], normalized) > 0.005f)
        {
            currentStroke.Add(normalized);

            // Convert screen to world for lineRenderer positioning
            Camera cam = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? uiCamera : Camera.main;
            if (cam == null) cam = Camera.main;

            Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, (cam.transform.position - inputRect.transform.position).magnitude));
            // Note: z is important so line appears in front of UI if needed; tweak if required

            // Only add world point if it's far enough from last world point
            if (worldPoints.Count == 0 || Vector3.Distance(worldPoints[worldPoints.Count - 1], worldPos) > minPointDistance)
            {
                worldPoints.Add(worldPos);
                lineRenderer.positionCount = worldPoints.Count;
                lineRenderer.SetPositions(worldPoints.ToArray());
            }
        }
    }

    // Clears the visible line and stored stroke
    public void ClearLine()
    {
        lineRenderer.positionCount = 0;
        currentStroke.Clear();
        worldPoints.Clear();
    }

    // For thumbnail capture: render line to texture (basic)
    // This is simple: capture full screen and return texture. You can crop later in UIManager.
    public Texture2D CaptureScreenTexture()
    {
        // Note: this uses ScreenCapture (reads back GPU); heavy but fine for thumbnails occasionally.
        Texture2D tex = ScreenCapture.CaptureScreenshotAsTexture();
        return tex;
    }
}
