using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class JuicyButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
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

    public void OnPointerClick(PointerEventData eventData)
    {
        sounds.Instance1.PlayClickSFX();
        // Do a quick punch/shake or bounce
        transform.DOPunchScale(Vector3.one * 0.15f, 0.2f, 8, 0.5f);
    }
}
