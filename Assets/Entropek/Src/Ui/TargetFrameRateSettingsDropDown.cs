using Entropek;
using UnityEngine;

public class TargetFrameRateSettingsDropDown : DisplaySettingsDropDown
{
    protected override void LoadValue()
    {
        dropdown.value = DisplaySettingsManager.Singleton.GetTargetFrameRatePreset();
    }

    protected override void OnValueChanged(int value)
    {
        DisplaySettingsManager.Singleton.SetTargetFrameRatePreset(value);
    }
}
