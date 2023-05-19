using System.Timers;

namespace DimaNahBot.Alarms;

public class YearlyAlarm : Alarm
{
    public YearlyAlarm(DateTime target, AlarmCallback callback, object? callbackParam) : base(target, callback, callbackParam)
    {
        _timer.Interval = 24 * Hour;
        Console.WriteLine($"[{DateTime.UtcNow} UTC] alarm for {target.ToShortDateString()} hours has been launched");
    }

    protected override void Tick(object? sender, ElapsedEventArgs args)
    {
        var targetZoneTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _targetTimeZone);
        if (targetZoneTime.Day == Target.Day && targetZoneTime.Month == Target.Month)
        {
            Console.WriteLine($"[{DateTime.UtcNow} UTC] alarm for {Target.ToShortDateString()} has rang");
            _callback?.Invoke(_callbackParam);
            return;
        }
        Console.WriteLine($"[{DateTime.UtcNow} UTC] alarm for {Target.ToShortDateString()} has not rang");
    }
}