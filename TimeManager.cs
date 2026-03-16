using System.Collections;
using TMPro;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public enum Day
    {
        Monday,
        Tuesday,
        Wednesday,
        Thursday,
        Friday,
        Saturday,
        Sunday
    }

    [Header("References")]
    public TextMeshProUGUI dayOfWeekText;
    public TextMeshProUGUI timeText;

    [Header("Time")]
    private float oneDayTime = 600f;
    public float dayTimer;
    public string currentTime;
    public Day currentDay;

    private void Start()
    {
        dayTimer = 0;
    }

    private void Update()
    {
        if (dayTimer >= oneDayTime)
        {
            DayPassed();
            dayTimer = 0;
        }
        else
        {
            dayTimer += Time.deltaTime;
        }

        int currentHour = Mathf.FloorToInt(dayTimer / 25);
        int currentMinute = Mathf.FloorToInt((dayTimer % 25) / 25 * 60);
        currentMinute = Mathf.FloorToInt(currentMinute / 10f) * 10;
        currentTime = currentHour.ToString("00") + ":" + currentMinute.ToString("00");

        dayOfWeekText.text = currentDay.ToString();
        timeText.text = currentTime;
    }

    public void DayPassed()
    {
        int nextDay = (int)currentDay + 1;

        if (nextDay > (int)Day.Sunday)
        {
            currentDay = Day.Monday;
        }
        else
        {
            currentDay = (Day)nextDay;
        }
    }

    public void WakeUp(int hour = 6)
    {
        DayPassed();
        dayTimer = hour * 25f;
    }
}
