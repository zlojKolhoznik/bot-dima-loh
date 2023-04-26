using System.Timers;

namespace DimaNahBot.Alarms;

public delegate object? AlarmCallback(object? parameter);

public abstract class Alarm
{
    protected readonly AlarmCallback? _callback;
    protected readonly System.Timers.Timer _timer;
    protected readonly object? _callbackParam;
    protected readonly TimeZoneInfo _targetTimeZone;
    protected const int Second = 1000;
    protected const int Minute = 60 * Second;
    protected const int Hour = 60 * Minute;

    protected Alarm(DateTime target, AlarmCallback callback, object? callbackParam)
    {
        _callback = callback;
        _callbackParam = callbackParam;
        _targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time");
        Target = target;
        _timer = new System.Timers.Timer();
        _timer.Elapsed += Tick;
    }
    
    public DateTime Target { get; set; }

    public void Enable()
    {
        _timer.Start();
    }

    public void Disable()
    {
        _timer.Stop();
    }

    protected abstract void Tick(object? sender, ElapsedEventArgs args);
}