using System.Globalization;
using DimaNahBot.Alarms;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DimaNahBot;

public delegate Task CommandHanlder(Message message, ITelegramBotClient botClient);

internal partial class Program
{
    private static Dictionary<long, int> _frequencies = null!;
    private static Dictionary<string, CongratulationParams> _calendar = null!;
    private static Dictionary<string, CommandHanlder> _commands = null!;
    private static TelegramBotClient _botClient = null!;
    private static List<Alarm> _alarms = null!;
    private static string _dimaId = null!;

    private static void Main()
    {
        using var cts = new CancellationTokenSource();
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };
        _botClient = new TelegramBotClient(System.Configuration.ConfigurationManager.AppSettings["DebugBotToken"]!);
        _frequencies = IO.TryLoadFrequencies();
        _calendar = IO.TryReadCalendar();
        _dimaId = System.Configuration.ConfigurationManager.AppSettings["DimaId"]!;
        _commands = new Dictionary<string, CommandHanlder>()
        {
            { "/help", HelpHanlderAsync },
            { "/f", SetFrequencyHanlderAsync },
            { "/testTodaysHoliday", TestTodaysHolidayHandlerAsync }
        };
        _alarms = new List<Alarm>();

        foreach (var item in _calendar)
        {
            var alarm = new YearlyAlarm(new DateTime(DateTime.Now.Year, int.Parse(item.Key[3..]), int.Parse(item.Key[..2])), SendCongratulation, item.Value);
            _alarms.Add(alarm);
            alarm.Enable();
        }

        _botClient.StartReceiving(UpdatesHanlderAsync, ErrorsHandlerAsync, receiverOptions, cts.Token);
        Console.WriteLine($"[{DateTime.Now}] Bot started");
        Console.ReadLine();
        cts.Cancel();
        Console.WriteLine($"[{DateTime.Now}] Bot stopped");
        IO.SaveFrequencies(_frequencies);
        Console.WriteLine($"[{DateTime.Now}] Settings saved");
    }

    private static async Task<object?> SendCongratulation(object? parameter)
    {
        if (parameter is not CongratulationParams @params)
        {
            return null;
        }

        string chatId = System.Configuration.ConfigurationManager.AppSettings["GroupId"]!;

        if (!string.IsNullOrEmpty(@params.GifId))
        {
            await _botClient.SendAnimationAsync(chatId, new InputFileId(@params.GifId));
        }

        await _botClient.SendTextMessageAsync(chatId, @params.Text,
            parseMode: ParseMode.MarkdownV2);

        return null;
    }
}