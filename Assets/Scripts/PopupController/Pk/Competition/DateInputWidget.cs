using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DateInputWidget : MonoBehaviour
{
    public class ValueChanged : UnityEvent<DateTime> { }

    public ValueChanged m_onValueChanged;
    public InputField m_yearInput;
    public InputField m_monthInput;
    public InputField m_dayInput;
    public InputField m_hourInput;
    public InputField m_minInput;

    private bool m_endEdit;

    void Start()
    {
        m_yearInput.onValueChanged.AddListener(OnYearChanged);
        m_yearInput.onEndEdit.AddListener(EndEdit(m_yearInput, 4, DateTime.Now.Year));

        m_monthInput.onValueChanged.AddListener(OnMonthChanged);
        m_monthInput.onEndEdit.AddListener(EndEdit(m_monthInput, 2, DateTime.Now.Month));
        
        m_dayInput.onValueChanged.AddListener(OnDayChanged);
        m_dayInput.onEndEdit.AddListener(EndEdit(m_dayInput, 2, DateTime.Now.Day));

        m_hourInput.onValueChanged.AddListener(ClampValue(m_hourInput, 23));
        m_hourInput.onEndEdit.AddListener(EndEdit(m_hourInput, 2, 0));

        m_minInput.onValueChanged.AddListener(ClampValue(m_minInput, 59));
        m_minInput.onEndEdit.AddListener(EndEdit(m_minInput, 2, 0));
    }

    private UnityAction<string> EndEdit(InputField input, int digits, int defaultValue)
    {
        return delegate {
            m_endEdit = true;
            Set(input, input.text != "" ? int.Parse(input.text) : defaultValue, digits);
        };
    }

    private void Set(InputField input, int value, int digits)
    {
        input.text = value.ToString().PadLeft(digits, '0');
    }

    private int year
    {
        get { return int.Parse(m_yearInput.text); }
        set { m_yearInput.text = value.ToString(); }
    }

    private int month
    {
        get { return int.Parse(m_monthInput.text); }
        set { m_monthInput.text = value.ToString(); }
    }

    private int day
    {
        get { return int.Parse(m_dayInput.text); }
        set { m_dayInput.text = value.ToString(); }
    }

    private int hour
    {
        get { return int.Parse(m_hourInput.text); }
        set { m_hourInput.text = value.ToString(); }
    }

    private int min
    {
        get { return int.Parse(m_minInput.text); }
        set { m_minInput.text = value.ToString(); }
    }

    private void OnYearChanged(string text)
    {
        // if true, then year must be valid
        if (m_endEdit || text == "") { return; }

        year = Mathf.Clamp(int.Parse(text), DateTime.MinValue.Year, DateTime.MaxValue.Year);
        ValidateDay();
    }

    private void OnMonthChanged(string text)
    {
        if (m_endEdit || text == "") { return; }

        month = Mathf.Clamp(int.Parse(text), 1, 12);
        ValidateDay();
    }

    private void OnDayChanged(string text)
    {
        if (m_endEdit || text == "") { return; }

        ValidateDay();
    }

    private void ValidateDay()
    {
        var days = DateTime.DaysInMonth(year, month);
        day = Mathf.Clamp(day, 1, days);
    }

    private UnityAction<string> ClampValue(InputField input, int maxValue)
    {
        return delegate {
            if (m_endEdit || input.text == "") { return; }

            input.text = Mathf.Clamp(int.Parse(input.text), 0, maxValue).ToString(); 
        };
    }

    private void LateUpdate()
    {
        m_endEdit = false;
    }

    public DateTime utcDate
    {
        get
        {
            var localTime = new DateTime(year, month, day, hour, min, 0, DateTimeKind.Local);
            return localTime.ToUniversalTime();
        }
        set
        {
            var localTime = value.ToLocalTime();
            Set(m_yearInput, localTime.Year, 4);
            Set(m_monthInput, localTime.Month, 2);
            Set(m_dayInput, localTime.Day, 2);
            Set(m_hourInput, localTime.Hour, 2);
            Set(m_minInput, localTime.Minute, 2);
        }
    }
}
