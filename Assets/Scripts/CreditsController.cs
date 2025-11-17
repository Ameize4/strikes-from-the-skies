using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class CreditsController : MonoBehaviour
{
    [SerializeField] private CanvasGroup[] slides;
    [SerializeField] private float fadeDuration = 0.75f;
    [SerializeField] private float autoNextSlideTimer = 5f;

    private int currentIndex = -1;
    private bool isTransitioning = false;
    private bool isRunning;
    private float countdownNextSlide;

    private CanvasGroup container;

    private void Awake()
    {
        container = GetComponent<CanvasGroup>();
        foreach (var s in slides)
        {
            container.alpha = 0f;
            s.alpha = 0f;
            s.gameObject.SetActive(false);
        }
    }

    public void StartCredits()
    {
        isRunning = true;
        container.alpha = 1f;
        currentIndex = 0;
        countdownNextSlide = autoNextSlideTimer;
        ShowSlide(currentIndex);
    }

    private void Update()
    {
        if (isTransitioning)
            return;
        if (!isRunning)
            return;

        if (DetectAnyInput())
        {
            HandleAdvance();
        }
        countdownNextSlide -= Time.deltaTime;
        if (countdownNextSlide <= 0)
        {
            countdownNextSlide = autoNextSlideTimer;
            HandleAdvance();
        }
    }

    private bool DetectAnyInput()
    {
        if (Input.anyKeyDown) return true;
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) return true;
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) return true;
        if (Input.GetButtonDown("Submit") || Input.GetButtonDown("Fire1")) return true;
        return false;
    }

    private void HandleAdvance()
    {
        if (currentIndex >= slides.Length - 1 + 1) // explicitly show that we need one more iteration 
        {
            isRunning = false;
            EndCredits();
            return;
        }

        SwitchToSlide(currentIndex + 1);
    }

    private void ShowSlide(int index)
    {
        var slide = slides[index];
        slide.gameObject.SetActive(true);
        slide.alpha = 0f;

        isTransitioning = true;
        slide.DOFade(1f, fadeDuration)
            .OnComplete(() => isTransitioning = false);
    }

    private void SwitchToSlide(int nextIndex)
    {
        isTransitioning = true;

        var current = slides[currentIndex];
        Sequence seq = DOTween.Sequence();

        seq.Append(current.DOFade(0f, fadeDuration))
            .AppendCallback(() => current.gameObject.SetActive(false));
            
        if (nextIndex < slides.Length)
        {
            var next = slides[nextIndex];
            next.gameObject.SetActive(true);
            next.alpha = 0f;
            seq.Append(next.DOFade(1f, fadeDuration));
        }
        
        seq.OnComplete(() =>
        {
            currentIndex = nextIndex;
            isTransitioning = false;
        });
    }

    public void EndCredits()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
