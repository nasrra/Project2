using System;
using Entropek.SceneManaging;
using Entropek.UnityUtils.Attributes;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{   


    /// 
    /// Constants.
    /// 

    
    private const string MainMenuSceneName = "MainMenu";
    private const string TheHollowSceneName = "TheHollow";

    public static GameManager Singleton {get; private set;}


    ///
    /// Callbacks.
    /// 


    public event Action<GameState> GameStateSet;
    public event Action GamePaused;
    public event Action GameResumed;


    /// 
    /// Data.
    /// 


    [RuntimeField] public GameState GameState {get; private set;} = GameState.None;
    [RuntimeField] public bool GameIsPaused {get; private set;} = false;


    void Start()
    {
        // link events in start instead of Awake as
        // the game manager links into singletons that also use 
        // a bootstrap class.

        LinkEvents();
        SetGameStateBySceneName(SceneManager.GetActiveScene().name);
    }

    void OnDestroy()
    {
        UnlinkEvents();
    }


    ///
    /// State Machine.
    /// 


    public void WinState()
    {
        EnableCursor();
        PauseGame();

        InputManager.Singleton.EnableMenuInput();
        InputManager.Singleton.DisableGameplayInputDeferred();
    
        GameState = GameState.Win;
        GameStateSet?.Invoke(GameState.Win);        
    }

    public void DeathState()
    {
        EnableCursor();
        PauseGame();

        InputManager.Singleton.EnableMenuInput();
        InputManager.Singleton.DisableGameplayInputDeferred();

        GameState = GameState.Death;
        GameStateSet?.Invoke(GameState.Death);
    }

    public void MainMenuState()
    {
        EnableCursor();
        ResumeGame();

        InputManager.Singleton.EnableMenuInput();
        InputManager.Singleton.DisableGameplayInputDeferred();

        GameState = GameState.MainMenu;
        GameStateSet?.Invoke(GameState.MainMenu);
    } 

    public void GameplayState()
    {
        DisableCursor();
        ResumeGame();

        InputManager.Singleton.EnableGameplayInput();
        InputManager.Singleton.DisableMenuInputDeferred();

        GameState = GameState.Gameplay;
        GameStateSet?.Invoke(GameState.Gameplay);
    }

    public void PauseMenuState()
    {
        EnableCursor();
        PauseGame();
        
        InputManager.Singleton.EnableMenuInput();
        InputManager.Singleton.DisableGameplayInputDeferred();

        GameState = GameState.PauseMenu;
        GameStateSet?.Invoke(GameState.PauseMenu);
    }


    /// 
    /// Event Linkage.
    /// 


    private void LinkEvents()
    {
        LinkInputManagerEvents();
        LinkSceneManagerEvents();
        LinkPlayerEvents();
        LinkPlaythroughStopwatchEvents();
    }

    private void UnlinkEvents()
    {
        UnlinkInputManagerEvents();
        UnlinkSceneManagerEvents();
        UnlinkPlayerEvents();
        UnlinkPlaythroughStopwatchEvents();
    }


    /// 
    /// Input Manager Linkage.
    /// 


    private void LinkInputManagerEvents()
    {
        InputManager.Singleton.PauseMenuToggle += OnPauseMenuToggle;
    }

    private void UnlinkInputManagerEvents()
    {
        InputManager.Singleton.PauseMenuToggle -= OnPauseMenuToggle;        
    }

    private void OnPauseMenuToggle()
    {
        if(GameState == GameState.Gameplay
        || GameState == GameState.PauseMenu)
        {
            if (GameIsPaused == false)
            {
                PauseMenuState();
            }
            else
            {
                GameplayState();
            }            
        }
    }

    private void PauseGame()
    {
        Time.timeScale = 0;
        GameIsPaused = true;
        GamePaused?.Invoke();
    }

    private void ResumeGame()
    {
        Time.timeScale = 1;
        GameIsPaused = false;
        GameResumed?.Invoke();
    }

    /// <summary>
    /// Sets the appropriate GameState dependning upon the name of a given scene.
    /// </summary>
    /// <param name="sceneName">The specified name of a Scene.</param>

    private void SetGameStateBySceneName(string sceneName)
    {
        switch (sceneName)
        {
            case MainMenuSceneName:
                MainMenuState();
                break;
            case TheHollowSceneName:
                GameplayState();
                break;
            default:
                SetGameState(GameState.None);
                break;
        }
    }

    private void SetGameState(GameState gameState)
    {
        GameState = gameState;
        GameStateSet?.Invoke(gameState);
    }

    public void EnableCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void DisableCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;        
    }


    /// 
    /// Custom Scene Manager Linkage.
    /// 


    private void LinkSceneManagerEvents()
    {
        CustomSceneManager.Singleton.LoadingScene += OnLoadingScene;
    }

    private void UnlinkSceneManagerEvents()
    {
        CustomSceneManager.Singleton.LoadingScene -= OnLoadingScene;
    }

    private void OnLoadingScene(string sceneName)
    {
        SetGameStateBySceneName(sceneName);
        if(GameState == GameState.MainMenu)
        {
            ResumeGame();
        }
    }


    ///
    /// Player Event Linkage.
    /// 

    
    private void LinkPlayerEvents()
    {
        Player.Death += OnPlayerDeath;
    }

    private void UnlinkPlayerEvents()
    {
        Player.Death -= OnPlayerDeath;        
    }

    private void OnPlayerDeath()
    {
        DeathState();   
    }


    ///
    /// PlaythroughStopwatch link events.
    /// 


    private void LinkPlaythroughStopwatchEvents()
    {
        PlaythroughStopwatch.ElapsedWinTime += OnElapsedWinTime;
    }

    private void UnlinkPlaythroughStopwatchEvents()
    {
        PlaythroughStopwatch.ElapsedWinTime -= OnElapsedWinTime;        
    }

    private void OnElapsedWinTime()
    {
        WinState();
    }


    /// 
    /// Bootstrap.
    /// 


    private static class Bootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialise()
        {
            GameObject main = new();
            main.name = nameof(GameManager);
            Singleton = main.AddComponent<GameManager>();
            DontDestroyOnLoad(main);
        }
    }
}
