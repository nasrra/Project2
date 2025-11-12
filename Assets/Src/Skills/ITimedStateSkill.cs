using Entropek.Time;
using UnityEngine;

public interface ITimedStateSkill
{
    OneShotTimer StateTimer{get;}



    bool CanUseTimedStateSkill()
    {
        return StateTimer.CurrentTime == 0;
    }

    void UseTimedStateSkill()
    {
        StateTimer.Begin();
    }

    void LinkTimedStateSkillEvents()
    {
        StateTimer.Timeout += OnStateTimerTimeout;
    }

    void UnlinkTimedStateSkillEvents()
    {
        StateTimer.Timeout -= OnStateTimerTimeout;        
    }

    protected void OnStateTimerTimeout();
}
