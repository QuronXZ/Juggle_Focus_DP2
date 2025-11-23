using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine.UI;               // if using Unity UI
using TMPro;
using UnityEngine.UIElements;
using Unity.VisualScripting;                    // uncomment if using TextMeshPro
using UnityEngine.SceneManagement;
public class pattern_quiz_mgr : MonoBehaviour


{
    [Header("References")]
    public pattern_move_mgr patternManager;      // assign your pattern_move_mgr instance
    public Transform targetTransform;

    //public RectTransform targetContainer;       // UI container where preview instance spawns
    //public Text targetLabel;                    // UI Text to show instruction (or use TMP)
    public TextMeshProUGUI targetLabel;      // if using TMPro, comment previous and uncomment this

    [Header("Preview")]
    public GameObject previewBallPrefab;        // prefab for preview (SpriteRenderer)
    private GameObject currentPreview;
    public List<Material> ballMaterials;

    [Header("Buttons")]
    public UnityEngine.UI.Button[] optionButtons = new UnityEngine.UI.Button[3];   // assign 3 buttons in inspector
    public TextMeshProUGUI[] optionButtonLabels;
    //public Text[] optionButtonLabels;            // labels inside buttons (or TextMeshProUGUI)
    // if using TMP: public TextMeshProUGUI[] optionButtonLabels;

    [Header("Results UI")]
    public GameObject basePanel;
    public GameObject resultsPanel;              // panel to activate at end
    //public Text resultsText;                     // summary text
    public TextMeshProUGUI resultsText;
    public star_controller starController;

    public feedback_anim feedbackAnimator;

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
        // Get reference to movement manager
        var pm = FindObjectOfType<pattern_move_mgr>();

        // pm.patternList is the correct pattern names list
        patternSprites = new Dictionary<string, Sprite>();

        for (int i = 0; i < pm.patternNames.Count && i < patternSpritesList.Count; i++)
        {
            string name = pm.patternNames[i];
            Sprite sprite = patternSpritesList[i];

            if (!patternSprites.ContainsKey(name))
                patternSprites.Add(name, sprite);
        }


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

    /* void NextTrial()
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

        *//* // pick distractors
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
         Shuffle(options);*//*

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
 */

    /*   void NextTrial()
       {
           // destroy old preview
           if (currentPreview != null) Destroy(currentPreview);

           if (queue.Count == 0)
           {
               ShowResults();
               return;
           }

           // pop next ball
           currentBP = queue[0];
           queue.RemoveAt(0);

           // --- instantiate preview at a world Transform position ---
           if (targetTransform == null)
           {
               Debug.LogError("pattern_quiz_mgr: targetTransform not assigned!");
               return;
           }

           // Choose whether to parent preview to targetTransform (true) or keep as separate world object (false)
           bool parentedToTarget = true;

           currentPreview = previewBallPrefab;     // always use the same one
           currentPreview.SetActive(true);
           *//* // --- DESTROY ANY OLD PREVIEW ---
            for (int i = targetTransform.childCount - 1; i >= 0; i--)
            {
                Destroy(targetTransform.GetChild(i).gameObject);
            }

            if (parentedToTarget)
            {
                currentPreview = Instantiate(previewBallPrefab, targetTransform, false);
                // ensure local position (zero) and scale
                currentPreview.transform.localPosition = Vector3.zero;
                currentPreview.transform.localRotation = Quaternion.identity;
                currentPreview.transform.localScale = Vector3.one;
            }
            else
            {
                currentPreview = Instantiate(previewBallPrefab, targetTransform.position, targetTransform.rotation);
                currentPreview.transform.localScale = Vector3.one;
            }*/

