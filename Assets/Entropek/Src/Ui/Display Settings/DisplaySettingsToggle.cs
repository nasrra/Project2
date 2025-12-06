using Entropek;
using UnityEngine;
using UnityEngine.UI;

public abstract class DisplaySettingsToggle : DisplaySettingsUiElement
{
    [Header(nameof(DisplaySettingsToggle)+" Components.")]
    [SerializeField] protected Toggle toggle;
    
    protected override void LinkUiElementEvents()
    {
        toggle.onValueChanged.AddListener(OnToggleValueChangedInternal);
    }

    protected override void UnlinkUiElementEvents()
    {
        toggle.onValueChanged.RemoveListener(OnToggleValueChangedInternal);        
    }

    private void OnToggleValueChangedInternal(bool value)
    {
        StartTempSet();
        OnToggleValueChanged(value);
    }

    protected abstract void OnToggleValueChanged(bool value);

}
