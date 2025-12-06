using System;
using System.Collections;
using Entropek.Exceptions;
using UnityEngine;

public class MenuManager : MonoBehaviour
{

    
    private const int PauseMenuId = 0;
    public static MenuManager Singleton {get; private set;}
    [SerializeField] private Canvas[] menus;


    /// 
    /// Base.
    /// 


    void Awake()
    {
        if(Singleton == null)
        {
            Singleton = this;
        }
        else if(Singleton != this)
        {
            throw new SingletonException($"There can be only one {nameof(MenuManager)}!");
        }

        LinkEvents();
    }

    void OnDestroy()
    {
        if(Singleton == this)
        {
            Singleton = null;
        }

        UnlinkEvents();
    }


    /// 
    /// Event Linkage.
    /// 


    private void LinkEvents()
    {
        LinkGameManagerEvents();
    }

    private void UnlinkEvents()
    {
        UnlinkGameManagerEvents();
    }

    private void LinkGameManagerEvents()
    {
        GameManager.Singleton.GamePaused += OnGamePaused;
        GameManager.Singleton.GameResumed += OnGameResumed;
        GameManager.Singleton.GameStateSet += OnGameStateSet;
    }

    private void UnlinkGameManagerEvents()
    {
        GameManager.Singleton.GamePaused -= OnGamePaused;        
        GameManager.Singleton.GameResumed -= OnGameResumed;
        GameManager.Singleton.GameStateSet -= OnGameStateSet;
    }

    private void OnGamePaused()
    {
        GameManager.Singleton.EnableCursor();
        menus[PauseMenuId].gameObject.SetActive(true);
        InputManager.Singleton.EnableMenuInput();
        InputManager.Singleton.DisableGameplayInputDeferred();
    }

    private void OnGameResumed()
    {
        // re-enable the cursor.

        GameManager.Singleton.DisableCursor();

        for(int i = 0; i < menus.Length; i++)
        {
            menus[i].gameObject.SetActive(false);        
        }

        InputManager.Singleton.EnableGameplayInput();
        InputManager.Singleton.DisableMenuInputDeferred();
    }

    private void OnGameStateSet(GameState gameState)
    {
        switch (gameState)
        {
            case GameState.Gameplay:
                GameManager.Singleton.DisableCursor();
            break;
            default:
                GameManager.Singleton.EnableCursor();
            break;
        }
    }
}
