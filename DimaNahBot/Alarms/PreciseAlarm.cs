using System.Timers;

namespace DimaNahBot.Alarms;

public class PreciseAlarm : Alarm
{
    public PreciseAlarm(DateTime target, AlarmCallback callback, object? callbackParam) : base(target, callback, callbackParam)
    {
        _timer.Interval = Second;
    }

    protected override void Tick(object? sender, ElapsedEventArgs args)
    {
        var now = DateTime.Now;
        if (now.Hour == Target.Hour && now.Minute == Target.Minute && now.Second == Target.Second)
        {
            _callback?.Invoke(_callbackParam);
            Console.WriteLine($"[{DateTime.Today.ToShortDateString()}] alarm has rang");
        }
        Console.WriteLine($"[{DateTime.Today.ToShortDateString()}] alarm has not rang");
    }
}