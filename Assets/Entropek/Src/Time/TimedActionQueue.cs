using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Entropek.Time
{

    public class TimedActionQueue : MonoBehaviour
    {
        [Header(nameof(TimedActionQueue) + " Components")]
        [SerializeField] private OneShotTimer timer;
        private Queue<(Action, float)> queue = new Queue<(Action, float)>();
        public Queue<(Action, float)> Queue => queue;

        private void Awake()
        {
            LinkEvents();
        }

        private void OnDestroy()
        {
            UnlinkEvents();
        }

        public void Begin(float time)
        {
            timer.Begin(time);
        }

        public bool Begin()
        {
            if (queue.Count == 0)
            {
                return false;
            }

            timer.Begin(queue.Peek().Item2); // begin with the initial time of the next action.
            return true;
        }

        public void Halt()
        {
            timer.Halt();
        }

        public void Pause()
        {
            timer.Pause();
        }

        public void Resume()
        {
            timer.Resume();
        }

        public void Enqueue(Action action, float time = 0)
        {
            queue.Enqueue((action, time));
        }

        public void Clear()
        {
            queue.Clear();
        }

        private void LinkEvents()
        {
            LinkTimerEvents();
        }

        private void UnlinkEvents()
        {
            UnlinkTimerEvents();
        }

        private void LinkTimerEvents()
        {
            timer.Timeout += OnTimerTimeout;
        }

        private void UnlinkTimerEvents()
        {
            timer.Timeout -= OnTimerTimeout;
        }

        private void OnTimerTimeout()
        {
            // do nothing if there are no more actions in the queue.

            if (queue.Count == 0)
            {
                return;
            }

            (Action, float) queuedAction = queue.Dequeue();

            // invoke the next action in the queue.
            Action action = queuedAction.Item1;

            queuedAction.Item1();

            // restart the state timer with the new time.

            timer.Begin(queuedAction.Item2);
        }
    }

}

