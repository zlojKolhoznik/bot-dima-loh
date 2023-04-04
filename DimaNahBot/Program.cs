using System.Net;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

var botClient = new TelegramBotClient(System.Configuration.ConfigurationManager.AppSettings["BotToken"]);
using var cts = new CancellationTokenSource();
var receiverOptions = new ReceiverOptions
{
    AllowedUpdates = Array.Empty<UpdateType>()
};
var frequencies = TryLoadFrequencies();
var dimaId = long.Parse(System.Configuration.ConfigurationManager.AppSettings["DimaId"]);

botClient.StartReceiving(HandleUpdatesAsync, HandleErrorsAsync, receiverOptions, cts.Token);
Console.WriteLine($"[{DateTime.Now}] Bot started");
Console.ReadLine();
cts.Cancel();
Console.WriteLine($"[{DateTime.Now}] Bot stopped");
SaveFrequencies(frequencies);
Console.WriteLine($"[{DateTime.Now}] Settings saved");

Task HandleErrorsAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    botClient.DeleteWebhookAsync(true, cts.Token).Wait();
    var updates = botClient.GetUpdatesAsync().Result;
    if (updates.Length > 0)
    {
    }
    Console.WriteLine($"[{DateTime.Now}] Error: {exception.Message}");
    return Task.CompletedTask;
}

async Task HandleUpdatesAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
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

    if (message.Text!.StartsWith("/f"))
    {
        await HandleSetFrequency(message, frequencies, botClient);
        return;
    }

    if (message.Text.StartsWith("/help"))
    {
        await HandleHelp(message, botClient);
        return;
    }

    if (!frequencies.TryGetValue(message.Chat.Id, out int frequency))
    {
        frequency = 100;
        frequencies.Add(message.Chat.Id, frequency);
    }

    int rnd = new Random().Next(0, frequency);
    if (rnd == 0)
    {
        await botClient.SendTextMessageAsync(message.Chat.Id, $"[діма](tg://user?id={dimaId}) іди нахуй", parseMode: ParseMode.MarkdownV2);
    }
}

async Task HandleSetFrequency(Message message1, Dictionary<long, int> dictionary, ITelegramBotClient telegramBotClient)
{
    if (int.TryParse(message1.Text[2..], out int newFrequency) && newFrequency > 0)
    {
        if (!dictionary.TryAdd(message1.Chat.Id, newFrequency))
        {
            dictionary[message1.Chat.Id] = newFrequency;
        }

        await telegramBotClient.SendTextMessageAsync(message1.Chat.Id, $"Нова частота: {newFrequency}");
    }
    else
    {
        await telegramBotClient.SendTextMessageAsync(message1.Chat.Id, "Невірний формат");
    }
}

async Task HandleHelp(Message message, ITelegramBotClient botClient)
{
    await botClient.SendTextMessageAsync(
        chatId: message.Chat.Id,
        text: "діма нахуй бот\n\n" +
              "Для зміни частоти введіть /f \\<частота\\> \n `**частота \\- раз на скільки повідомлень я посилатиму діму нахуй \\(100 за замовчуванням\\)**`\n\n" +
              "Для перегляду допомоги введіть /help",
        parseMode: ParseMode.MarkdownV2
    );
}

static Dictionary<long, int> TryLoadFrequencies()
{
    try
    {
        return JsonConvert.DeserializeObject<Dictionary<long, int>>(System.IO.File.ReadAllText("frequencies.json"));
    }
    catch (Exception)
    {
        return new Dictionary<long, int>();
    }
}

static void SaveFrequencies(Dictionary<long, int> frequencies)
{
    System.IO.File.WriteAllText("frequencies.json", JsonConvert.SerializeObject(frequencies, Formatting.Indented));
}