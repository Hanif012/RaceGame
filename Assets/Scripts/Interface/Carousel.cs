using UnityEngine;
using UnityEngine.UI;
using LitMotion;
using LitMotion.Extensions;
using System.Collections;
using System.Collections.Generic;

public class Carousel : MonoBehaviour
{
    [Header("Carousel Settings")]
    [Tooltip("The content GameObject that holds the slides.")]
    [SerializeField] private GameObject content;

    [Tooltip("Array of RectTransforms representing the UI slides.")]
    [SerializeField] private RectTransform[] slides;

    [Tooltip("Duration of the slide transition in seconds.")]
    [SerializeField] private float slideDuration = 0.5f;

    [Tooltip("Enable or disable auto-sliding.")]
    [SerializeField] private bool autoCycle = true;

    [Tooltip("Time interval before auto-slide starts.")]
    [SerializeField] private float interval = 5f;

    [Tooltip("Enable or disable infinite looping.")]
    [SerializeField] private bool infiniteLoop = true;

    [Tooltip("Enable or disable showing background transition")]
    [SerializeField] private bool showBackgroundTransition = true;

    [Tooltip("Current active slide index.")]
    [SerializeField] private int activeIndex = 0;
    private Coroutine autoSlideCoroutine;

    [Header("Slide Positions")]
    [SerializeField] private RectTransform MostLeftPosition;
    [SerializeField] private RectTransform LeftPosition;
    [SerializeField] private RectTransform TargetPosition;
    [SerializeField] private RectTransform RightPosition;
    [SerializeField] private RectTransform MostRightPosition;

    private const int MAX_VISIBLE_SLIDES = 5;  // Always an odd number
    private bool isAnimating = false;

    private void Start()
    {
        InitializeSlides();

        // Ensure the middle slide is properly positioned at the start
        if (slides.Length >= MAX_VISIBLE_SLIDES)
        {
            activeIndex = slides.Length / 2;
        }
        else
        {
            activeIndex = 0;
        }

        PositionSlidesInstantly();

        if (slides.Length == 0)
        {
            Debug.LogWarning("Carousel has no slides! Please check your content setup.");
            return;
        }

        if (autoCycle)
        {
            autoSlideCoroutine = StartCoroutine(AutoSlide());
        }
    }

    private void PositionSlidesInstantly()
    {
        if (slides.Length == 0) return;

        // Check if the middle is the first slide, then only show 3 slides instead of 5
        int visibleSlides = (activeIndex == 0) ? 3 : MAX_VISIBLE_SLIDES;
        int middleIndex = visibleSlides / 2;
        
        // Start index needs to be dynamically adjusted
        int startIndex = Mathf.Max(0, activeIndex - middleIndex);
        int endIndex = Mathf.Min(slides.Length, startIndex + visibleSlides);

        for (int i = 0; i < slides.Length; i++)
        {
            RectTransform slide = slides[i];
            bool shouldBeActive = (i >= startIndex && i < endIndex);

            if (!shouldBeActive)
            {
                slide.gameObject.SetActive(false);
                continue;
            }

            int relativePosition = i - startIndex;

            if (visibleSlides == 3)
            {
                // If only 3 slides are visible (when middle is 1)
                if (relativePosition == 0) SetSlide(slide, LeftPosition, LeftPosition.localScale, 0.7f, shouldBeActive);
                else if (relativePosition == 1) SetSlide(slide, TargetPosition, TargetPosition.localScale, 1.0f, shouldBeActive);
                else if (relativePosition == 2) SetSlide(slide, RightPosition, RightPosition.localScale, 0.7f, shouldBeActive);
            }
            else
            {
                // If 5 slides are visible
                if (relativePosition == 0) SetSlide(slide, MostLeftPosition, MostLeftPosition.localScale, 0.3f, shouldBeActive);
                else if (relativePosition == 1) SetSlide(slide, LeftPosition, LeftPosition.localScale, 0.7f, shouldBeActive);
                else if (relativePosition == middleIndex) SetSlide(slide, TargetPosition, TargetPosition.localScale, 1.0f, shouldBeActive);
                else if (relativePosition == middleIndex + 1) SetSlide(slide, RightPosition, RightPosition.localScale, 0.7f, shouldBeActive);
                else if (relativePosition == MAX_VISIBLE_SLIDES - 1) SetSlide(slide, MostRightPosition, MostRightPosition.localScale, 0.3f, shouldBeActive);
            }
        }
    }


