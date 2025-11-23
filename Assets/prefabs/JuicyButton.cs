using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class JuicyButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    Vector3 originalScale;
    private Vector3 startPos;
    private Image childImage;  // Actual visual image on button
    private Sprite originalSprite;
    private bool isPressed = false;

    [Header("Pressed Sprite (Optional)")]
    public Sprite pressedSprite;  // Image to show when pressed
    public float pressDuration = 1.0f;

    [Header("Floating Animation Settings")]
    public float floatDistance = 10f;
    public float floatDuration = 1.5f;

    void Start()
    {
        originalScale = transform.localScale;
        startPos = transform.localPosition;
        childImage = GetComponentInChildren<Image>();

        if (childImage != null)
            originalSprite = childImage.sprite;

        StartFloating();
    }


    // -------------------------------------------
    // FLOATING ANIMATION
    // -------------------------------------------
    void StartFloating()
    {
        // Floating: move up and down forever
        transform.DOLocalMoveY(startPos.y + floatDistance, floatDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }


    // -------------------------------------------
    // ON CLICK
    // -------------------------------------------
    public void OnPointerClick(PointerEventData eventData)
    {
        if (isPressed) return;

        sounds.Instance1.PlayClickSFX();

        // Punch scale
        transform.DOPunchScale(Vector3.one * 0.15f, 0.25f, 8, 0.6f);
        //transform.DOPunchScale(Vector3.one * 0.15f, 0.2f, 8, 0.5f);


        // Change sprite
        if (childImage != null)
        {
            childImage.sprite = pressedSprite != null ? pressedSprite : originalSprite;
        }

        isPressed = true;

        // Reset after 3 seconds
        Invoke(nameof(ResetButtonVisual), pressDuration);
    }

    // -------------------------------------------
    // RESET
    // -------------------------------------------
    private void ResetButtonVisual()
    {
        if (childImage != null)
            childImage.sprite = originalSprite;

        isPressed = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Scale up smoothly
        sounds.Instance1.PlayNavigationSFX();
        transform.DOScale(originalScale * 1.1f, 0.2f).SetEase(Ease.OutBack);
        //Button.m_Transition.m_Colors.m_HighlightedColor= new Color(1.0f, 0.917f, 0.608f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Scale back to original
        transform.DOScale(originalScale, 0.2f).SetEase(Ease.OutBack);
    }

/*    public void OnPointerClick(PointerEventData eventData)
    {
        sounds.Instance1.PlayClickSFX();
        // Do a quick punch/shake or bounce
        transform.DOPunchScale(Vector3.one * 0.15f, 0.2f, 8, 0.5f);
    }*/
}
