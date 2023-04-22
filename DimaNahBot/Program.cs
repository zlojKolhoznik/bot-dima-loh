using DimaNahBot.Alarms;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DimaNahBot;

public delegate Task CommandHanlder(Message message, ITelegramBotClient botClient);

internal partial class Program
{
    private static Dictionary<long, int> _frequencies;
    private static Dictionary<string, CongratulationParams> _calendar;
    private static Dictionary<string, CommandHanlder> _commands;
    private static TelegramBotClient _botClient;
    private static User _botUser;
    private static List<Alarm> _alarms;
    private static string _dimaId;

    public static async Task Main()
    {
        using var cts = new CancellationTokenSource();
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };
        _botClient = new TelegramBotClient(System.Configuration.ConfigurationManager.AppSettings["DebugBotToken"]);
        _frequencies = IOTools.TryLoadFrequencies();
        _calendar = IOTools.TryReadCalendar();
        _dimaId = System.Configuration.ConfigurationManager.AppSettings["DimaId"];
        _commands = new Dictionary<string, CommandHanlder>()
        {
            {"/help", HandleHelpAsync},
            {"/f", HandleSetFrequencyAsync},
            {"/testTodaysHoliday", HandleTestTodaysHoliday}
        };
        _botUser = await _botClient.GetMeAsync(cts.Token);
        _alarms = new List<Alarm>();

        foreach (var item in _calendar)
        {
            var alarm = new YearlyAlarm(DateTime.Parse(item.Key), Congratulate, item.Value);
            _alarms.Add(alarm);
            alarm.Enable();
        }

        _botClient.StartReceiving(HandleUpdatesAsync, HandleErrorsAsync, receiverOptions, cts.Token);
        Console.WriteLine($"[{DateTime.Now}] Bot started");
        Console.ReadLine();
        cts.Cancel();
        Console.WriteLine($"[{DateTime.Now}] Bot stopped");
        IOTools.SaveFrequencies(_frequencies);
        Console.WriteLine($"[{DateTime.Now}] Settings saved");
    }

    private static async Task<object?> Congratulate(object? parameter)
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