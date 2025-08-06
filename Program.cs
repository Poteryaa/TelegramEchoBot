using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
using DotNetEnv;

Env.Load();

var token = Environment.GetEnvironmentVariable("TELEGRAM_TOKEN");
var botClient = new TelegramBotClient(token!);

using var cts = new CancellationTokenSource();

botClient.StartReceiving(
    new DefaultUpdateHandler(HandleUpdateAsync, HandleErrorAsync),
    cancellationToken: cts.Token
);

var me = await botClient.GetMe();
Console.WriteLine($"Бот запущен: @{me.Username}");
Console.ReadLine();

static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    if (update.Message is not { Text: { } messageText }) return;

    var chatId = update.Message.Chat.Id;

    Console.WriteLine($"Получено сообщение от {chatId}: {messageText}");

    await botClient.SendMessage(
        chatId: chatId,
        text: $"Вы написали: {messageText}",
        cancellationToken: cancellationToken
    );
}

static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var errorMessage = exception switch
    {
        ApiRequestException apiEx => $"Ошибка Telegram API:\n[{apiEx.ErrorCode}] {apiEx.Message}",
        _ => exception.ToString()
    };

    Console.WriteLine(errorMessage);
    return Task.CompletedTask;
}