    private IEnumerator AutoSlide()
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);
            NextSlide();
        }
    }

    private void AnimateSlideTransition(int targetIndex)
    {
        if (slides.Length == 0 || targetIndex == activeIndex) return;

        isAnimating = true;

        int middleIndex = MAX_VISIBLE_SLIDES / 2;
        int startIndex = Mathf.Max(0, targetIndex - middleIndex);
        int endIndex = Mathf.Min(slides.Length, startIndex + MAX_VISIBLE_SLIDES);

        for (int i = 0; i < slides.Length; i++)
        {
            RectTransform slide = slides[i];
            bool shouldBeActive = (i >= startIndex && i < endIndex);

            if (!shouldBeActive)
            {
                slide.gameObject.SetActive(false);
                continue;
            }

            int relativePosition = i - startIndex;

            if (relativePosition == 0) MoveSlide(slide, MostLeftPosition, MostLeftPosition.localScale, 0.3f, shouldBeActive);
            else if (relativePosition == 1) MoveSlide(slide, LeftPosition, LeftPosition.localScale, 0.7f, shouldBeActive);
            else if (relativePosition == middleIndex) MoveSlide(slide, TargetPosition, TargetPosition.localScale, 1.0f, shouldBeActive);
            else if (relativePosition == middleIndex + 1) MoveSlide(slide, RightPosition, RightPosition.localScale, 0.7f, shouldBeActive);
            else if (relativePosition == MAX_VISIBLE_SLIDES - 1) MoveSlide(slide, MostRightPosition, MostRightPosition.localScale, 0.3f, shouldBeActive);
        }

        activeIndex = targetIndex;
        isAnimating = false;
    }

    private void MoveSlide(RectTransform slide, RectTransform targetPosition, Vector3 scale, float alpha, bool isActive)
    {
        slide.gameObject.SetActive(isActive);

        LMotion.Create(slide.anchoredPosition, targetPosition.anchoredPosition, slideDuration)
            .WithEase(Ease.OutQuad)
            .BindToAnchoredPosition(slide);

        LMotion.Create(slide.localScale, scale, slideDuration)
            .WithEase(Ease.OutQuad)
            .BindToLocalScale(slide);

        CanvasGroup canvasGroup = slide.GetComponent<CanvasGroup>() ?? slide.gameObject.AddComponent<CanvasGroup>();
        LMotion.Create(canvasGroup.alpha, alpha, slideDuration)
            .WithEase(Ease.OutQuad)
            .BindToAlpha(canvasGroup);
    }

    private void InitializeSlides()
    {
        if (slides.Length == 0 && content != null)
        {
            int childCount = content.transform.childCount;
            if (childCount == 0)
            {
                Debug.LogWarning("Carousel content has no child slides.");
                return;
            }

            slides = new RectTransform[childCount];

            for (int i = 0; i < childCount; i++)
            {
                slides[i] = content.transform.GetChild(i).GetComponent<RectTransform>();
            }
        }

        if (slides.Length == 0)
        {
            Debug.LogWarning("No slides were found in the content object.");
            return;
        }
    }

    private void SetSlide(RectTransform slide, RectTransform position, Vector3 scale, float alpha, bool isActive)
    {
        slide.anchoredPosition = position.anchoredPosition;
        slide.localScale = scale;
        slide.gameObject.SetActive(isActive);

        CanvasGroup canvasGroup = slide.GetComponent<CanvasGroup>() ?? slide.gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = alpha;
    }

    public void PauseCarousel()
    {
        if (autoSlideCoroutine != null)
        {
            StopCoroutine(autoSlideCoroutine);
        }
    }

    public void ResumeCarousel()
    {
        if (autoCycle)
        {
            autoSlideCoroutine = StartCoroutine(AutoSlide());
        }
    }
    public void NextSlide()
    {
        if (slides.Length == 0 || isAnimating) return;

        int nextIndex = Mathf.Min(activeIndex + 1, slides.Length - 1);
        AnimateSlideTransition(nextIndex);
        Debug.Log("NextSlide: \n" + nextIndex + " " + activeIndex);
    }

    public void PrevSlide()
    {
        if (slides.Length == 0 || isAnimating) return;

        int prevIndex = Mathf.Max(activeIndex - 1, 0);
        AnimateSlideTransition(prevIndex);
        Debug.Log("PrevSlide: " + prevIndex + " " + activeIndex);
    }
}
