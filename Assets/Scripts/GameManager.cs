using System;
using System.IO;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Singleton Instance
    public static GameManager Instance { get; private set; }

    // Static Events
    public delegate void GameStateChangedDelegate(GameState newState);
    public delegate void MoneyChangedDelegate(int newMoney);
    public delegate void RaceCompletedDelegate(int completedRaces);
    public delegate void EnergyChangedDelegate(int newEnergy);

    public static event GameStateChangedDelegate OnGameStateChanged;
    public static event MoneyChangedDelegate OnMoneyChanged;
    public static event RaceCompletedDelegate OnRaceCompleted;
    public static event EnergyChangedDelegate OnEnergyChanged;

    // Enums
    public enum GameState{ MainMenu, Racing, TycoonManagement, Paused, CheatMode }

    // Private Variables
    [Header("Player Data")]
    [SerializeField] private int playerMoney = 1000;
    [SerializeField] private int completedRaces = 0;
    [SerializeField] private int playerEnergy = 100;
    [SerializeField] private GameState currentState = GameState.MainMenu;

    // Properties
    public int PlayerMoney
    {
        get => playerMoney;
        set
        {
            if (playerMoney != value)
            {
                playerMoney = value;
                OnMoneyChanged?.Invoke(playerMoney);
            }
        }
    }

    public int CompletedRaces
    {
        get => completedRaces;
        set
        {
            if (completedRaces != value)
            {
                completedRaces = value;
                OnRaceCompleted?.Invoke(completedRaces);
            }
        }
    }

    public int PlayerEnergy
    {
        get => playerEnergy;
        set
        {
            if (playerEnergy != value)
            {
                playerEnergy = value;
                OnEnergyChanged?.Invoke(playerEnergy);
            }
        }
    }

    public GameState CurrentState
    {
        get => currentState;
        set
        {
            if (currentState != value)
            {
                currentState = value;
                OnGameStateChanged?.Invoke(currentState);
            }
        }
    }

    // Unity Lifecycle
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        LoadGame(); // Load saved data or initialize defaults
    }

    // Public Methods
    public void ChangeGameState(GameState newState)
    {
        CurrentState = newState;
    }

    public void AddMoney(int amount)
    {
        PlayerMoney += amount;
    }

    public void SubtractMoney(int amount)
    {
        PlayerMoney -= amount;
        if (PlayerMoney < 0) PlayerMoney = 0;
    }

    public void CompleteRace()
    {
        CompletedRaces++;
    }

    public void SaveGame()
    {
        SaveData saveData = new SaveData
        {
            playerMoney = playerMoney,
            completedRaces = completedRaces,
            playerEnergy = playerEnergy,
            // currentState = currentState
        };

        string json = JsonUtility.ToJson(saveData);
        File.WriteAllText(Application.persistentDataPath + "/savefile.json", json);
        Debug.Log("Game Saved: " + json);
    }

    public void LoadGame()
    {
        string filePath = Application.persistentDataPath + "/savefile.json";

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);

            playerMoney = saveData.playerMoney;
            completedRaces = saveData.completedRaces;
            playerEnergy = saveData.playerEnergy;
            // currentState = saveData.currentState;

            InitializeGame();
            Debug.Log("Game Loaded: " + json);
        }
        else
        {
            Debug.LogWarning("No save file found. Using default values.");
            InitializeGame();
        }
    }

    private void InitializeGame()
    {
        PlayerMoney = playerMoney;
        CompletedRaces = completedRaces;
        PlayerEnergy = playerEnergy;
        // CurrentState = currentState;
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }
}

// SaveData Class
[Serializable]
public class SaveData
{
    public int playerMoney;
    public int completedRaces;
    public int playerEnergy;
    public GameManager.GameState currentState;
}
