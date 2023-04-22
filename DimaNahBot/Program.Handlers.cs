using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DimaNahBot;

partial class Program
{
    private static async Task HandleTestTodaysHoliday(Message message, ITelegramBotClient botClient)
    {
        if (!_calendar.TryGetValue(DateTime.Now.ToString("dd.MM"), out var @params))
        {
            await botClient.SendTextMessageAsync(message.Chat.Id, "Сьогодні нема жодного комуністичного свята :(");
            return;
        }

        var chatId = long.Parse(System.Configuration.ConfigurationManager.AppSettings["GroupId"]);
        // var chatId = -1001622188493;
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

        if (message.Text.StartsWith("/"))
        {
            if (_commands.TryGetValue(message.Text.Split(' ')[0], out var hanlder))
            {
                await hanlder(message, botClient);
            }
            else
            {
                await botClient.SendTextMessageAsync(message.Chat.Id,
                    $"Невідома команда. Щоб отримати допомогу, напишіть /help або /help@{_botUser.Username}, якщо ви у групі",
                    cancellationToken: cancellationToken);
            }
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
}