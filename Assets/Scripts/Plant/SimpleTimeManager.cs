// SimpleTimeManager.cs - Basic time system for garden
using UnityEngine;
using System;

public class SimpleTimeManager : MonoBehaviour
{
    [Header("Time Settings")]
    [SerializeField] private float timeScale = 1f; // 1 = real time, 60 = 1 minute = 1 hour
    [SerializeField] private bool pauseWhenUIOpen = true;

    [Header("Starting Time")]
    [SerializeField] private int startHour = 6;
    [SerializeField] private int startMinute = 0;

    // Current game time
    private DateTime currentGameTime;
    private float realTimeAccumulator = 0f;

    // Events
    public static event Action<DateTime> OnTimeChanged;
    public static event Action<int> OnHourChanged; // New hour started
    public static event Action<int> OnDayChanged; // New day started

    public static SimpleTimeManager Instance { get; private set; }

    // Properties
    public DateTime CurrentTime => currentGameTime;
    public float TimeScale => timeScale;
    public int CurrentHour => currentGameTime.Hour;
    public int CurrentMinute => currentGameTime.Minute;
    public int CurrentDay => currentGameTime.DayOfYear;
    public string TimeString => currentGameTime.ToString("HH:mm");
    public string DateString => currentGameTime.ToString("MMM dd");

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeTime();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeTime()
    {
        // Start at day 1, specified hour
        currentGameTime = new DateTime(2023, 1, 1, startHour, startMinute, 0);
        realTimeAccumulator = 0f;
    }

    private void Update()
    {
        // Check if time should be paused
        if (pauseWhenUIOpen && UIManager.Instance != null && !UIManager.Instance.IsGameplayUI)
        {
            return; // Don't advance time when menus are open
        }

        AdvanceTime();
    }

    private void AdvanceTime()
    {
        // Accumulate real time
        realTimeAccumulator += Time.deltaTime * timeScale;

        // Convert to game time (1 real second = 1 game minute with default scale)
        if (realTimeAccumulator >= 60f) // 60 real seconds = 1 game hour
        {
            int hoursToAdd = Mathf.FloorToInt(realTimeAccumulator / 60f);
            realTimeAccumulator %= 60f;

            DateTime previousTime = currentGameTime;
            currentGameTime = currentGameTime.AddHours(hoursToAdd);

            // Fire events
            OnTimeChanged?.Invoke(currentGameTime);

            // Check for hour change
            if (previousTime.Hour != currentGameTime.Hour)
            {
                OnHourChanged?.Invoke(currentGameTime.Hour);
            }

            // Check for day change
            if (previousTime.DayOfYear != currentGameTime.DayOfYear)
            {
                OnDayChanged?.Invoke(currentGameTime.DayOfYear);
            }
        }
    }

    // Utility methods
    public float GetHoursPassedSince(DateTime startTime)
    {
        TimeSpan timeDiff = currentGameTime - startTime;
        return (float)timeDiff.TotalHours;
    }

    public float GetRealSecondsPassedSince(DateTime startTime)
    {
        float gameHours = GetHoursPassedSince(startTime);
        return gameHours * 60f / timeScale; // Convert back to real seconds
    }

    public void SetTimeScale(float newScale)
    {
        timeScale = Mathf.Max(0.1f, newScale); // Minimum 0.1x speed
    }

    public void AdvanceTimeBy(int hours)
    {
        currentGameTime = currentGameTime.AddHours(hours);
        OnTimeChanged?.Invoke(currentGameTime);
    }

    // Fast forward time (useful for testing)
    [ContextMenu("Advance 1 Hour")]
    public void AdvanceOneHour()
    {
        AdvanceTimeBy(1);
    }

    [ContextMenu("Advance 1 Day")]
    public void AdvanceOneDay()
    {
        AdvanceTimeBy(24);
    }

    // Save/Load support
    [System.Serializable]
    public class TimeData
    {
        public string dateTime;
        public float accumulator;
    }

    public TimeData GetSaveData()
    {
        return new TimeData
        {
            dateTime = currentGameTime.ToBinary().ToString(),
            accumulator = realTimeAccumulator
        };
    }

    public void LoadSaveData(TimeData data)
    {
        try
        {
            long binaryTime = Convert.ToInt64(data.dateTime);
            currentGameTime = DateTime.FromBinary(binaryTime);
            realTimeAccumulator = data.accumulator;

            OnTimeChanged?.Invoke(currentGameTime);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Failed to load time data: {e.Message}");
            InitializeTime(); // Reset to default
        }
    }
}