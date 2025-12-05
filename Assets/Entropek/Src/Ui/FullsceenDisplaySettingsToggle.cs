using Entropek;
using UnityEngine;
using UnityEngine.UI;

public class FullsceenDisplaySettingsToggle : DisplaySettingsToggle
{
    protected override void LoadValue()
    {        
        toggle.isOn = DisplaySettingsManager.Singleton.GetIsFullscreen();
    }

    protected override void OnToggleValueChanged(bool value)
    {
        DisplaySettingsManager.Singleton.SetFullscreen(value);
    }
}
