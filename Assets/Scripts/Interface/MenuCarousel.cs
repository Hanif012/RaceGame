using UnityEngine;
using LitMotion;
using LitMotion.Extensions;

public class MenuCarousel : MonoBehaviour
{
    [Header("Carousel Settings")]
    [Tooltip("The content GameObject that holds the slides.")]
    [SerializeField] private GameObject content;
    [Tooltip("Array of RectTransforms representing the UI slides.")]
    [SerializeField] private RectTransform[] slides;
    [Tooltip("Slide transition duration in seconds.")]
    [SerializeField] private float slideDuration = 0.5f;
    [Tooltip("Enable or disable auto infinite sliding.")]

    [SerializeField] private int activeIndex = 0;
    
    [Header("Slide Positions")]
    [SerializeField] private RectTransform MostLeftPosition;
    [SerializeField] private RectTransform LeftPosition;
    [SerializeField] private RectTransform TargetPosition;
    [SerializeField] private RectTransform RightPosition;
    [SerializeField] private RectTransform MostRightPosition;
    [Header("Slide Settings")]
    [SerializeField] private float firstOpacity = 1f;
    [SerializeField] private float SecondOpacity = 0.7f;
    [SerializeField] private float ThirdOpacity = 0.3f;
    private const int MAX_VISIBLE_SLIDES = 5;  // Always an odd number
    private bool isAnimating = false;

    private void Start()
    {
        InitializeSlides();
        PositionSlidesInstantly();

        if (slides.Length == 0)
        {
            Debug.LogWarning("Carousel has no slides! Please check your content setup.");
            return;
        }
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

    private void PositionSlidesInstantly()
    {
        if (slides.Length == 0) return;

        for (int i = 0; i < slides.Length; i++)
        {
            RectTransform slide = slides[i];
            if(i < 3)
            {
                if (i == 0) SetSlide(slide, TargetPosition, TargetPosition.localScale, firstOpacity, true);
                else if (i == 1) SetSlide(slide, RightPosition, RightPosition.localScale, SecondOpacity, true);
                else if (i == 2) SetSlide(slide, MostRightPosition, MostRightPosition.localScale, ThirdOpacity, true);
            }
            else
            {
                slide.gameObject.SetActive(false);
            }
        }

    }

    private void SetSlide(RectTransform slide, RectTransform position, Vector3 scale, float alpha, bool isActive)
    {
        slide.gameObject.SetActive(isActive);
        slide.anchoredPosition = position.anchoredPosition;
        slide.localScale = scale;
        
        CanvasGroup canvasGroup = slide.GetComponent<CanvasGroup>() ?? slide.gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = alpha;
    }

    public void MoveNext()
    {
        if (activeIndex < slides.Length - 1 && !isAnimating)
        {
            isAnimating = true;
            activeIndex++;
            MoveToSlide(activeIndex);
        }
    }

    public void MovePrev()
    {
        if (activeIndex > 0 && !isAnimating)
        {
            isAnimating = true;
            activeIndex--;
            MoveToSlide(activeIndex);
        }
    }

    private void MoveToSlide(int index)
    {
        if (index < 0 || index >= slides.Length) return;

        for (int i = 0; i < slides.Length; i++)
        {
            RectTransform slide = slides[i];
            int start = index - 2;
            int end = index + 2;

            if (i < start || i > end)
            {
                MoveSlide(slide, MostLeftPosition, MostLeftPosition.localScale, 0, false);
            }
            else if (i == start)
            {
                MoveSlide(slide, MostLeftPosition, MostLeftPosition.localScale, ThirdOpacity, true);
            }
            else if (i == start + 1)
            {
                MoveSlide(slide, LeftPosition, LeftPosition.localScale, SecondOpacity, true);
            }
            else if (i == index)
            {
                MoveSlide(slide, TargetPosition, TargetPosition.localScale, firstOpacity, true);
            }
            else if (i == end - 1)
            {
                MoveSlide(slide, RightPosition, RightPosition.localScale, SecondOpacity, true);
            }
            else if (i == end)
            {
                MoveSlide(slide, MostRightPosition, MostRightPosition.localScale, ThirdOpacity, true);
            }
        }
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
            .WithOnComplete(() => isAnimating = false)
            .BindToAlpha(canvasGroup);
    }

    private void SlideTransition(int targetIndex)
    {
        if(slides.Length == 0 || targetIndex == activeIndex) return;

        
    }

}

/*

class ButtonViewport:
    def __init__(self, buttons, viewport_size=5):
        self.buttons = buttons
        self.viewport_size = viewport_size
        self.n = len(buttons)
        self.mid = viewport_size // 2
        self.index = 0  # Start at the first index

    def get_viewport(self):
        start = self.index - self.mid
        viewport = []
        
        for j in range(self.viewport_size):
            idx = start + j
            if 0 <= idx < self.n:
                viewport.append(self.buttons[idx])
            else:
                viewport.append(None)
        
        return viewport

    def move_next(self):
        if self.index < self.n - 1:
            self.index += 1
        return self.get_viewport()
    
    def move_prev(self):
        if self.index > 0:
            self.index -= 1
        return self.get_viewport()

# Example usage:
buttons = [1, 2, 3, 4, 5, 6, 7, 8, 9]
viewport_manager = ButtonViewport(buttons)

# Initial viewport
print("Initial:", viewport_manager.get_viewport())

# Move next and prev
print("Next:", viewport_manager.move_next())
print("Next:", viewport_manager.move_next())
print("Prev:", viewport_manager.move_prev())


*/