using System.Timers;

namespace DimaNahBot.Alarms;

public class DailyAlarm : Alarm
{
    public DailyAlarm(DateTime target, AlarmCallback callback) : base(target, callback)
    {
        _timer.Interval = Minute;
    }

    protected override void Tick(object? sender, ElapsedEventArgs args)
    {
        var now = DateTime.Now;
        if (now.Hour == Target.Hour && now.Minute == Target.Minute)
        {
            _callback?.Invoke(null);
        }
    }
}