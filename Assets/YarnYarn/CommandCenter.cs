using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using Yarn.Unity;
using LitMotion;

public class CommandCenter : DialogueViewBase
{
    DialogueRunner runner;

    void Awake()
    {
        runner = GetComponent<DialogueRunner>();
        runner.AddCommandHandler<int>("AddMoney", AddMoney);
        runner.AddCommandHandler<string>("AddItem", AddItem);
        runner.AddCommandHandler<string>("GameState", GameState);

    }

    public void AddMoney(int amount)
    {
        GameManager.Instance.AddMoney(amount);
    }

    public void AddItem(string item)
    {
        Inventory.Instance.AddItem(item);
    }
    public void GameState(string state)
    {
        GameManager.GameState gameState;
        if (Enum.TryParse(state, out gameState))
        {
            GameManager.Instance.ChangeGameState(gameState);
        }
        else
        {
            Debug.LogError($"Invalid game state: {state}");
        }

    }
    private void OnEnable()
    {
        GameManager.OnGameStateChanged += GameStateChanged;
    }
    private void OnDisable()
    {
        GameManager.OnGameStateChanged -= GameStateChanged;
    }

    private void GameStateChanged(GameManager.GameState newState)
    {
        Debug.Log("Should be called");
        if(newState == GameManager.GameState.MainMenu)
        {
            Debug.Log("Main Menu Activated");
        }
        else if(newState == GameManager.GameState.Racing)
        {
            Debug.Log("Racing Activated");
        }
        else if(newState == GameManager.GameState.Paused)
        {
            Debug.Log("Paused Activated");
        }
        else if(newState == GameManager.GameState.TycoonManagement)
        {
            Debug.Log("Tycoon Activated");
        }
        else
        if(newState == GameManager.GameState.CheatMode)
        {
            if(runner == null) return;
            Debug.Log("Cheat Mode Activated");
            runner.StartDialogue("Cheat");
        }
    }

}