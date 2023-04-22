using System.Timers;

namespace DimaNahBot.Alarms;

public class DailyAlarm : Alarm
{
    public DailyAlarm(DateTime target, AlarmCallback callback, object? callbackParam) : base(target, callback, callbackParam)
    {
        _timer.Interval = Minute;
    }

    protected override void Tick(object? sender, ElapsedEventArgs args)
    {
        var now = DateTime.Now;
        if (now.Hour == Target.Hour && now.Minute == Target.Minute)
        {
            _callback?.Invoke(_callbackParam);
        }
    }
}