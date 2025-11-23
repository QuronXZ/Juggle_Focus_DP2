using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UI_slider_script : MonoBehaviour

{
    public enum EntryType
    {
        SlideFromLeft,
        SlideFromRight,
        SlideFromTop,
        SlideFromBottom,
        CenterPop
    }

    [System.Serializable]
    public class UIElement
    {
        public GameObject panel;
        public EntryType entryType = EntryType.SlideFromLeft;

        public float duration = 0.5f;         // If 0 → default duration
        public int layerIndex = 0;            // 0 = Base, 1 = Mid, 2 = Top
    }

    [Header("Panel Settings")]
    public List<UIElement> uiElements = new List<UIElement>();

    [Header("Global Settings")]
    public float defaultDuration = 0.5f;
    public float layerDelay = 0.5f;          // Time between layer groups

    private Dictionary<UIElement, Vector3> originalPos = new Dictionary<UIElement, Vector3>();
    private Dictionary<UIElement, Vector3> originalScale = new Dictionary<UIElement, Vector3>();


    IEnumerator Start()
    {
        yield return null; // Give UI 1 frame to initialize

        // Store original positions
        foreach (var el in uiElements)
        {
            if (el.panel != null)
                originalPos[el] = el.panel.transform.position;
                originalScale[el] = el.panel.transform.localScale;
        }

        // Move all panels off-screen instantly
        foreach (var el in uiElements)
        {
            MoveOffScreen(el);
        }

        // Animate in by layer
        yield return StartCoroutine(PlayEntrance());
    }

    void MoveOffScreen(UIElement el)
    {
        Transform rt = el.panel.transform;
        Vector2 off = Vector2.zero;

        float screenW = Screen.width;
        float screenH = Screen.height;

        switch (el.entryType)
        {
            case EntryType.SlideFromLeft:
                off = new Vector2(-screenW, originalPos[el].y);
                break;

            case EntryType.SlideFromRight:
                off = new Vector2(screenW, originalPos[el].y);
                break;

            case EntryType.SlideFromTop:
                off = new Vector2(originalPos[el].x, screenH);
                break;

            case EntryType.SlideFromBottom:
                off = new Vector2(originalPos[el].x, -screenH);
                break;

            case EntryType.CenterPop:
                // Start nearly invisible
                rt.localScale = Vector3.zero;
                break;
        }

        if (el.entryType != EntryType.CenterPop)
            rt.transform.position = off;
    }

    IEnumerator PlayEntrance()
    {
        // Sort by layers: 0 → 1 → 2
        uiElements.Sort((a, b) => a.layerIndex.CompareTo(b.layerIndex));

        int currentLayer = -1;

        foreach (var el in uiElements)
        {
            // If this element is a new layer, delay
            if (el.layerIndex != currentLayer)
            {
                currentLayer = el.layerIndex;
                yield return new WaitForSeconds(layerDelay);
            }

            PlaySingleAnimation(el);
        }
    }

    void PlaySingleAnimation(UIElement el)
    {
        float dur = el.duration > 0 ? el.duration : defaultDuration;

        Transform rt = el.panel.transform;

        if (el.entryType == EntryType.CenterPop)
        {
            // Pop-in punch
            // Restore to original scale instead of 1
            Vector3 targetScale = originalScale[el];
            rt.localScale = Vector3.zero;
            rt.DOScale(targetScale, dur).SetEase(Ease.OutBack);
            rt.DOPunchScale(Vector3.one * 0.1f, 0.3f, 8, 0.7f);
            return;
        }

        // Slide animation
        rt.DOLocalMove(originalPos[el], dur).SetEase(Ease.OutCubic);
    }
}
