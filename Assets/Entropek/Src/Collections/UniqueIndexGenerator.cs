using System.Collections.Generic;
using UnityEngine;

namespace Entropek.Collections{

    public class UniqueIdGenerator{
    
        private uint largestGeneratedId = 0;

        private Stack<uint> availableIndexes = new Stack<uint>();

        public uint GenerateId(){
            if(availableIndexes.Count==0){

                // create a new index if there are non to be re-used.

                return ++largestGeneratedId;
            }
            else{

                // re-use indexes that are made.

                return availableIndexes.Pop();
            }
        }

        public void FreeId(uint index){

            // add the free index for later re-use.

            availableIndexes.Push(index);
        }

        public void Clear(){
            availableIndexes.Clear();
            largestGeneratedId = 0;
        }
    }
}
