using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using LitMotion;
using LitMotion.Extensions;

public class Racing : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject TransitionCanvas;
    [SerializeField] private TextMeshProUGUI CountdownText;
    [SerializeField] private float offScreenBottom = 500f;
    [SerializeField] private float onScreen = 0f;
    [SerializeField] private float offScreenTop = -500f;
    [SerializeField] private float transitionDuration = 0.5f;

    [Header("Game Racing")]
    public GameObject PlayerCar;
    public CameraManager cameraManager;

    private bool isRaceStarted = false;
    private bool isFirstCarCinematicPlayed = false;
    private bool isPlayerCarFinished = false;

    void Start()
    {
        if (cameraManager == null)
        {
            Debug.LogError("CameraManager is not assigned in the Inspector!");
            return;
        }
    }

    void Update()
    {
        if (!isRaceStarted) return;

        // Handle the first car cinematic
        if (!isFirstCarCinematicPlayed && RankingManager.Instance.FirstCar != null)
        {
            isFirstCarCinematicPlayed = true;
            StartCoroutine(PlayFirstCarCinematic());
        }

        // Check for player car finish
        if (!isPlayerCarFinished && PlayerCar.GetComponent<CarProgress>().isFinished)
        {
            isPlayerCarFinished = true;
            Debug.Log("Player car has finished the race.");
            StartCoroutine(cameraManager.PanToPlayerCar(3f));
        }
    }

    private IEnumerator PlayFirstCarCinematic()
    {
        var firstCar = RankingManager.Instance.FirstCar;
        if (firstCar == null)
        {
            Debug.LogError("No first car assigned in RankingManager!");
            yield break;
        }

        Debug.Log($"{firstCar.name} is the first car to finish!");
        yield return StartCoroutine(cameraManager.PanToFirstCar(3.5f));
    }

    private IEnumerator StartCountdown()
    {
        StartCoroutine(cameraManager.SwitchToPOV());
        CountdownText.gameObject.SetActive(true);

        // Countdown
        for (int i = 3; i > 0; i--)
        {
            CountdownText.text = i.ToString();
            Debug.Log($"Countdown: {i}");
            yield return new WaitForSeconds(1f);
        }

        CountdownText.text = "GO!";
        Debug.Log("Race Started!");
        yield return new WaitForSeconds(1f);
        CountdownText.gameObject.SetActive(false);

        isRaceStarted = true;
        EnableCarMovement();
    }

    private void EnableCarMovement()
    {
        // Enable player car movement
        PlayerCar.GetComponent<Car>().StartRace();

        // Enable all opponent cars from RankingManager
        foreach (var car in RankingManager.Instance.cars)
        {
            if (car.gameObject != PlayerCar)
            {
                car.GetComponent<Car>().StartRace();
            }
        }
    }

    void OnEnable()
    {
        GameManager.OnGameStateChanged += UpdateGameStateUI;
    }

    void OnDisable()
    {
        GameManager.OnGameStateChanged -= UpdateGameStateUI;
    }

    private void UpdateGameStateUI(GameManager.GameState newState)
    {
        if (newState == GameManager.GameState.Racing)
        {
            SlideInCanvas(TransitionCanvas);
            StartCoroutine(StartCountdown());
        }
        else if (newState == GameManager.GameState.Paused)
        {
            SlideOutCanvas(TransitionCanvas);
        }
    }

    private void SlideInCanvas(GameObject canvas)
    {
        canvas.SetActive(true);
        LSequence.Create()
            .Insert(0f, LMotion.Create(offScreenBottom, onScreen, transitionDuration).BindToLocalPositionY(canvas.transform))
            .Insert(0.1f, LMotion.Create(0f, 1f, transitionDuration).BindToAlpha(canvas.GetComponent<CanvasGroup>()))
            .Run();
    }

    private void SlideOutCanvas(GameObject canvas)
    {
        if (canvas == null) return;

        var anim = LSequence.Create()
            .Insert(0f, LMotion.Create(onScreen, offScreenTop, transitionDuration).BindToLocalPositionY(canvas.transform))
            .Insert(0.1f, LMotion.Create(1f, 0f, transitionDuration).BindToAlpha(canvas.GetComponent<CanvasGroup>()))
            .Run();

        if (!anim.IsActive())
        {
            canvas.SetActive(false);
        }
    }
}
