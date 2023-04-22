using System.Timers;

namespace DimaNahBot.Alarms;

public class YearlyAlarm : Alarm
{
    public YearlyAlarm(DateTime target, AlarmCallback callback, object? callbackParam) : base(target, callback, callbackParam)
    {
        _timer.Interval = 24 * Hour;
    }

    protected override void Tick(object? sender, ElapsedEventArgs args)
    {
        var now = DateTime.Now;
        if (now.Day == Target.Day && now.Month == Target.Month)
        {
            _callback?.Invoke(_callbackParam);
        }
    }
}