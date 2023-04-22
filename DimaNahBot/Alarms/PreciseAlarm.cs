using System.Timers;

namespace DimaNahBot.Alarms;

public class PreciseAlarm : Alarm
{
    public PreciseAlarm(DateTime target, AlarmCallback callback) : base(target, callback)
    {
        _timer.Interval = Second;
    }

    protected override void Tick(object? sender, ElapsedEventArgs args)
    {
        var now = DateTime.Now;
        if (now.Hour == Target.Hour && now.Minute == Target.Minute && now.Second == Target.Second)
        {
            _callback?.Invoke(null);
        }
    }
}