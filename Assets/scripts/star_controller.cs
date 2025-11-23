/*using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class star_controller : MonoBehaviour

{
    [Header("Assign Star Fill Images (Filled Star Sprite)")]
    public Image[] starFills;
    // Each element = the FILLED star image child. Turn them OFF initially.

    [Header("Animation Settings")]
    public float popDuration = 0.3f;
    public float punchAmount = 0.4f;
    public float delayBetweenStars = 0.2f;

    void Start()
    {
        // Ensure all stars are hidden at start
        foreach (var s in starFills)
            if (s != null) s.gameObject.SetActive(false);
    }

    public void ShowStars(int correct, int total)
    {
        float ratio = (float)correct / Mathf.Max(1, total);

        int starsToShow = 0;

        // Basic scoring thresholds
        if (ratio >= 0.85f) starsToShow = 3;
        else if (ratio >= 0.55f) starsToShow = 2;
        else if (ratio >= 0.30f) starsToShow = 1;
        else starsToShow = 0;

        StartCoroutine(AnimateStars(starsToShow));
    }

    private System.Collections.IEnumerator AnimateStars(int count)
    {
        for (int i = 0; i < starFills.Length; i++)
        {
            starFills[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < count; i++)
        {
            Image star = starFills[i];
            star.gameObject.SetActive(true);

            // Reset scale
            star.transform.localScale = Vector3.one * 0.2f;

            // POP animation
            star.transform.DOScale(1f, popDuration).SetEase(Ease.OutBack);

            // Punch effect
            star.transform.DOPunchScale(
                Vector3.one * punchAmount,
                popDuration,
                10,
                1
            );

            yield return new WaitForSecondsRealtime(delayBetweenStars);
        }
    }
}
*/

using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class star_controller : MonoBehaviour
{
    public Image[] starFills;

    [Header("Animation Settings")]
    public float popDuration = 0.3f;
    public float punchAmount = 0.3f;
    public float delayBetweenStars = 0.15f;

    private Vector3[] originalScales;


    void Awake()
    {
        // Capture scale BEFORE Unity UI systems modify them
        originalScales = new Vector3[starFills.Length];

        for (int i = 0; i < starFills.Length; i++)
        {
            if (starFills[i] != null)
            {
                originalScales[i] = starFills[i].rectTransform.localScale;
                starFills[i].gameObject.SetActive(false);
            }
        }
    }

    void Start()
    {
        // NOW UI is fully built → safe to read localScale
        originalScales = new Vector3[starFills.Length];

        for (int i = 0; i < starFills.Length; i++)
        {
            if (starFills[i] != null)
            {
                RectTransform rt = starFills[i].rectTransform;
                originalScales[i] = rt.localScale;

                // hide initially
                starFills[i].gameObject.SetActive(false);
                Debug.Log("listttt" + originalScales[i]);
            }
        }
        
    }

    public void ShowStars(int correct, int total)
    {
        float ratio = (float)correct / Mathf.Max(1, total);
        int starsToShow = 0;

        if (ratio >= 0.65f) starsToShow = 3;
        else if (ratio >= 0.40f) starsToShow = 2;
        else if (ratio >= 0.10f) starsToShow = 1;

        StartCoroutine(AnimateStars(starsToShow));
    }

    private System.Collections.IEnumerator AnimateStars(int count)
    {
        // Turn off all stars first
        foreach (var s in starFills)
            if (s != null) s.gameObject.SetActive(false);

        for (int i = 0; i < count; i++)
        {
            var img = starFills[i];
            var rt = img.rectTransform;

            img.gameObject.SetActive(true);
            //Debug.Log("originalScales:"+ "[ "+i +" ]: " + originalScales[i]);

            // Shrink before popping
            //rt.localScale = originalScales[i] * 0.2f;
            //Debug.Log("rt.localscale" + "[ " + i + " ]: " + rt.localScale);


            // POP to original scale
            rt.DOScale(originalScales[i]*1.2f, popDuration).SetEase(Ease.OutBack);

            // Punch effect
            //rt.DOPunchScale(originalScales[i] * punchAmount, popDuration, 10, 1);
            /*if (i == 1)
            {
                rt.DOPunchScale(originalScales[i] * punchAmount, popDuration, 10, 1);
            }
            if (i == 2)
            {
                rt.DOPunchScale(originalScales[i] * punchAmount, popDuration, 10, 1);
            }
            if (i == 0)
            {
                rt.DOPunchScale(originalScales[i] * punchAmount, popDuration, 10, 1);
            }*/

            yield return new WaitForSecondsRealtime(delayBetweenStars);
        }
    }
}
