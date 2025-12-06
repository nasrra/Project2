using Entropek;
using UnityEngine;
using UnityEngine.UI;

namespace Entropek.Ui
{    
    public class ResolutionSettingsDropdown : DisplaySettingsDropDown
    {
        protected override void LoadValue()
        {
            dropdown.value = DisplaySettingsManager.Singleton.GetResolutionPreset();
        }

        protected override void OnValueChanged(int value)
        {
            DisplaySettingsManager.Singleton.SetResolution(value);
        }
    }
}

