/*using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class uiSliderScript : MonoBehaviour


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

        public float duration = 0.5f;
        public int layerIndex = 0;
    }

    [Header("Panel Settings")]
    public List<UIElement> uiElements = new List<UIElement>();

    [Header("Global Settings")]
    public float defaultDuration = 0.5f;
    public float layerDelay = 0.5f;

    // ⭐ NEW
    [Header("Extra Animated Sprites")]
    public SpriteRenderer startSprite;     // Appears at game start
    public SpriteRenderer endSprite;       // Appears after timer completes

    private Vector3 startSpriteOriginalPos;
    private Vector3 endSpriteOriginalPos;

    private Dictionary<UIElement, Vector3> originalPos = new Dictionary<UIElement, Vector3>();
    private Dictionary<UIElement, Vector3> originalScale = new Dictionary<UIElement, Vector3>();


    IEnumerator Start()
    {
        yield return null;

        // Store UI original positions
        foreach (var el in uiElements)
        {
            if (el.panel != null)
            {
                originalPos[el] = el.panel.transform.position;
                originalScale[el] = el.panel.transform.localScale;
            }
        }

        // ⭐ Store extra sprite original positions
        if (startSprite != null)
            startSpriteOriginalPos = startSprite.transform.position;

        if (endSprite != null)
            endSpriteOriginalPos = endSprite.transform.position;

        // Move UI panels off-screen
        foreach (var el in uiElements)
        {
            MoveOffScreen(el);
        }

        // ⭐ Move start sprite off-screen (above)
        if (startSprite != null)
        {
            Vector3 off = startSpriteOriginalPos + new Vector3(0, -Screen.height, 0);
            startSprite.transform.position = off;
        }

        // ⭐ Move end sprite off-screen (below)
        if (endSprite != null)
        {
            Vector3 off = endSpriteOriginalPos + new Vector3(0, Screen.height, 0);
            endSprite.transform.position = off;
        }

        // ⭐ Animate start sprite first
        PlayStartSprite();

        // Animate UI entrance
        yield return StartCoroutine(PlayEntrance());
    }

    void MoveOffScreen(UIElement el)
    {
        Transform rt = el.panel.transform;
        Vector3 off = originalPos[el];

        float W = Screen.width;
        float H = Screen.height;

        switch (el.entryType)
        {
            case EntryType.SlideFromLeft:
                off = new Vector3(-W, originalPos[el].y, 0);
                break;

            case EntryType.SlideFromRight:
                off = new Vector3(W, originalPos[el].y, 0);
                break;

            case EntryType.SlideFromTop:
                off = new Vector3(originalPos[el].x, H, 0);
                break;

            case EntryType.SlideFromBottom:
                off = new Vector3(originalPos[el].x, -H, 0);
                break;

            case EntryType.CenterPop:
                rt.localScale = Vector3.zero;
                return;
        }

        rt.position = off;
    }

    IEnumerator PlayEntrance()
    {
        uiElements.Sort((a, b) => a.layerIndex.CompareTo(b.layerIndex));

        int currentLayer = -1;

        foreach (var el in uiElements)
        {
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
            Vector3 targetScale = originalScale[el];
            rt.localScale = Vector3.zero;
            rt.DOScale(targetScale, dur).SetEase(Ease.OutBack);
            rt.DOPunchScale(Vector3.one * 0.1f, 0.3f, 8, 0.7f);
            return;
        }

        rt.DOMove(originalPos[el], dur).SetEase(Ease.OutCubic);
    }

    // ⭐ CALLED AUTOMATICALLY AT START
    public void PlayStartSprite()
    {
        if (startSprite == null) return;

        startSprite.transform.DOMove(startSpriteOriginalPos,defaultDuration)
            .SetEase(Ease.OutBack);
    }

    // ⭐ CALL THIS FROM GAME MANAGER after timer ends
    public void PlayEndSprite()
    {
        if (endSprite == null) return;

        endSprite.transform.DOMove(endSpriteOriginalPos, defaultDuration)
            .SetEase(Ease.OutBack);
    }
}

*/


