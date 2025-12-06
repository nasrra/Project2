using Entropek;
using TMPro;
using UnityEngine;
using static TMPro.TMP_Dropdown;

public abstract class DisplaySettingsDropDown : DisplaySettingsUiElement{
    [SerializeField] protected TMP_Dropdown dropdown;

    protected virtual void OnValueChangedInternal(int value)
    {
        StartTempSet();
        OnValueChanged(value);
    }

    protected abstract void OnValueChanged(int value);

    protected override void LinkUiElementEvents()
    {
        dropdown.onValueChanged.AddListener(OnValueChangedInternal);
    }

    protected override void UnlinkUiElementEvents(){
        dropdown.onValueChanged.RemoveListener(OnValueChangedInternal);
    }
}