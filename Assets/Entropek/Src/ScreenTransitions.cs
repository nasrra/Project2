using System;
using Entropek.UnityUtils.AnimatorUtils;
using UnityEngine;

public class ScreenTransitions : MonoBehaviour
{
    private const string FadeToBlackAnimation = "FadeToBlack";
    private const string FadeFromBlackAnimation = "FadeFromBlack";

    private const string FadeToBlackCompletedAnimationEvent = "FadeToBlackCompleted";
    private const string FadeFromBlackCompletedAnimationEvent = "FadeFromBlackCompleted";

    private const string ResourcesPath = "Entropek/Ui/"+nameof(ScreenTransitions);

    public static ScreenTransitions Singleton {get; private set;}

    public event Action FadeToBlackCompleted;
    public event Action FadeFromBlackCompleted;

    [SerializeField] Animator animator;
    [SerializeField] AnimationEventReciever animationEventReciever;


    /// 
    /// Base.
    /// 


    private void Awake()
    {
        LinkEvents();
    }

    private void OnDestroy()
    {
        UnlinkEvents();
    }


    /// 
    /// Unique Functions.
    /// 


    /// <summary>
    /// Fades the entire screen to black.
    /// </summary>

    public void FadeToBlack()
    {
        animator.Play(FadeToBlackAnimation);
    }

    /// <summary>
    /// Fades the entire screen from black.
    /// </summary>

    public void FadeFromBlack()
    {
        animator.Play(FadeFromBlackAnimation);
    }


    ///
    /// Event Linkage.
    /// 


    private void LinkEvents()
    {
        LinkAnimationEventRecieverEvents();
    }

    private void UnlinkEvents()
    {
        UnlinkAnimationEventRecieverEvents();
    }

    private void LinkAnimationEventRecieverEvents()
    {
        animationEventReciever.AnimationEventTriggered += OnAnimationEventTriggeredWrapper;
    }

    private void UnlinkAnimationEventRecieverEvents()
    {
        animationEventReciever.AnimationEventTriggered -= OnAnimationEventTriggeredWrapper;        
    }

    private void OnAnimationEventTriggeredWrapper(string eventName)
    {
        OnAnimationEventTriggered(eventName);
    }

    private bool OnAnimationEventTriggered(string eventName)
    {
        switch (eventName)
        {
            case FadeToBlackCompletedAnimationEvent:
                FadeToBlackCompleted?.Invoke();
                return true;
            case FadeFromBlackCompletedAnimationEvent:
                FadeFromBlackCompleted?.Invoke();
                return true;
            default:
                return false;
        }
    }


    /// 
    /// Bootstrap
    /// 


    private static class Bootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialise()
        {
            GameObject main = Instantiate(Resources.Load<GameObject>(ResourcesPath));
            Singleton = main.GetComponent<ScreenTransitions>();
            DontDestroyOnLoad(main);
        }
    }
}
