using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LitMotion;
using LitMotion.Extensions;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI raceCompletedText;

    [Header("Canvas References")]
    public GameObject mainMenuCanvas;
    public GameObject racingCanvas;
    public GameObject pausedCanvas;
    public GameObject tycoonCanvas;
    public GameObject cheatCanvas;

    private GameObject currentCanvas; // Tracks the currently active canvas

    [Header("UI Animations")]
    [SerializeField] private float offScreenbottom = -450f;
    [SerializeField] private float offScreentop = 450f;
    [SerializeField] private float offScreenleft = -800f;
    [SerializeField] private float offScreenright = 800f;
    [SerializeField] private float onScreen = 0f;
    [SerializeField] private float transitionDuration = 0.5f;


    void Start()
    {
        // Set the initial canvas
        currentCanvas = mainMenuCanvas;

        // Update UI elements
        UpdateMoneyUI(GameManager.Instance.PlayerMoney);
        UpdateRaceUI(GameManager.Instance.CompletedRaces);
        UpdateGameStateUI(GameManager.Instance.CurrentState);
        
        mainMenuCanvas.SetActive(true);
        racingCanvas.SetActive(false);
        pausedCanvas.SetActive(false);
        tycoonCanvas.SetActive(false);
        cheatCanvas.SetActive(false);
    }

    private void OnEnable()
    {
        // Subscribe to GameManager events
        GameManager.OnMoneyChanged += UpdateMoneyUI;
        GameManager.OnRaceCompleted += UpdateRaceUI;
        GameManager.OnGameStateChanged += UpdateGameStateUI;
    }

    private void OnDisable()
    {
        // Unsubscribe from GameManager events
        GameManager.OnMoneyChanged -= UpdateMoneyUI;
        GameManager.OnRaceCompleted -= UpdateRaceUI;
        GameManager.OnGameStateChanged -= UpdateGameStateUI;
    }

    private void UpdateMoneyUI(int newMoney)
    {
        if (moneyText == null) return;
        moneyText.text = $"Money: {newMoney}";
    }

    private void UpdateRaceUI(int completedRaces)
    {
        if (raceCompletedText == null) return;
        raceCompletedText.text = $"Races Completed: {completedRaces}";
    }

    private void UpdateGameStateUI(GameManager.GameState newState)
    {
        Debug.Log("State Changed: " + newState);

        GameObject targetCanvas = GetCanvasForState(newState);

        if (targetCanvas == null || currentCanvas == targetCanvas)
        {
            Debug.LogWarning("No canvas change or invalid target canvas.");
            return;
        }

        // Slide out the current canvas and slide in the target canvas
        SlideOutCanvas(currentCanvas);
        SlideInCanvas(targetCanvas);

        // Update the current canvas
        currentCanvas = targetCanvas;
    }

    private GameObject GetCanvasForState(GameManager.GameState state)
    {
        return state switch
        {
            GameManager.GameState.MainMenu => mainMenuCanvas,
            GameManager.GameState.Racing => racingCanvas,
            GameManager.GameState.Paused => pausedCanvas,
            GameManager.GameState.TycoonManagement => tycoonCanvas,
            GameManager.GameState.CheatMode => cheatCanvas,
            _ => null,
        };
    }

    private void SlideInCanvas(GameObject canvas)
    {
        canvas.SetActive(true); // Ensure the canvas is active
        LSequence.Create()
            .Insert(0f, LMotion.Create(offScreenbottom, onScreen, transitionDuration).BindToLocalPositionY(canvas.transform)) // Slide from bottom
            .Insert(0.1f, LMotion.Create(0f, 1f, transitionDuration).BindToAlpha(canvas.GetComponent<CanvasGroup>())) // Fade in
            .Run();
    }

    private void SlideOutCanvas(GameObject canvas)
    {
        if (canvas == null) return;

        var Anim = LSequence.Create()
            .Insert(0f, LMotion.Create(onScreen, offScreentop, transitionDuration).BindToLocalPositionY(canvas.transform)) // Slide to top
            .Insert(0.1f, LMotion.Create(1f, 0f, transitionDuration).BindToAlpha(canvas.GetComponent<CanvasGroup>())) // Fade out
            .Run();
        if(Anim.IsActive() == false)
        {
            canvas.SetActive(false); // Deactivate the canvas
        }
    }

    // Button Callbacks
    public void OnStartButtonClicked() => GameManager.Instance.CurrentState = GameManager.GameState.Racing;

    public void OnPauseButtonClicked() => GameManager.Instance.CurrentState = GameManager.GameState.Paused;

    public void OnTycoonButtonClicked() => GameManager.Instance.CurrentState = GameManager.GameState.TycoonManagement;

    public void OnCheatButtonClicked() => GameManager.Instance.CurrentState = GameManager.GameState.CheatMode;

    public void OnMenuButtonClicked() => GameManager.Instance.CurrentState = GameManager.GameState.MainMenu;

    public void OnQuitButtonClicked() => Application.Quit();
}