    /*        // --- Make sure the preview does not block UI raycasts ---
            // If preview is a UI element, disable Graphic.raycastTarget on children.
            var graphics = currentPreview.GetComponentsInChildren<UnityEngine.UI.Graphic>(true);
            foreach (var g in graphics) g.raycastTarget = false;

            // If preview is a 3D object, move it to Ignore Raycast layer so it won't steal UI clicks
            int ignoreLayer = LayerMask.NameToLayer("Ignore Raycast");
            if (ignoreLayer != -1)
            {
                SetLayerRecursively(currentPreview, ignoreLayer);
            }
            else
            {
                // if layer not found, log but continue
                Debug.LogWarning("Ignore Raycast layer not found. Consider creating it (Unity default).");
            }*//*

    // copy appearance from live ball (attempt)
    CopyAppearanceFromBall(currentBP.ball, currentPreview);

    // set label (top UI text)
    string expectedName = GetPatternName(currentBP);
    if (targetLabel != null) targetLabel.text = "Focus on this ball :";

    // build options as before (kept same)
    List<string> allNames = GetAllPatternNames();
    if (!allNames.Contains(expectedName)) allNames.Add(expectedName);

    List<string> distractors = allNames
        .Where(n => n != expectedName)
        .Distinct()
        .ToList();

    // ensure at least two distractors — fallback safe-guards
    while (distractors.Count < 2)
    {
        // add any other name again (may duplicate) — prevents out-of-range
        distractors.Add(allNames.FirstOrDefault(n => n != expectedName) ?? "(none)");
    }

    Shuffle(distractors);

    List<string> options = new List<string>() { expectedName, distractors[0], distractors[1] };
    Shuffle(options);

    // keep the textual option values so OnOptionSelected can compare (if you later switch to images, store mapping)
    //optionValues = options; // make sure you declared: private List<string> optionValues;

    // assign to buttons (text labels)
    for (int i = 0; i < optionButtons.Length; i++)
    {
        string label = (i < options.Count) ? options[i] : "—";
        if (optionButtonLabels != null && optionButtonLabels.Length == optionButtons.Length && optionButtonLabels[i] != null)
            optionButtonLabels[i].text = label;
        optionButtons[i].interactable = true;
    }


    for (int i = 0; i < optionButtons.Length; i++)
    {
        string label = (i < options.Count) ? options[i] : "Unknown";

        // Assign sprite
        if (patternSprites.ContainsKey(label))
        {
            UnityEngine.UI.Image img1 = optionButtons[i].transform.GetChild(1).GetComponent<UnityEngine.UI.Image>();
                //GetObjectInChildren<UnityEngine.UI.Image>();
            //img1.image.sprite = patternSprites[label];
            img1.sprite = patternSprites[label];
            //optionButtons[i].image.sprite = patternSprites[label];
        }
        else
        { Debug.LogWarning("No sprite for " + label); }


        optionButtons[i].interactable = true;
    }
}*/


    void NextTrial()
    {
        if (queue.Count == 0)
        {
            ShowResults();
            return;
        }

        currentBP = queue[0];
        queue.RemoveAt(0);
        // Debug which pattern this trial expects
        Debug.Log("<color=yellow>--- NEW TRIAL ---</color>");
        Debug.Log("Expected Pattern Name = " + currentBP.pattern.patternName);
        Debug.Log("Ball Material on Live Ball = " + currentBP.ball.GetComponent<MeshRenderer>().material.name);

        // enable preview and reuse it
        currentPreview = previewBallPrefab;
        currentPreview.SetActive(true);

        // Copy ball look
        CopyAppearanceFromBall(currentBP.ball, currentPreview);

        // UI label
        if (targetLabel != null)
            targetLabel.text = "Focus on this ball :";

        // Build options (same code as before)
        List<string> allNames = GetAllPatternNames();
        string expectedName = GetPatternName(currentBP);

        if (!allNames.Contains(expectedName)) allNames.Add(expectedName);

        List<string> distractors = allNames.Where(n => n != expectedName).ToList();
        while (distractors.Count < 2)
            distractors.Add(allNames.First(n => n != expectedName));

        Shuffle(distractors);

        List<string> options = new List<string>() { expectedName, distractors[0], distractors[1] };
        Shuffle(options);

        // Apply button icons
        for (int i = 0; i < optionButtons.Length; i++)
        {
            string label = (i < options.Count) ? options[i] : "Unknown";

            optionButtonLabels[i].text = label;   // Still required for logic
            optionButtonLabels[i].alpha = 0f;

            if (patternSprites.ContainsKey(label))
            {
                var img = optionButtons[i].transform.GetChild(1).GetComponent<UnityEngine.UI.Image>();
                img.sprite = patternSprites[label];
            }

            optionButtons[i].interactable = true;
        }
    }



