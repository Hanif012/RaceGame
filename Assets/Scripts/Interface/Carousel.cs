using UnityEngine;
using UnityEngine.UI;
using LitMotion;
using LitMotion.Extensions;
using System.Collections;

public class LitMotionCarousel : MonoBehaviour
{
    [SerializeField] private RectTransform[] slides; // UI Slides (Panels or Images)
    [SerializeField] private float slideDuration = 0.5f; // Time for slide transition
    [SerializeField] private float interval = 5f; // Time before auto-slide starts
    [SerializeField] private bool autoCycle = true; // Enable auto-sliding

    private int activeIndex = 0; // Current active slide index
    private Coroutine autoSlideCoroutine;
    private Vector2 originalPosition;

    private void Start()
    {
        originalPosition = slides[0].anchoredPosition;

        if (autoCycle)
        {
            autoSlideCoroutine = StartCoroutine(AutoSlide());
        }

        InitializeSlides();
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
        int nextIndex = (activeIndex + 1) % slides.Length;
        AnimateSlideTransition(nextIndex, 1);
    }

    public void PrevSlide()
    {
        int prevIndex = (activeIndex - 1 + slides.Length) % slides.Length;
        AnimateSlideTransition(prevIndex, -1);
    }

    private void AnimateSlideTransition(int targetIndex, int direction)
    {
        if (targetIndex == activeIndex) return;

        // Move current slide out
        LMotion.Create(slides[activeIndex].anchoredPosition, slides[activeIndex].anchoredPosition + new Vector2(-direction * slides[activeIndex].rect.width, 0), slideDuration)
            .WithEase(Ease.OutQuad)
            .BindToAnchoredPosition(slides[activeIndex]);

        // Move next slide in
        slides[targetIndex].anchoredPosition = originalPosition + new Vector2(direction * slides[targetIndex].rect.width, 0);
        slides[targetIndex].gameObject.SetActive(true);
        
        LMotion.Create(slides[targetIndex].anchoredPosition, originalPosition, slideDuration)
            .WithEase(Ease.OutQuad)
            .WithOnComplete(() =>
            {
                slides[activeIndex].gameObject.SetActive(false);
                activeIndex = targetIndex;
            })
            .BindToAnchoredPosition(slides[targetIndex]);
    }

    private void InitializeSlides()
    {
        for (int i = 0; i < slides.Length; i++)
        {
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
