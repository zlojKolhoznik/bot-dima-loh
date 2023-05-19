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
    private static Dictionary<string, CongratulationParameters> _calendar = null!;
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
        _botClient = new TelegramBotClient(System.Configuration.ConfigurationManager.AppSettings["BotToken"]!);
        _frequencies = IO.TryLoadFrequencies();
        _calendar = IO.TryReadCalendar();
        _dimaId = System.Configuration.ConfigurationManager.AppSettings["DimaId"]!;
        _commands = new Dictionary<string, CommandHanlder>()
        {
            { "/help", HelpHanlderAsync },
            { "/f", SetFrequencyHanlderAsync },
            { "/testTodaysHoliday", TestTodaysHolidayHandlerAsync },
            { "/sendTestGif", SendGifHandlerAsync }
        };
        _alarms = new List<Alarm>();
        var calendarActivator = new HourAlarm(new DateTime(DateTime.Now.Year, 1, 1, 0, 0, 0), ActivateAlarms, null, 1);
        calendarActivator.Enable();
        _alarms.Add(calendarActivator);

        _botClient.StartReceiving(UpdatesHanlderAsync, ErrorsHandlerAsync, receiverOptions, cts.Token);
        Console.WriteLine($"[{DateTime.UtcNow} UTC] Bot started");
        Console.ReadLine();
        cts.Cancel();
        Console.WriteLine($"[{DateTime.UtcNow} UTC] Bot stopped");
        IO.SaveFrequencies(_frequencies);
        Console.WriteLine($"[{DateTime.UtcNow} UTC] Settings saved");
    }

    private static object? ActivateAlarms(object? param)
    {
        foreach (var item in _calendar)
        {
            var alarm = new YearlyAlarm(new DateTime(DateTime.Now.Year, int.Parse(item.Key[3..]), int.Parse(item.Key[..2])), SendCongratulation, item.Value);
            _alarms.Add(alarm);
            alarm.Enable();
        }

        return null;
    }

    private static async Task<object?> SendCongratulation(object? param)
    {
        if (param is not CongratulationParameters parameters)
        {
            return null;
        }

        var chatId = long.Parse(System.Configuration.ConfigurationManager.AppSettings["GroupId"]!);

        if (!string.IsNullOrEmpty(parameters.GifUrl))
        {
            try
            {
                await _botClient.SendAnimationAsync(chatId, new InputFileUrl(parameters.GifUrl));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        
        await _botClient.SendTextMessageAsync(chatId, parameters.Text,
            parseMode: ParseMode.MarkdownV2);
        
        return null;
    }
}