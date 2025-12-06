using Entropek;
using UnityEngine;

namespace Entropek.Ui
{    
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
}