using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class uiSliderScript : MonoBehaviour
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

        public float duration = 0.5f;
        public int layerIndex = 0;
    }

    [Header("Panel Settings")]
    public List<UIElement> uiElements = new List<UIElement>();

    [Header("Global Settings")]
    public float defaultDuration = 0.5f;
    public float layerDelay = 0.5f;

    // ⭐ NEW – EXTRA POP ELEMENTS AT SCENE START
    [Header("Pop-Up On Start")]
    public List<Transform> popOnStartElements = new List<Transform>();
    public float popOnStartDuration = 0.5f;

    // ⭐ Extra Animated Sprites
    [Header("Extra Animated Sprites")]
    public SpriteRenderer startSprite;     // Appears at game start
    public SpriteRenderer endSprite;       // Appears after timer completes

    private Vector3 startSpriteOriginalPos;
    private Vector3 endSpriteOriginalPos;

    private Dictionary<UIElement, Vector3> originalPos = new Dictionary<UIElement, Vector3>();
    private Dictionary<UIElement, Vector3> originalScale = new Dictionary<UIElement, Vector3>();

    IEnumerator Start()
    {
        yield return null;

        // Store UI original positions
        foreach (var el in uiElements)
        {
            if (el.panel != null)
            {
                originalPos[el] = el.panel.transform.position;
                originalScale[el] = el.panel.transform.localScale;
            }
        }

        // Store extra sprite original positions
        if (startSprite != null)
            startSpriteOriginalPos = startSprite.transform.position;

        if (endSprite != null)
            endSpriteOriginalPos = endSprite.transform.position;

        // Move UI panels off-screen
        foreach (var el in uiElements)
        {
            if (el.panel != null)
            {
                MoveOffScreen(el);
            }
        }

        // Move start sprite off-screen (above)
        if (startSprite != null)
        {
            Vector3 off = startSpriteOriginalPos + new Vector3(0, -Screen.height, 0);
            startSprite.transform.position = off;
        }

        // Move end sprite off-screen (below)
        if (endSprite != null)
        {
            Vector3 off = endSpriteOriginalPos + new Vector3(0, Screen.height, 0);
            endSprite.transform.position = off;
        }

        // ⭐ POP-UP ELEMENTS AT SCENE START
        PlayPopOnStart();
        StartCoroutine(DisablePopOnStartAfterDelay());
        // Animate start sprite
        PlayStartSprite();

        // Animate UI entrance
        yield return StartCoroutine(PlayEntrance());
    }


    IEnumerator DisablePopOnStartAfterDelay()
    {
        yield return new WaitForSeconds(5f);

        foreach (var t in popOnStartElements)
        {
            if (t != null)
                t.gameObject.SetActive(false);
        }
    }


    void MoveOffScreen(UIElement el)
    {
        Transform rt = el.panel.transform;
        Vector3 off = originalPos[el];

        float W = Screen.width;
        float H = Screen.height;

        switch (el.entryType)
        {
            case EntryType.SlideFromLeft:
                off = new Vector3(-W, originalPos[el].y, 0);
                break;

            case EntryType.SlideFromRight:
                off = new Vector3(W, originalPos[el].y, 0);
                break;

            case EntryType.SlideFromTop:
                off = new Vector3(originalPos[el].x, H, 0);
                break;

            case EntryType.SlideFromBottom:
                off = new Vector3(originalPos[el].x, -H, 0);
                break;

            case EntryType.CenterPop:
                rt.localScale = Vector3.zero;
                return;
        }

        rt.position = off;
    }

    IEnumerator PlayEntrance()
    {
        uiElements.Sort((a, b) => a.layerIndex.CompareTo(b.layerIndex));

        int currentLayer = -1;

        foreach (var el in uiElements)
        {
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
            Vector3 targetScale = originalScale[el];
            rt.localScale = Vector3.zero;
            rt.DOScale(targetScale, dur).SetEase(Ease.OutBack);
            rt.DOPunchScale(Vector3.one * 0.1f, 0.3f, 8, 0.7f);
            return;
        }

        rt.DOMove(originalPos[el], dur).SetEase(Ease.OutCubic);
    }

    // ⭐ POP-UP ON START FOR CUSTOM ELEMENTS
    public void PlayPopOnStart()
    {
        foreach (var t in popOnStartElements)
        {
            if (t == null) continue;

            Vector3 original = t.localScale;
            t.localScale = Vector3.zero;

            Sequence seq = DOTween.Sequence();
            seq.Append(t.DOScale(original, popOnStartDuration).SetEase(Ease.OutBack));
            seq.Append(t.DOPunchScale(Vector3.one * 0.1f, 0.3f, 8, 0.7f));
        }
    }

    // CALLED AUTOMATICALLY AT START
    public void PlayStartSprite()
    {
        if (startSprite == null) return;

        startSprite.transform.DOMove(startSpriteOriginalPos, defaultDuration)
            .SetEase(Ease.OutBack);
    }

    // CALL THIS FROM GAME MANAGER after timer ends
    public void PlayEndSprite()
    {
        if (endSprite == null) return;

        endSprite.transform.DOMove(endSpriteOriginalPos, defaultDuration)
            .SetEase(Ease.OutBack);
    }
}