    void OnOptionSelected(int buttonIndex)
    {
        string chosenLabel = optionButtonLabels[buttonIndex].text;
        string expected = GetPatternName(currentBP);

        Debug.Log("<color=orange>Chosen Option:</color> " + chosenLabel);
        Debug.Log("<color=yellow>Expected Pattern:</color> " + expected);

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
        Color og1 = optionButtons[buttonIndex].image.color;
        optionButtons[buttonIndex].image.color = correct ? Color.green : Color.red;

        // play your animation
        if (correct)
            feedbackAnimator.PlayCorrect();
        else
            feedbackAnimator.PlayWrong();


        // wait a short moment
        yield return new WaitForSeconds(0.6f);

        // reset color
        optionButtonLabels[buttonIndex].color = original;
        optionButtons[buttonIndex].image.color= og1;
        // move to next trial
        NextTrial();
    }

    void ShowResults()
    {
        basePanel.SetActive(false);
        previewBallPrefab.SetActive(false);
        if (resultsPanel != null) resultsPanel.SetActive(true);

        Time.timeScale = 0;

        int correctCount = answers.Count(a => a.correct);
        int total = answers.Count;
        if (resultsText != null) resultsText.text = $"You scored {correctCount} / {total} ({(int)(100f * correctCount / Mathf.Max(1, total))}%)";

        // you can also populate a full details list here if you want
        // disable UI buttons area to avoid further input
        foreach (var b in optionButtons) b.gameObject.SetActive(false);

        // destroy preview if any
        if (currentPreview != null) Destroy(currentPreview);


        // ⭐ CALL STAR ANIMATION ⭐
        if (starController != null)
            starController.ShowStars(correctCount, total);
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
        if (source == null || preview == null)
        {
            Debug.LogError("CopyAppearanceFromBall: Source OR Preview is null!");
            return; }

        // ---- 3D Sphere Material Copy ----
        MeshRenderer srcMR = source.GetComponent<MeshRenderer>();
        MeshRenderer dstMR = preview.GetComponent<MeshRenderer>();

        if (srcMR != null && dstMR != null)
        {
            // Copy material instance so preview looks exactly the same
            //dstMR.material = srcMR.material;
            dstMR.material = new Material(srcMR.material);
            return;
        }
        Debug.Log("<color=cyan>Copying Material:</color> " + srcMR.material.name);
        dstMR.material = new Material(srcMR.material);
        Debug.Log("<color=green>Preview Material Set To:</color> " + dstMR.material.name);

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


    public void ReplayLevel()
    {
        Time.timeScale = 1f;  // ensure game is unpaused
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;

        // Option A: Using scene name
        SceneManager.LoadScene("Main_Menu");

        // Option B: Using build index (if your main menu is scene 0)
        // SceneManager.LoadScene(0);
    }
    public void GoToMiddleMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("middle_screen"); // Replace with your main menu scene name
    }
    public void PlayNextLevel()
    {
        Time.timeScale = 1f;

        string current = SceneManager.GetActiveScene().name;

        if (current == "focus1_phone_lvl1")
        {
            SceneManager.LoadScene("focus1_phone_lvl2");
        }
        else if (current == "focus1_phone_lvl2")
        {
            SceneManager.LoadScene("focus1_phone_lvl3");
        }
        else if (current == "focus1_phone_lvl3")
        {
            SceneManager.LoadScene("Main_Menu");
        }
        else
        {
            Debug.Log("Unknown scene, sending to Main Menu as fallback.");
            SceneManager.LoadScene("Main_Menu");
        }
    }

}
