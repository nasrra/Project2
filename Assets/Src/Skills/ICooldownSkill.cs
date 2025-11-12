using Entropek.Time;
using Entropek.UnityUtils.Attributes;
using UnityEngine;

public interface ICooldownSkill
{
    [SerializeField] OneShotTimer CooldownTimer {get;}

    bool CanUseCooldownSkill()
    {
        return CooldownTimer.CurrentTime == 0;
    }

    void LinkCooldownSkillEvents()
    {
        CooldownTimer.Timeout += OnCooldownTimeout;
    }

    void UnlinkCooldownSkillEvents()
    {
        CooldownTimer.Timeout -= OnCooldownTimeout;        
    }

    void OnCooldownTimeout();
}
