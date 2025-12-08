using System;
using System.Collections;
using Entropek.Exceptions;
using UnityEngine;

public class MenuManager : MonoBehaviour
{

    
    private const int PauseMenuId = 0;
    private const int DeathSreenId = 1;
    private const int WinScreenId = 2;

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


    /// 
    /// Game Manager Event Linkage.
    /// 


    private void LinkGameManagerEvents()
    {
        GameManager.Singleton.GameStateSet += OnGameStateSet;
    }

    private void UnlinkGameManagerEvents()
    {
        GameManager.Singleton.GameStateSet -= OnGameStateSet;
    }

    private void OnGameStatePauseMenu()
    {
        menus[PauseMenuId].gameObject.SetActive(true);
    }

    private void OnGameStateGameplay()
    {
        for(int i = 0; i < menus.Length; i++)
        {
            menus[i].gameObject.SetActive(false);        
        }
    }

    private void OnGameStateDeath()
    {
        menus[DeathSreenId].gameObject.SetActive(true);
    }
    
    private void OnGameStateWin()
    {
        menus[WinScreenId].gameObject.SetActive(true);
    }

    private void OnGameStateSet(GameState gameState)
    {
        switch (gameState)
        {
            case GameState.PauseMenu:
                OnGameStatePauseMenu();
                break;
            case GameState.Gameplay:
                OnGameStateGameplay();
                break;
            case GameState.Death:
                OnGameStateDeath();
                break;
            case GameState.Win:
                OnGameStateWin();
                break;
        }
    }

}
