using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CustomerTimerOnline : NetworkBehaviour
{
    [Header("TimerSettings")]
    [SerializeField] List<TimerPoint> timerPoints = new();
    [SerializeField] Slider timerSlider;
    [Header("Events")]
    [SerializeField] UnityEvent<Customer.Mood> receiveMoodStatus;
    [SerializeField] UnityEvent receiveTimeUpStatus;

    [Networked] bool IsRunning { get; set; }  
    [Networked] float PreviousCounterValue { get; set; }
    [Networked] TickTimer TickTimer { get; set; }
    [Networked] private float CountingTime { get; set; }
    //[Networked] private float StartTick { get; set; }

    //[Networked] float Counter { get; set; }
    public override void Spawned()
    {
        ResetTimer();
    }
    public void SetTimer(float time)
    {
        if (time < 0) return;
        CountingTime = time;
        Debug.Log("Set timer: " + CountingTime + " seconds");
    }
    public void StopTimer()
    {
        IsRunning = false;
    }
    public void ResetTimer()
    {
        if (Runner != null && Runner.IsSharedModeMasterClient)
        {
            IsRunning = false;
            PreviousCounterValue = 0f;
            TickTimer = TickTimer.None;
        }
    }
    public void StartTimer()
    {
        if (Runner == null || !Runner.IsSharedModeMasterClient) return;

        IsRunning = true;
        PreviousCounterValue = CountingTime;
        TickTimer = TickTimer.CreateFromSeconds(Runner, CountingTime);
        Debug.Log("Start counting: " + CountingTime + " seconds");
        Debug.Log("Start timer: " + TickTimer.RemainingTime(Runner) + " seconds remaining");
    }
    private void Update()
    {
        if (!IsRunning || Runner == null || !Runner.IsSharedModeMasterClient) return;

        if (TickTimer.Expired(Runner))
        {
            Debug.Log("Timer expired!");
            TickTimer = TickTimer.None;
            receiveTimeUpStatus?.Invoke();
            StopTimer();
            return;
        }

        if (TickTimer.IsRunning)
        {
            if (timerSlider != null)
            {
                timerSlider.value = (float)TickTimer.RemainingTime(Runner) / CountingTime;
            }

            // Check if timer has exceeded any mood change points -> announce all listeners
            foreach (var point in timerPoints)
            {
                if (PreviousCounterValue / CountingTime >= point.TimePoint && (float)TickTimer.RemainingTime(Runner) / CountingTime < point.TimePoint)
                {
                    receiveMoodStatus?.Invoke(point.Mood);
                    break;
                }
            }

            PreviousCounterValue = (float)TickTimer.RemainingTime(Runner);
        }
    }

    [Serializable]
    public class TimerPoint
    {
        [SerializeField] Customer.Mood mood = Customer.Mood.Embarrassed;
        [Range(0f, 1f)]
        [SerializeField] float timePoint;

        public Customer.Mood Mood { get => mood; }
        public float TimePoint { get => timePoint; }
    }
}
