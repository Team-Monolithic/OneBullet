using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public enum GameState
{
    Editor,
    Playing,
}

public class GameplayManager : Singleton<GameplayManager>
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button pauseButton;
    [SerializeField] private GameObject playerCharacter;

    public GameState currentState;
    public UnityEvent gamePlayed;
    public UnityEvent gamePaused;
    
    private bool gameInitialized = false;
    private Color normalColor = new Color(1f, 1f, 1f);
    private Color pressedColor = new Color(0.55f, 0.55f, 0.55f);
    
    private void Start()
    {
        playButton.onClick.AddListener(StartButtonClicked);
        pauseButton.onClick.AddListener(PauseGame);
    }

    private void InitGame()
    {
        BuildingCreator creator = BuildingCreator.GetInstance();
        if (creator.playerStartPosition == Vector3Int.back)
        {
            Debug.LogError("No PlayerStart");
            return;
        }
            
        // Spawn
        Instantiate(playerCharacter, creator.playerStartPosition, Quaternion.identity);
        
        // TODO : 모든 오브젝트 정위치

        gameInitialized = true;
    }

    private void StartButtonClicked()
    {
        if (gameInitialized == false)
        {
            InitGame();
        }
        StartGame();
    }
    
    private void StartGame()
    {
        currentState = GameState.Playing;
        Time.timeScale = 1;
        gamePlayed.Invoke();
        
        SetButtonState(playButton, true);
        SetButtonState(pauseButton, false);
    }
    
    private void PauseGame()
    {
        currentState = GameState.Editor;
        Time.timeScale = 0;
        gamePaused.Invoke();
        
        SetButtonState(playButton, false);
        SetButtonState(pauseButton, true);
    }

    private void SetButtonState(Button targetButton, bool isClicked)
    {
        ColorBlock colorBlock = targetButton.colors;
        colorBlock.normalColor = isClicked ? pressedColor : normalColor;
        targetButton.colors = colorBlock;
    }
}
