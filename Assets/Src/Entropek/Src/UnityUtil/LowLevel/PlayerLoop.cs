using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.LowLevel;

namespace Entropek.UnityUtils.LowLevel{

    public class PlayerLoop{


        ///
        /// Remove a system from the player loop.
        /// 

        
        public static void RemoveSystem<T>(ref PlayerLoopSystem loop, in PlayerLoopSystem systemToRemove){
            
            // short circuit if this is a leaf node in the player loop tree.
            
            if(loop.subSystemList==null){
                return;
            }

            List<PlayerLoopSystem> subSystems = new List<PlayerLoopSystem>(loop.subSystemList);
            for(int i= 0; i < subSystems.Count; i++){
                
                // check if the sub systems type and update delegate matches the system to remove.
                
                if(subSystems[i].type == systemToRemove.type
                && subSystems[i].updateFunction == systemToRemove.updateFunction){
                    
                    subSystems.RemoveAt(i);
                    loop.subSystemList = subSystems.ToArray();
                                        
                    break;

                }
            }

            HandleSubSystemLoopForRemoval<T>(ref loop, systemToRemove);
        }
        
        static void HandleSubSystemLoopForRemoval<T>(ref PlayerLoopSystem loop, PlayerLoopSystem systemToRemove){
            
            // short-circuit if this is a lead node in the player loop tree.
            
            if(loop.subSystemList == null){
                return;
            }

            for(int i = 0; i < loop.subSystemList.Length; i++){
                RemoveSystem<T>(ref loop.subSystemList[i], systemToRemove);            
            }
        }


        ///
        /// Add a system from the player loop.
        ///


        public static bool InsertSystem<T>(ref PlayerLoopSystem loop, in PlayerLoopSystem systemToInsert, int index){
            
            if(loop.type != typeof(T)){
                return HandleSubSystemLoop<T>(ref loop, systemToInsert, index);
            }

            // get all of the sub systems of the current root system. 
            
            List<PlayerLoopSystem> playerLoopSystemList = new List<PlayerLoopSystem>();
            if(loop.subSystemList != null){
                playerLoopSystemList.AddRange(loop.subSystemList);
            }

            // insert the desired system and set the root systems sub system list.
            
            playerLoopSystemList.Insert(index, systemToInsert);
            loop.subSystemList = playerLoopSystemList.ToArray();

            return true;
        }

        static bool HandleSubSystemLoop<T>(ref PlayerLoopSystem loop, in PlayerLoopSystem systemToInsert, int index){
            
            // short-circuit if this a a leaf in the player loop tree.
            
            if(loop.subSystemList == null){
                return false;
            }

            // find the subsystem to insert the system into.
            
            for(int i = 0; i < loop.subSystemList.Length; i++){
                
                if(InsertSystem<T>(ref loop.subSystemList[i], in systemToInsert, index)==true){
                    // found system and inserted.
                    return true;
                }
                
                continue;
            }

            // there is no sub system of type T.
            
            return false;
        }


        /// 
        /// Debug.
        /// 


        public static void PrintPlayerLoop(PlayerLoopSystem playerLoop){
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[Unity Player Loop]");
            foreach(PlayerLoopSystem subSystem in playerLoop.subSystemList){
                PrintSubsystem(subSystem, sb, 0);
            }
            Debug.Log(sb.ToString());
        }

        static void PrintSubsystem(PlayerLoopSystem system, StringBuilder sb, int level){
            
            // indentation for string.
            sb.Append(' ', level * 2);
            
            // formatting.
            if(level==0){
                sb.Append("[");
            }
            else{
                sb.Append("* ");
            }


            // append the sub systems name.
            if(level==0){
                sb.Append(system.type.ToString());
                
                // formatting.
                sb.AppendLine("]");
            }
            else{
                sb.AppendLine(system.type.ToString());
            }

            // short-circuit if this is a leaf in the player loop tree.
            if(system.subSystemList == null || system.subSystemList.Length == 0){
                return;
            }

            // if there are more sub systems under this one.
            foreach(PlayerLoopSystem subSystem in system.subSystemList){
                PrintSubsystem(subSystem, sb, level + 1);
            }
        }
    }

}

// ==== Infomative Links: ====

// https://www.youtube.com/watch?v=ilvmOQtl57c&t=824s