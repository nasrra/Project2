using Entropek;

namespace Entropek.Ui
{
    public class VsyncDisplaySettingsToggle : DisplaySettingsToggle
    {
        protected override void LoadValue()
        {        
            toggle.isOn = DisplaySettingsManager.Singleton.GetVsyncPreset();
        }

        protected override void OnToggleValueChanged(bool value)
        {
            DisplaySettingsManager.Singleton.SetVsync(value);
        }
    }    
}

