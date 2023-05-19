using System.Timers;

namespace DimaNahBot.Alarms;

public class HourAlarm : Alarm
{
    private int _maxRings;
    private int _ringsEllapsed;
    
    public HourAlarm(DateTime target, AlarmCallback callback, object? callbackParam, int maxRings) : base(target, callback, callbackParam)
    {
        _maxRings = maxRings;
        _timer.Interval = Minute;
        _ringsEllapsed = 0;
        Console.WriteLine($"[{DateTime.UtcNow} UTC] Hour alarm for {target.Hour} hours has been launched");
    }

    protected override void Tick(object? sender, ElapsedEventArgs args)
    {
        var targetZoneTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _targetTimeZone);
        Console.WriteLine($"[{DateTime.UtcNow} UTC] Hour alarm for {Target.Hour} hours tick");
        if (targetZoneTime.Hour == Target.Hour)
        {
            _callback?.Invoke(_callbackParam);
            _ringsEllapsed++;
            Console.WriteLine($"[{DateTime.UtcNow} UTC] Hour alarm for {Target.Hour} hours has rung for {_ringsEllapsed} time");
            if (_ringsEllapsed >= _maxRings)
            {
                Disable();
                Console.WriteLine($"[{DateTime.UtcNow} UTC] Hour alarm for {Target.Hour} hours has been stopped");
            }
        }
    }
}