using UnityEngine;

namespace Entropek.Lighting
{
    public class SimpleLightDecay : MonoBehaviour
    {
        [SerializeField] new Light light;
        [SerializeField] float decaySpeed;
        [SerializeField] float initialIntensity;

        void OnEnable()
        {
            light.intensity = initialIntensity;
        }
        
        void FixedUpdate()
        {
            float lightIntensity = light.intensity; 

            if (lightIntensity > 0)
            {
                lightIntensity -= decaySpeed;
                if (lightIntensity < 0)
                {
                    lightIntensity = 0;
                }

                light.intensity = lightIntensity;
            }
        }
    }
}

