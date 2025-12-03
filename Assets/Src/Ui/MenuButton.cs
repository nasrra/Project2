using Entropek.UnityUtils.AnimatorUtils;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class MenuButton : MonoBehaviour,
    IPointerEnterHandler,
    IPointerClickHandler
{
    private const string PointerEnterCompletedAnimationEvent = "PointerEnterCompleted";
    private const string PointerClickCompletedAnimationEvent = "PointerClickCompleted";

    private const string PointerEnterAnimation = "PointerEnter";
    private const string PointerClickAnimation = "PointerClick";

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


    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        animator.Play(PointerClickAnimation);
        OnPointerClick(eventData);
    }

    protected abstract void OnPointerClick(PointerEventData eventData);

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        animator.Play(PointerEnterAnimation);
        OnPointerEnter(eventData);
    }

    protected abstract void OnPointerEnter(PointerEventData eventData);


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


    /// 
    /// AnimationEventReciever Event Linkage.
    /// 


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
        OnAnimationEventTriggerd(eventName);
    }

    protected bool OnAnimationEventTriggerd(string eventName)
    {
        switch (eventName)
        {
            case PointerEnterCompletedAnimationEvent:
                OnPointerEnterAnimationCompleted();
                return true;
            case PointerClickCompletedAnimationEvent:
                OnPointerClickAnimationCompleted();
                return true;
            default:
                return false;
        }
    }

    protected abstract void OnPointerEnterAnimationCompleted();
    protected abstract void OnPointerClickAnimationCompleted();
}
