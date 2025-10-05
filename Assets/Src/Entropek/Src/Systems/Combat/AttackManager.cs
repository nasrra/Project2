using System;
using Entropek.Systems.Combat;
using UnityEngine;

namespace Entropek.Systems.Combat{

    public class AttackManager : MonoBehaviour{
        
        public event Action AttackHit;

        [Header("Attacks")]
        [SerializeField] private AttackInstance[] attacks;


        /// 
        /// Base.
        /// 


        private void OnEnable(){
            LinkEvents();
        }

        private void OnDisable(){
            UnlinkEvents();        
        }


        /// 
        /// Functions.
        /// 


        public void BeginAttack(int attackId){
            attacks[attackId].Begin();
        }


        /// 
        /// Linkage.
        /// 


        void LinkEvents(){
            LinkAttackInstanceEvents();
        }

        void UnlinkEvents(){
            UnlinkAttackInstanceEvents();
        }

        void LinkAttackInstanceEvents(){

            // link attack instances as they are not MonoBehaviour
            // and cannot call it themselves.

            for(int i = 0; i < attacks.Length; i++){
                
                AttackInstance attack = attacks[i];
                
                attack.LinkEvents();
                attack.Hitbox.Hit += OnAttackHitboxHit;
            }
        }

        private void UnlinkAttackInstanceEvents(){

            // unlink attack instances as they are not MonoBehaviour
            // and cannot call it themselves.

            for(int i = 0; i < attacks.Length; i++){

                AttackInstance attack = attacks[i];
                
                attack.UnlinkEvents();
                attack.Hitbox.Hit -= OnAttackHitboxHit;
            }
        }

        private void OnAttackHitboxHit(GameObject gameObject){
            AttackHit?.Invoke();   
        }


    }

}

