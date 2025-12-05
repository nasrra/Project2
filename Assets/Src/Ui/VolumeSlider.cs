using Entropek.Audio;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    [SerializeField] Slider slider;
    [SerializeField] BusHandle busHandle;


    /// 
    /// Base.
    /// 


    void Awake()
    {
        LinkEvents();
    }

    void OnDestroy()
    {
        UnlinkEvents();
    }


    /// 
    /// Event Linkage.
    /// 


    private void LinkEvents()
    {
        LinkSliderEvents();        
    }

    private void UnlinkEvents()
    {
        UnlinkSliderEvents();
    }

    private void LinkSliderEvents()
    {
        slider.onValueChanged.AddListener(OnSliderValueChanged); 
    }

    private void UnlinkSliderEvents()
    {
        slider.onValueChanged.RemoveListener(OnSliderValueChanged);         
    }

    private void OnSliderValueChanged(float value)
    {
        value = value > float.Epsilon
        ? value
        : 0;

        if(value > float.Epsilon)
        {
            AudioManager.Singleton.UnmuteBus(busHandle);
            AudioManager.Singleton.SetBusVolume(busHandle, value>float.Epsilon?value:0);
        }
        else
        {
            AudioManager.Singleton.MuteBus(busHandle);            
        }
    }
}
