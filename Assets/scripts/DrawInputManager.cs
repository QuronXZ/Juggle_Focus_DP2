using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DrawInputManager : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Canvas / Appearance")]
    public int textureWidth = 512;
    public int textureHeight = 512;
    public Color drawColor = Color.white;
    public int brushSize = 6;
    public bool clearOnStart = true;

    [Header("Runtime")]
    public Texture2D drawTexture { get; private set; }
    public List<Vector2> currentStroke = new List<Vector2>(); // points in normalized local (0..1) canvas space
    public bool isDrawing { get; private set; } = false;

    // References
    RawImage rawImage;
    RectTransform rectTransform;
    Canvas rootCanvas;

    void Awake()
    {
        rawImage = GetComponent<RawImage>();
        rectTransform = rawImage.rectTransform;
        rootCanvas = GetComponentInParent<Canvas>();
        CreateTexture();
    }

    void CreateTexture()
    {
        drawTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
        drawTexture.wrapMode = TextureWrapMode.Clamp;
        if (clearOnStart) ClearTexture();
        rawImage.texture = drawTexture;
    }

    public void ClearTexture()
    {
        Color32[] cols = new Color32[textureWidth * textureHeight];
        Color32 bg = new Color32(0, 0, 0, 0); // transparent
        for (int i = 0; i < cols.Length; i++) cols[i] = bg;
        drawTexture.SetPixels32(cols);
        drawTexture.Apply();
        currentStroke.Clear();
    }

    // IPointerDownHandler
    public void OnPointerDown(PointerEventData eventData)
    {
        isDrawing = true;
        currentStroke.Clear();
        Vector2 local;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position,
            rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera, out local))
        {
            Vector2 normalized = LocalPointToNormalized(local);
            currentStroke.Add(normalized);
            DrawDotOnTexture(normalized);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDrawing) return;
        Vector2 local;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position,
            rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera, out local))
        {
            Vector2 normalized = LocalPointToNormalized(local);
            // only add if distance > tiny threshold
            if (currentStroke.Count == 0 || Vector2.Distance(currentStroke[currentStroke.Count - 1], normalized) > 0.005f)
            {
                currentStroke.Add(normalized);
                DrawLineOnTexture(currentStroke[currentStroke.Count - 2], normalized);
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDrawing = false;
    }

    // convert local (RectTransform local) to normalized 0..1
    Vector2 LocalPointToNormalized(Vector2 local)
    {
        Rect r = rectTransform.rect;
        float nx = (local.x - r.x) / r.width;   // 0..1
        float ny = (local.y - r.y) / r.height;  // 0..1
        // flip Y so lower-left is (0,0) for texture
        return new Vector2(Mathf.Clamp01(nx), Mathf.Clamp01(ny));
    }

    // draw a single pixel / small disc
    void DrawDotOnTexture(Vector2 normalized)
    {
        int x = Mathf.RoundToInt(normalized.x * (textureWidth - 1));
        int y = Mathf.RoundToInt(normalized.y * (textureHeight - 1));
        DrawFilledCircle(x, y, brushSize, drawColor);
        drawTexture.Apply();
    }

    void DrawLineOnTexture(Vector2 aNorm, Vector2 bNorm)
    {
        int x0 = Mathf.RoundToInt(aNorm.x * (textureWidth - 1));
        int y0 = Mathf.RoundToInt(aNorm.y * (textureHeight - 1));
        int x1 = Mathf.RoundToInt(bNorm.x * (textureWidth - 1));
        int y1 = Mathf.RoundToInt(bNorm.y * (textureHeight - 1));
        DrawLineBresenham(x0, y0, x1, y1, brushSize, drawColor);
        drawTexture.Apply();
    }

    // simple filled circle
    void DrawFilledCircle(int cx, int cy, int r, Color col)
    {
        int sqrR = r * r;
        for (int dx = -r; dx <= r; dx++)
        {
            int dyMax = Mathf.FloorToInt(Mathf.Sqrt(sqrR - dx * dx));
            for (int dy = -dyMax; dy <= dyMax; dy++)
            {
                int px = cx + dx, py = cy + dy;
                if (px >= 0 && px < textureWidth && py >= 0 && py < textureHeight)
                    drawTexture.SetPixel(px, py, col);
            }
        }
    }

    // Bresenham line with thick brush
    void DrawLineBresenham(int x0, int y0, int x1, int y1, int thickness, Color col)
    {
        int dx = Mathf.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
        int dy = -Mathf.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
        int err = dx + dy, e2;
        while (true)
        {
            DrawFilledCircle(x0, y0, thickness, col);
            if (x0 == x1 && y0 == y1) break;
            e2 = 2 * err;
            if (e2 >= dy) { err += dy; x0 += sx; }
            if (e2 <= dx) { err += dx; y0 += sy; }
        }
    }

    // Utility: return a copy of the texture (use for thumbnail capture)
    public Texture2D GetTextureCopy()
    {
        Texture2D copy = new Texture2D(drawTexture.width, drawTexture.height, drawTexture.format, false);
        copy.SetPixels32(drawTexture.GetPixels32());
        copy.Apply();
        return copy;
    }
}
