using UnityEngine;
using UnityEngine.UI;
using LitMotion;
using LitMotion.Extensions;
using System.Collections;
using System.Collections.Generic;

public class Carousel : MonoBehaviour
{
    [Tooltip("The content GameObject that holds the slides.")]
    [SerializeField] private GameObject content;

    [Tooltip("Array of RectTransforms representing the UI slides (Panels or Images).")]
    [SerializeField] private RectTransform[] slides;

    [Tooltip("Array of RectTransforms representing the left, target, and right positions.")]
    [SerializeField] private RectTransform[] activeIndexLocation; // [0] = Left, [1] = Target, [2] = Right

    [Tooltip("Duration of the slide transition in seconds.")]
    [SerializeField] private float slideDuration = 0.5f;

    [Tooltip("Time interval in seconds before the auto-slide starts.")]
    [SerializeField] private float interval = 5f;

    [Tooltip("Enable or disable auto-sliding.")]
    [SerializeField] private bool autoCycle = true;

    [Tooltip("Active Index of the slide.")]
    [SerializeField] private int activeIndex = 0; // Current active slide index

    private Coroutine autoSlideCoroutine;
    private RectTransform leftPosition;
    private RectTransform targetPosition;
    private RectTransform rightPosition;

    private void Start()
    {
        if (activeIndexLocation.Length < 3)
        {
            Debug.LogError("Carousel requires 3 positions (Left, Target, Right) in activeIndexLocation[]!");
            return;
        }

        // Assign position references
        leftPosition = activeIndexLocation[0];
        targetPosition = activeIndexLocation[1];
        rightPosition = activeIndexLocation[2];

        InitializeSlides();

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

    private IEnumerator AutoSlide()
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);
            NextSlide();
        }
    }

    public void NextSlide()
    {
        if (slides.Length == 0) return;

        int nextIndex = (activeIndex + 1) % slides.Length;
        AnimateSlideTransition(nextIndex, 1);
    }

    public void PrevSlide()
    {
        if (slides.Length == 0) return;

        int prevIndex = (activeIndex - 1 + slides.Length) % slides.Length;
        AnimateSlideTransition(prevIndex, -1);
    }

    private void AnimateSlideTransition(int targetIndex, int direction)
    {
        if (slides.Length == 0 || targetIndex == activeIndex) return;

        // Move the current active slide to the left or right
        RectTransform exitingSlide = slides[activeIndex];
        Vector2 exitPosition = (direction == 1) ? leftPosition.anchoredPosition : rightPosition.anchoredPosition;

        LMotion.Create(exitingSlide.anchoredPosition, exitPosition, slideDuration)
            .WithEase(Ease.OutQuad)
            .BindToAnchoredPosition(exitingSlide);

        // Move the new active slide from the opposite side to the target position
        RectTransform incomingSlide = slides[targetIndex];
        Vector2 enterPosition = (direction == 1) ? rightPosition.anchoredPosition : leftPosition.anchoredPosition;
        incomingSlide.anchoredPosition = enterPosition;
        incomingSlide.gameObject.SetActive(true);

        LMotion.Create(incomingSlide.anchoredPosition, targetPosition.anchoredPosition, slideDuration)
            .WithEase(Ease.OutQuad)
            .WithOnComplete(() =>
            {
                // Hide the previous active slide after animation
                exitingSlide.gameObject.SetActive(false);
                activeIndex = targetIndex;
            })
            .BindToAnchoredPosition(incomingSlide);
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

        for (int i = 0; i < slides.Length; i++)
        {
            if (i == activeIndex)
            {
                slides[i].anchoredPosition = targetPosition.anchoredPosition;
            }
            else if (i < activeIndex)
            {
                slides[i].anchoredPosition = leftPosition.anchoredPosition;
            }
            else
            {
                slides[i].anchoredPosition = rightPosition.anchoredPosition;
            }

            slides[i].gameObject.SetActive(i == activeIndex);
        }
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
}
