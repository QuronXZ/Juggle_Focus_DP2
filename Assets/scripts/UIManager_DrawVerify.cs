using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

public class UIManager_DrawVerify : MonoBehaviour
{
    List<List<Vector2>> truthCache;
    public int truthSampleCount = 128;
    public int compareResampleCount = 64;


    [Header("References")]
    public line_input drawInput;      // assign draw area (RawImage object)
    public pattern_move_mgr patternManager;        // your pattern manager
    public Canvas uiCanvas;                        // assign your UI Canvas
    public Camera uiCamera;                        // assign if canvas uses ScreenSpace-Camera (else leave null)

    [Header("UI")]
    public Button redButton, yellowButton, blueButton, endGameButton;
    public Image redThumb, yellowThumb, blueThumb;
    public TextMeshProUGUI feedbackText;
    public float successThreshold = 0.6f; // 60%

    // active mapping: color -> index in patternManager.ballPatterns
    public int redIndex = 0;
    public int yellowIndex = 1;
    public int blueIndex = 2;
    [Header("Time Tracking")]
    public ButtonTimeTracker timeTracker;


    int activeIndex = -1; // which ball is currently selected to draw for

    void Start()
    {
        redButton.onClick.AddListener(() => SetActiveColor(redIndex));
        yellowButton.onClick.AddListener(() => SetActiveColor(yellowIndex));
        blueButton.onClick.AddListener(() => SetActiveColor(blueIndex));
        endGameButton.onClick.AddListener(EndGame);
        // default highlight none
        SetActiveColor(-1);

        // build cache
        truthCache = new List<List<Vector2>>(patternManager.ballPatterns.Count);
        for (int i = 0; i < patternManager.ballPatterns.Count; i++)
        {
            truthCache.Add(patternManager.SamplePatternScreenNormalized(patternManager.ballPatterns[i], truthSampleCount));
        }

    }

    void Update()
    {
        // detect drawing end: user releases (drawInput sets isDrawing false on pointer up)
        if (!drawInput.isDrawing && drawInput.currentStroke != null && drawInput.currentStroke.Count > 1)
        {
            // if there is a newly finished stroke and active color selected
            if (activeIndex >= 0)
            {
                TryEvaluateDrawing(activeIndex);
            }
            // clear stroke buffer to avoid repeating until new draw
            drawInput.currentStroke = new List<Vector2>();      
        }
    }
    
    public void SetActiveColor(int patternIndex)
    {
        timeTracker.OnButtonPressed(patternIndex);  // Notify time tracker about button press
        activeIndex = patternIndex;
        // visual highlight can be added here (change button color)
        feedbackText.text = (activeIndex >= 0) ? "Draw now for selected color" : "";
    }

    // Add this method to end the game
    void EndGame()
    {
        if (timeTracker != null)
        {
            timeTracker.EndAllTimers();
        }

        // Add any other end-game logic here
        feedbackText.text = "Game Over! Check time results.";
    }

    // convert true pattern to normalized canvas coordinates (0..1)
    List<Vector2> GetTruePatternNormalized(int ballPatternIndex)
    {
        if (patternManager == null || ballPatternIndex < 0 || ballPatternIndex >= patternManager.ballPatterns.Count) return null;
        var bp = patternManager.ballPatterns[ballPatternIndex];
        if (bp.pattern == null) return null;

        List<Vector2> outPts = new List<Vector2>();

        // For each pattern point (x,y) which we treat as X -> local X, Y -> local Z in world
        foreach (var p in bp.pattern.points)
        {
            // world position
            Vector3 world = patternManager.center.position + new Vector3(p.x * bp.pattern.scale, 0f, p.y * bp.pattern.scale);
            // convert world to screen point
            Vector3 screen = Camera.main.WorldToScreenPoint(world);

            // convert screen to local point in drawInput.rectTransform
            RectTransform drawRect = drawInput.GetComponent<RectTransform>();
            Vector2 localPoint;
            bool ok = RectTransformUtility.ScreenPointToLocalPointInRectangle(drawRect, screen, (uiCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : uiCamera), out localPoint);
            if (!ok)
            {
            //    // fallback: project normalized using screen coords
                Vector2 norm = new Vector2(screen.x / Screen.width, screen.y / Screen.height);
                outPts.Add(norm);
            }
            else
            {
                Rect r = drawRect.rect;
                float nx = (localPoint.x - r.x) / r.width;
                float ny = (localPoint.y - r.y) / r.height;
                outPts.Add(new Vector2(Mathf.Clamp01(nx), Mathf.Clamp01(ny)));
            }
        }
        return outPts;
    }

    // Evaluate the drawing for the selected ball/pattern
    void TryEvaluateDrawing(int ballPatternIndex)
    {
       /* if (drawInput.currentStroke == null || drawInput.currentStroke.Count < 4)
        {
            feedbackText.text = "Draw more clearly.";
            return;
        }

        List<Vector2> playerPath = new List<Vector2>(drawInput.currentStroke); // already normalized 0..1 by DrawInputManager
        List<Vector2> truth = GetTruePatternNormalized(ballPatternIndex);
        if (truth == null || truth.Count < 2)
        {
            feedbackText.text = "Pattern data missing.";
            return;
        }

        float score = PatternComparer.ComparePaths(playerPath, truth, 64, 0.8f);
        float percent = score; // 0..1

        if (percent >= successThreshold)
        {
            feedbackText.text = "✅ Correct! (" + Mathf.RoundToInt(percent * 100f) + "%)";
            // create thumbnail sprite from drawInput texture and assign to thumbnail
            //Texture2D copy = drawInput.GetTextureCopy();
            //Sprite s = Sprite.Create(copy, new Rect(0, 0, copy.width, copy.height), new Vector2(0.5f, 0.5f));
            //AssignThumbnailToIndex(ballPatternIndex, s);
        }
        else
        {
            feedbackText.text = "❌ Try again (" + Mathf.RoundToInt(percent * 100f) + "%)";
            //AssignThumbnailToIndex(ballPatternIndex, null);
        }*/

        List<Vector2> playerPath = new List<Vector2>(drawInput.currentStroke);
        if (playerPath == null || playerPath.Count < 4) { feedbackText.text = "Draw more clearly."; return; }

        var truth = truthCache[ballPatternIndex];
        if (truth == null || truth.Count < 2) { feedbackText.text = "Pattern data missing."; return; }

        float similarity = Gesture_recog.Compare(playerPath, truth, compareResampleCount);
        Debug.Log($"Similarity score: {similarity}");
        // update UI
        int percent = Mathf.RoundToInt(similarity * 100f);
        if (similarity >= successThreshold)
        {
            feedbackText.text = $"✅ Correct! ({percent}%)";
            // optionally capture thumbnail:
            Texture2D tex = drawInput.CaptureScreenTexture();
            Sprite s = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            AssignThumbnailToIndex(ballPatternIndex, s);
        }
        else
        {
            feedbackText.text = $"❌ Try again ({percent}%)";
            AssignThumbnailToIndex(ballPatternIndex, null);
        }

        // clear line
        drawInput.ClearLine();


        // clear drawing area for next try
        drawInput.ClearLine();
    }

    void AssignThumbnailToIndex(int idx, Sprite s)
    {
        if (idx == redIndex) redThumb.sprite = s;
        else if (idx == yellowIndex) yellowThumb.sprite = s;
        else if (idx == blueIndex) blueThumb.sprite = s;
    }
}
