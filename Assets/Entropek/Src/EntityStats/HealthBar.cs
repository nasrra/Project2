using UnityEngine;
using UnityEngine.UI;

namespace Entropek.EntityStats{


    public class HealthBar : MonoBehaviour{
        
        [Header("Components")]
        [SerializeField] private Slider slider;
        private Health health;


        /// 
        /// Base.
        /// 


        private void OnEnable(){
            if(health!=null){
                LinkEvents();
            }
        }

        private void OnDisable(){
            if(health!=null){
                UnlinkEvents();        
            }
        }


        /// 
        /// Functions.
        /// 


        public void DisplayHealth(Health health){

            // unlink from the previous health if there was any.

            if(this.health!=null){
                UnlinkHealth();
            }

            this.health     = health;
            slider.maxValue = health.GetMaxHealthValue();
            slider.value    = health.GetHealthValue();

            // link to this new one.

            LinkHealth();
        }


        /// 
        /// Linkage.
        /// 


        private void LinkEvents(){
            LinkHealth();
        }

        private void UnlinkEvents(){
            UnlinkHealth();
        }

        private void LinkHealth(){
            health.HealthDamaged += OnHealthDamaged;
            health.Healed        += OnHealthHealed;
        }

        private void UnlinkHealth(){
            health.HealthDamaged -= OnHealthDamaged;
            health.Healed        -= OnHealthHealed;
        }

        private void OnHealthHealed(float amount){
            slider.value = health.GetHealthValue();
        }

        private void OnHealthDamaged(float amount){
            slider.value = health.GetHealthValue();
        }
    }


}

