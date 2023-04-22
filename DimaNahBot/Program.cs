using DimaNahBot.Alarms;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DimaNahBot;

public delegate Task CommandHanlder(Message message, ITelegramBotClient botClient);

internal class Program
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

    private static async Task HandleTestTodaysHoliday(Message message, ITelegramBotClient botClient)
    {
        if (!_calendar.TryGetValue(DateTime.Now.ToString("dd.MM"), out var @params))
        {
            await botClient.SendTextMessageAsync(message.Chat.Id, "Сьогодні нема жодного комуністичного свята :(");
            return;
        }

        var chatId = message.Chat.Id;

        if (!string.IsNullOrEmpty(@params.GifId))
        {
            await _botClient.SendAnimationAsync(chatId, new InputFileId(@params.GifId));
        }

        await _botClient.SendTextMessageAsync(chatId, @params.Text,
            parseMode: ParseMode.MarkdownV2);
    }

    private static async Task HandleErrorsAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        await botClient.DeleteWebhookAsync(true, cancellationToken);
        Console.WriteLine($"[{DateTime.Now}] Error: {exception.Message}");
    }

    private static async Task HandleUpdatesAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Type != UpdateType.Message)
        {
            return;
        }

        var message = update.Message!;
        if (message.Type != MessageType.Text)
        {
            return;
        }

        if (message.Text.StartsWith("/") && _commands.TryGetValue(message.Text.Split(' ')[0], out var hanlder))
        {
            await hanlder(message, botClient);
        }
        else
        {
            await botClient.SendTextMessageAsync(message.Chat.Id, 
                $"Невідома команда. Щоб отримати допомогу, напишіть /help або /help@{_botUser.Username}, якщо ви у групі", 
                cancellationToken: cancellationToken);
        }

        if (!_frequencies.TryGetValue(message.Chat.Id, out var frequency))
        {
            frequency = 100;
            _frequencies.Add(message.Chat.Id, frequency);
        }

        int rnd = new Random().Next(0, frequency);
        if (rnd == 0)
        {
            await botClient.SendTextMessageAsync(message.Chat.Id, $"[діма](tg://user?id={_dimaId}) іди нахуй",
                parseMode: ParseMode.MarkdownV2);
        }
    }

    private static async Task HandleSetFrequencyAsync(Message message, ITelegramBotClient telegramBotClient)
    {
        if (int.TryParse(message.Text[2..], out int newFrequency) && newFrequency > 0)
        {
            if (!_frequencies.TryAdd(message.Chat.Id, newFrequency))
            {
                _frequencies[message.Chat.Id] = newFrequency;
            }

            await telegramBotClient.SendTextMessageAsync(message.Chat.Id, $"Нова частота: {newFrequency}");
        }
        else
        {
            await telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Невірний формат");
        }
    }

    private static async Task HandleHelpAsync(Message message, ITelegramBotClient botClient)
    {
        await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "діма нахуй бот\n\n" +
                  "Для зміни частоти введіть /f \\<частота\\> \n `**частота \\- раз на скільки повідомлень я посилатиму діму нахуй \\(100 за замовчуванням\\)**`\n\n" +
                  "Для перегляду допомоги введіть /help",
            parseMode: ParseMode.MarkdownV2
        );
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