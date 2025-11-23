using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;

public class results_anim : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject bgObject;
    public GameObject extraObject;           // NEW – appears + scales in
    public Image animatedImage;
    public Image animatedBgImage;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI scoreText;

    [Header("Frame Animation")]
    public Sprite[] loopFrames;
    public float frameRate = 0.06f;

    [Header("Punch Settings")]
    public Vector3 punchStrength = new Vector3(0.25f, 0.25f, 0f);
    public float punchDuration = 0.35f;
    public int vibrato = 10;
    public float elasticity = 0.5f;

    private Coroutine loopRoutine;

    // --- ORIGINAL SCALES ---
    private Vector3 titleOriginalScale;
    private Vector3 scoreOriginalScale;
    private Vector3 imageOriginalScale;
    private Vector3 extraOriginalScale;

    void Awake()
    {
        if (titleText) titleOriginalScale = titleText.rectTransform.localScale;
        if (scoreText) scoreOriginalScale = scoreText.rectTransform.localScale;
        if (animatedImage) imageOriginalScale = animatedImage.rectTransform.localScale;
        if (animatedBgImage) imageOriginalScale = animatedBgImage.rectTransform.localScale;
        if (extraObject) extraOriginalScale = extraObject.transform.localScale;
    }
    private void Start()
    {
        bgObject.SetActive(false);
        extraObject.SetActive(false);
        animatedBgImage.enabled = false;
    }
    void OnEnable()
    {
        if (bgObject) bgObject.SetActive(true);
        animatedBgImage.enabled = true;

        if (extraObject)
        {
            extraObject.SetActive(true);
            extraObject.transform.localScale = Vector3.zero;

            extraObject.transform
                .DOScale(extraOriginalScale, 0.35f)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);
        }

        RunPunchAnimations();
        StartLoopAnimation();
    }

    void OnDisable()
    {
        if (loopRoutine != null)
            StopCoroutine(loopRoutine);
    }

    void RunPunchAnimations()
    {
        if (titleText)
        {
            titleText.rectTransform.localScale = titleOriginalScale;
            titleText.rectTransform
                .DOPunchScale(punchStrength, punchDuration, vibrato, elasticity)
                .SetUpdate(true);
        }

        if (scoreText)
        {
            scoreText.rectTransform.localScale = scoreOriginalScale;
            scoreText.rectTransform
                .DOPunchScale(punchStrength, punchDuration, vibrato, elasticity)
                .SetUpdate(true);
        }

        if (animatedImage)
        {
            animatedImage.rectTransform.localScale = imageOriginalScale;
            animatedImage.rectTransform
                .DOPunchScale(punchStrength, punchDuration, vibrato, elasticity)
                .SetUpdate(true);
        }
        if (animatedBgImage)
        {
            animatedBgImage.rectTransform.localScale = imageOriginalScale;
            animatedBgImage.rectTransform
                .DOPunchScale(punchStrength, punchDuration, vibrato, elasticity)
                .SetUpdate(true);
        }
    }

    void StartLoopAnimation()
    {
        if (loopRoutine != null)
            StopCoroutine(loopRoutine);

        loopRoutine = StartCoroutine(LoopFrames());
    }

    IEnumerator LoopFrames()
    {
        if (loopFrames.Length == 0 || animatedImage == null)
            yield break;

        int index = 0;

        while (true)
        {
            animatedImage.sprite = loopFrames[index];
            index = (index + 1) % loopFrames.Length;

            yield return new WaitForSecondsRealtime(frameRate);
        }
    }
}
