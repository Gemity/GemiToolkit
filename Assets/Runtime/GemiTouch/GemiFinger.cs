using Gemity.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gemity.GemiTouch
{
    public class GemiSnapshot : GemiPool<GemiSnapshot>
    {
        public float Age;
        public Vector2 ScreenPosition;
        public bool Interact;
    }

    public class GemiFinger : GemiPool<GemiFinger>
    {
        public int Id { get; internal set; }
        public float BirthTime { get; internal set; }
        public TouchPhase Phase { get; private set; }
        public Vector2 StartScreenPosition { get; internal set; }
        public Vector2 CurrentScreenPosition { get; internal set; }
        public Vector2 LastScreenPosition { get; internal set; }
        public bool StartOverGui { get; internal set; }

        /// <summary>
        /// Store the time interval for each phase. It will be reset when the phase changes
        /// </summary>
        internal float Timer { get; set; }
        public float Age => Time.time - BirthTime;

        private List<GemiSnapshot> _snapshots = new();
        public IReadOnlyList<GemiSnapshot> Snapshots => _snapshots;

        public void BeganFinger(Vector2 screenPosition)
        {
            BirthTime = Time.time;
            StartScreenPosition = screenPosition;
            CurrentScreenPosition = screenPosition;
            LastScreenPosition = screenPosition;
            StartOverGui = GemiTouch.PointOverGui(screenPosition);
            Phase = TouchPhase.Began;
            Timer = 0;
        }

        internal GemiSnapshot TakeSnapshot()
        {
            GemiSnapshot snapshot = GemiSnapshot.Pop();
            snapshot.ScreenPosition = CurrentScreenPosition;
            snapshot.Age = Time.time - BirthTime;
            _snapshots.Add(snapshot);

            return snapshot;
        }

        internal void ChangePhase(TouchPhase phase)
        {
            if(Phase != phase)
            {
                Timer = 0;
                Phase = phase;
            }
        }

        internal void Release()
        {
            _snapshots.ForEach(x => GemiSnapshot.Push(x));
            _snapshots.Clear();

            Push(this); 
        }
    }
}
