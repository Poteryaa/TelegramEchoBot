using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var token = Environment.GetEnvironmentVariable("TELEGRAM_TOKEN");

if (string.IsNullOrEmpty(token))
{
    Console.WriteLine("TELEGRAM_TOKEN не найден!");
    return;
}

var botClient = new TelegramBotClient(token);

// Создаем веб-приложение для Render
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Health check endpoints для Render
app.MapGet("/", () => "Telegram Echo Bot работает!");
app.MapGet("/health", () => "OK");

// Запускаем бота в фоновом режиме
_ = Task.Run(async () =>
{
    using var cts = new CancellationTokenSource();

    botClient.StartReceiving(
        new DefaultUpdateHandler(HandleUpdateAsync, HandleErrorAsync),
        cancellationToken: cts.Token
    );

    var me = await botClient.GetMe();
    Console.WriteLine($"Бот запущен: @{me.Username}");

    // Держим бота активным
    try
    {
        await Task.Delay(-1, cts.Token);
    }
    catch (TaskCanceledException)
    {
        Console.WriteLine("Бот остановлен");
    }
});

// Получаем порт от Render (обязательно!)
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Urls.Add($"http://0.0.0.0:{port}");

Console.WriteLine($"Веб-сервер запускается на порту {port}");
await app.RunAsync();

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