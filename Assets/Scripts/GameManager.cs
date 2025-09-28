using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static event Action<Vector2> OnInGameLeftClick;

    public static GameState GameState;

    void Awake()
    {
        GameState = GameState.MainGame;  
    }
    void OnEnable()
    {
        InputManager.OnLeftClick += HandleLeftClick;
    }

    private void HandleLeftClick(Vector2 mousePos)
    {
        switch (GameState)
        {
            case GameState.MainGame:
                OnInGameLeftClick?.Invoke(mousePos);
                break;
        }
    }

    void OnDisable()
    {
        InputManager.OnLeftClick -= HandleLeftClick;
    }
}

public enum GameState
{
    MainGame,
}
