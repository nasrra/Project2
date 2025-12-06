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


    public event Action GamePaused;
    public event Action GameResumed;
    public event Action<GameState> GameStateSet;


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
    /// Event Linkage.
    /// 


    private void LinkEvents()
    {
        LinkInputManagerEvents();
        LinkSceneManagerEvents();
    }

    private void UnlinkEvents()
    {
        UnlinkInputManagerEvents();
        UnlinkSceneManagerEvents();
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
                PauseGame();
                SetGameState(GameState.PauseMenu);
            }
            else
            {
                ResumeGame();
                SetGameState(GameState.Gameplay);
            }            
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0;
        GameIsPaused = true;
        GamePaused?.Invoke();
    }

    public void ResumeGame()
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
                SetGameState(GameState.MainMenu);
                break;
            case TheHollowSceneName:
                SetGameState(GameState.Gameplay);
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
