using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine.UI;               // if using Unity UI
using TMPro;                    // uncomment if using TextMeshPro

public class pattern_quiz_mgr2 : MonoBehaviour


{
    [Header("References")]
    public pattern_move_mgr patternManager;      // assign your pattern_move_mgr instance
    public RectTransform targetContainer;       // UI container where preview instance spawns
    //public Text targetLabel;                    // UI Text to show instruction (or use TMP)
    public TextMeshProUGUI targetLabel;      // if using TMPro, comment previous and uncomment this

    [Header("Preview")]
    public GameObject previewBallPrefab;        // prefab for preview (SpriteRenderer)
    private GameObject currentPreview;

    [Header("Buttons")]
    public Button[] optionButtons = new Button[3];   // assign 3 buttons in inspector
    public TextMeshProUGUI[] optionButtonLabels;
    //public Text[] optionButtonLabels;            // labels inside buttons (or TextMeshProUGUI)
    // if using TMP: public TextMeshProUGUI[] optionButtonLabels;

    [Header("Results UI")]
    public GameObject resultsPanel;              // panel to activate at end
    //public Text resultsText;                     // summary text
    public TextMeshProUGUI resultsText;

    // internal quiz state
    private List<pattern_move_mgr.BallPattern> queue = new List<pattern_move_mgr.BallPattern>();
    private List<AnswerRecord> answers = new List<AnswerRecord>();
    private pattern_move_mgr.BallPattern currentBP;

    [Header("Pattern Images")]
    public List<Sprite> patternSpritesList;
    private Dictionary<string, Sprite> patternSprites;

    [System.Serializable]
    public class AnswerRecord
    {
        public string expected;
        public string chosen;
        public bool correct;
    }

    void Start()
    {
        if (patternManager == null)
        {
            patternManager = FindObjectOfType<pattern_move_mgr>();
            if (patternManager == null) Debug.LogError("Pattern Manager not found in scene.");
        }

        // initialize labels array if not set
        if (optionButtonLabels == null || optionButtonLabels.Length != optionButtons.Length)
        {
            optionButtonLabels = new TextMeshProUGUI[optionButtons.Length];
            for (int i = 0; i < optionButtons.Length; i++)
            {
                optionButtonLabels[i] = optionButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            }
        }

        // populate queue with a shallow copy
        queue = new List<pattern_move_mgr.BallPattern>(patternManager.ballPatterns);

        // shuffle queue for randomness
        Shuffle(queue);

        // attach click listeners to buttons (index captures)
        for (int i = 0; i < optionButtons.Length; i++)
        {
            int idx = i;
            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() => OnOptionSelected(idx));
        }

        // hide results panel initially
        if (resultsPanel != null) resultsPanel.SetActive(false);

