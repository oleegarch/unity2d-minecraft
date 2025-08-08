using System;
using UnityEngine;

namespace World
{
    public class WorldTime : MonoBehaviour
    {
        [SerializeField] private float _oneDayDuration;

        public float DayDuration
        {
            get => _oneDayDuration;
            set => _oneDayDuration = value;
        }

        public float TotalPassedSecs { get; private set; }
        public float DayRatio { get; private set; }

        public event Action<float> OnTimeChanged;

        private void Update()
        {
            float passedTimeSecs = Time.deltaTime;

            TotalPassedSecs += passedTimeSecs;
            DayRatio += passedTimeSecs / _oneDayDuration;
            if (DayRatio > 1f) DayRatio -= 1f;

            OnTimeChanged?.Invoke(DayRatio);
        }
    }
}