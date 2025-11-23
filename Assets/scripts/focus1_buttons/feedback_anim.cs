using UnityEngine;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class feedback_anim : MonoBehaviour


{
    public Image baseImg;
    public TextMeshProUGUI correct_txt, wrong_txt;
    public Image feedbackImage;
    

    [Header("Animations")]
    public Sprite[] correctFrames;
    public Sprite[] wrongFrames;

    [Header("Settings")]
    public float frameRate = 0.05f; // 20fps
    public float dura = 0.3f;
    private Coroutine playRoutine;
    private bool showingCorrect = false;
    private bool showingWrong = false;
    private Vector2 baseOriginalPos;


    void Awake()
    {
        if (feedbackImage == null)
            feedbackImage = GetComponent<Image>();
        baseImg.enabled = false;
        feedbackImage.enabled = false;
        correct_txt.enabled = false;
        wrong_txt.enabled = false;

        baseOriginalPos = baseImg.rectTransform.anchoredPosition;

    }

    public void PlayCorrect()
    {
        showingCorrect = true;
        showingWrong = false;
        //correct_txt.enabled = true;
        PlayAnimation(correctFrames);
       // correct_txt.enabled = false;
    }

    public void PlayWrong()
    {
        showingCorrect = false;
        showingWrong = true;
        //wrong_txt.enabled = true;
        PlayAnimation(wrongFrames);
        //wrong_txt.enabled = false;
    }

    void PlayAnimation(Sprite[] frames)
    {
        if (playRoutine != null)
            StopCoroutine(playRoutine);


        // Move base image offscreen (bottom)
        baseImg.rectTransform.anchoredPosition =
            baseOriginalPos + new Vector2(0, -Screen.height);

        baseImg.enabled = true;

        // Slide into original position
        Tween slideTween = baseImg.rectTransform.DOAnchorPos(baseOriginalPos, 0.35f)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);

        //playRoutine = StartCoroutine(PlayFramesLoop(frames));
        // Start coroutine only AFTER slide finishes
        slideTween.OnComplete(() =>
        {
            playRoutine = StartCoroutine(PlayFramesLoop(frames));
        });
    }

    /*    IEnumerator PlayFrames(Sprite[] frames)
        {
            baseImg.enabled = true;
            feedbackImage.enabled = true;

            for (int i = 0; i < frames.Length; i++)
            {
                feedbackImage.sprite = frames[i];
                yield return new WaitForSeconds(frameRate);
            }

            yield return new WaitForSeconds(dura);

            baseImg.enabled = false;
            feedbackImage.enabled = false;
        }*/
    IEnumerator PlayFramesLoop(Sprite[] frames)
    {
        // --- PAUSE GAMEPLAY ---
        Time.timeScale = 0;

        baseImg.enabled = true;
        feedbackImage.enabled = true;

        // enable correct/wrong text based on mode
        correct_txt.enabled = showingCorrect;
        wrong_txt.enabled = showingWrong;

        // --- PUNCH SCALE ON APPEAR ---
        feedbackImage.rectTransform.DOPunchScale(
            new Vector3(0.2f, 0.2f, 0f),
            0.35f,
            10,
            0.5f
        );
        if (showingCorrect)
            correct_txt.rectTransform.DOPunchScale(new Vector3(0.3f, 0.3f, 0), 0.6f).SetUpdate(true);
        else if (showingWrong)
            wrong_txt.rectTransform.DOPunchScale(new Vector3(0.3f, 0.3f, 0), 0.6f).SetUpdate(true);

        float timer = 0f;
        int index = 0;

        while (timer < dura)
        {
            feedbackImage.sprite = frames[index];
            index = (index + 1) % frames.Length;   // loop through frames

            timer += frameRate;

            // Wait while ignoring Time.timeScale (important!)
            yield return new WaitForSecondsRealtime(frameRate);
        }

        // hold last frame a tiny bit
        yield return new WaitForSecondsRealtime(0.15f);

        baseImg.enabled = false;
        feedbackImage.enabled = false;
        correct_txt.enabled = false;
        wrong_txt.enabled = false;


        // --- RESUME GAMEPLAY ---
        //yield return new WaitForSecondsRealtime(gamePauseTime);
        Time.timeScale = 1f;
    }

}