        // start first trial
        NextTrial();
    }

    void NextTrial()
    {
        // clear existing preview
        if (currentPreview != null) Destroy(currentPreview);

        if (queue.Count == 0)
        {
            ShowResults();
            return;
        }

        // pop next ball
        currentBP = queue[0];
        queue.RemoveAt(0);

        // create preview instance at UI container
        currentPreview = Instantiate(previewBallPrefab, targetContainer);
        // place visually centered
        RectTransform rt = currentPreview.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchoredPosition = Vector2.zero;
            rt.localScale = Vector3.one * 1f;
        }
        else
        {
            // if preview is world-space prefab (Sprite), ensure it's inside a UI world-space canvas or use a camera overlay
            // For simplicity, ensure previewBallPrefab is a UI prefab (Image) or use ConvertWorldToUISpace if needed.
            currentPreview.transform.localPosition = Vector3.zero;
        }

        // copy appearance from live ball (attempt)
        CopyAppearanceFromBall(currentBP.ball, currentPreview);

        // set label
        string expectedName = GetPatternName(currentBP);
        //if (targetLabel != null) targetLabel.text = $"Focus on this ball — pattern: {expectedName}";
        if (targetLabel != null) targetLabel.text = "Focus on this ball :";


        // populate option button texts (one correct + two distractors)
        List<string> allNames = GetAllPatternNames();
        // remove duplicates and ensure expected exists
        //allNames = allNames.Distinct().ToList();
        // ensure expected included
        if (!allNames.Contains(expectedName)) allNames.Add(expectedName);

       /* // pick distractors
        List<string> distractors = allNames.Where(n => n != expectedName).ToList();
        Shuffle(distractors);

        // build options list with correct + two distractors
        List<string> options = new List<string>();
        options.Add(expectedName);
        for (int i = 0; i < optionButtons.Length - 1; i++)
        {
            if (i < distractors.Count) options.Add(distractors[i]);
            else options.Add("Unknown");
        }

        // shuffle options so correct is not always first
        Shuffle(options);*/

            // Build distractor list (all names except correct)
        List<string> distractors = allNames
            .Where(n => n != expectedName)
            .Distinct()
            .ToList();

        // If not enough distractors, just reshuffle full list
        if (distractors.Count < optionButtons.Length - 1)
        {
            distractors = allNames
                .Where(n => n != expectedName)
                .Distinct()
                .ToList();
        }

        // Shuffle distractors
        Shuffle(distractors);

        // Build exactly N options = 3 (1 correct + 2 random distractors)
        List<string> options = new List<string>();
        options.Add(expectedName);
        options.Add(distractors[0]);
        options.Add(distractors[1]);

        // shuffle final options
        Shuffle(options);


        // assign to buttons
        for (int i = 0; i < optionButtons.Length; i++)
        {
            Debug.Log($"Button {i} assigned? => {optionButtons[i]}");
  
            string label = (i < options.Count) ? options[i] : "—";
            if (optionButtonLabels[i] != null) optionButtonLabels[i].text = label;
            optionButtons[i].interactable = true;
        }
    }

    void OnOptionSelected(int buttonIndex)
    {
        string chosenLabel = optionButtonLabels[buttonIndex].text;
        string expected = GetPatternName(currentBP);

        bool correct = chosenLabel == expected;
        // record answer
        answers.Add(new AnswerRecord() { expected = expected, chosen = chosenLabel, correct = correct });

        // optionally provide immediate feedback (color button)
        StartCoroutine(ShowFeedbackThenNext(buttonIndex, correct));
    }

    IEnumerator ShowFeedbackThenNext(int buttonIndex, bool correct)
    {
        // disable all buttons to prevent double press
        foreach (var b in optionButtons) b.interactable = false;
        // color feedback (simple)
        Color original = optionButtonLabels[buttonIndex].color;
        optionButtonLabels[buttonIndex].color = correct ? Color.green : Color.red;

        // wait a short moment
        yield return new WaitForSeconds(0.6f);

        // reset color
        optionButtonLabels[buttonIndex].color = original;

        // move to next trial
        NextTrial();
    }

    void ShowResults()
    {
        if (resultsPanel != null) resultsPanel.SetActive(true);
        int correctCount = answers.Count(a => a.correct);
        int total = answers.Count;
        if (resultsText != null) resultsText.text = $"You scored {correctCount} / {total} ({(int)(100f * correctCount / Mathf.Max(1, total))}%)";

        // you can also populate a full details list here if you want
        // disable UI buttons area to avoid further input
        foreach (var b in optionButtons) b.gameObject.SetActive(false);

        // destroy preview if any
        if (currentPreview != null) Destroy(currentPreview);
    }

    // utility: copy simple appearance from a live ball to preview
    /*  void CopyAppearanceFromBall(ball_script source, GameObject preview)
      {
          if (source == null || preview == null) return;

          // try to match color if SpriteRenderer exists
          SpriteRenderer srcSr = source.GetComponent<SpriteRenderer>();
          SpriteRenderer dstSr = preview.GetComponent<SpriteRenderer>();
          if (srcSr != null && dstSr != null)
          {
              dstSr.color = srcSr.color;
              dstSr.sprite = srcSr.sprite;
          }
          // if preview uses Image (UI), try that
          Image dstImg = preview.GetComponentInChildren<Image>();
          if (dstImg != null)
          {
              if (srcSr != null)
              {
                  dstImg.color = srcSr.color;
                  // if you want sprite -> convert to UI sprite as needed
              }
          }
      }*/

    void CopyAppearanceFromBall(ball_script source, GameObject preview)
    {
        if (source == null || preview == null) return;

        // ---- 3D Sphere Material Copy ----
        MeshRenderer srcMR = source.GetComponent<MeshRenderer>();
        MeshRenderer dstMR = preview.GetComponent<MeshRenderer>();

        if (srcMR != null && dstMR != null)
        {
            // Copy material instance so preview looks exactly the same
            dstMR.material = srcMR.material;
            return;
        }

/*        // ---- Fall back to SpriteRenderer (2D case) ----
        SpriteRenderer srcSr = source.GetComponent<SpriteRenderer>();
        SpriteRenderer dstSr = preview.GetComponent<SpriteRenderer>();

        if (srcSr != null && dstSr != null)
        {
            dstSr.sprite = srcSr.sprite;
            dstSr.color = srcSr.color;
            return;
        }*/

/*        // ---- Fallback: UI Image (if preview is a UI element) ----
        Image dstImg = preview.GetComponentInChildren<Image>();
        if (dstImg != null && srcSr != null)
        {
            dstImg.color = srcSr.color;
        }*/
    }


    string GetPatternName(pattern_move_mgr.BallPattern bp)
    {
        if (bp == null || bp.pattern == null) return "(unknown)";
        return bp.pattern.patternName;
    }

    List<string> GetAllPatternNames()
    {
        /*var names = new List<string>();
        foreach (var bp in patternManager.ballPatterns)
        {
            if (bp != null && bp.pattern != null) names.Add(bp.pattern.patternName);
        }
        return names;*/
        return patternManager.patternNames;

    }

    // Fisher-Yates shuffle
    void Shuffle<T>(IList<T> list)
    {
        for (int i = 0; i < list.Count - 1; i++)
        {
            int r = Random.Range(i, list.Count);
            T tmp = list[r];
            list[r] = list[i];
            list[i] = tmp;
        }
    }
}
