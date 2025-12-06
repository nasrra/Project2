using System;
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
    }

    private void UnlinkGameManagerEvents()
    {
        GameManager.Singleton.GamePaused -= OnGamePaused;        
        GameManager.Singleton.GameResumed -= OnGameResumed;
    }

    private void OnGamePaused()
    {
        menus[PauseMenuId].gameObject.SetActive(true);
    }

    private void OnGameResumed()
    {
        menus[PauseMenuId].gameObject.SetActive(false);        
    }
}
