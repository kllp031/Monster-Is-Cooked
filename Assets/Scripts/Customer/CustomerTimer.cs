using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CustomerTimer : MonoBehaviour
{
    [Header("TimerSettings")]
    [SerializeField] List<TimerPoint> timerPoints = new();
    [SerializeField] Slider timerSlider;
    [Header("Events")]
    [SerializeField] UnityEvent<Customer.Mood> receiveMoodStatus;
    [SerializeField] UnityEvent receiveTimeUpStatus;
    private bool isRunning = false;
    private float previousCounterValue = 0f;
    private float timer = 0f;
    private float counter = 0f;
    private void OnEnable()
    {
        timer = 0;
        previousCounterValue = 0f;
    }
    public void SetTimer(float time)
    {
        if (time < 0) return;
        timer = time;
    }
    public void StopTimer()
    {
        isRunning = false;
    }
    public void ResetCounter()
    {
        previousCounterValue = timer;
        counter = timer;
    }
    public void StartTimer()
    {
        isRunning = true;
    }
    private void Update()
    {
        if (!isRunning) return;
        if (counter > 0f)
        {
            previousCounterValue = counter;
            counter -= Time.deltaTime;
            if (timerSlider != null)
            {
                timerSlider.value = counter / timer;
            }

            // Check if timer has exceeded any mood change points -> announce all listeners
            foreach (var point in timerPoints)
            {
                if (previousCounterValue / timer >= point.TimePoint && counter / timer < point.TimePoint)
                {
                    receiveMoodStatus?.Invoke(point.Mood);
                    break;
                }
            }

            // Check if timer has run out -> announce all listeners
            if (counter <= 0f)
            {
                counter = 0f;
                receiveTimeUpStatus?.Invoke();
                StopTimer();
            }
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
