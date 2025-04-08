using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Polling;
using TelegramBot.Models;

var builder = WebApplication.CreateBuilder(args);

var botToken = builder.Configuration.GetSection("BotToken").Get<string>();
var address = builder.Configuration.GetSection("HostAddress").Get<string>();
var connectionString = builder.Configuration.GetSection("ConnectionString").Get<string>();

// Создайте экземпляр клиента Telegram Bot.
var botClient = new TelegramBotClient(botToken);

// Установите вебхук для бота. Это необходимо сделать один раз.
var webhookUrl = $@"{address}/api/TelegramBot"; // Замените на ваш URL.
botClient.DeleteWebhookAsync().Wait();
botClient.SetWebhookAsync(webhookUrl).Wait();

// Регистрируем бота как сервис для доступа из контроллеров.
builder.Services.AddSingleton<ITelegramBotClient>(botClient);

builder.Services
    .AddControllers()
    .AddNewtonsoftJson();

builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<TelegramAPPContext>(options =>
        options.UseSqlServer(connectionString)
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public class BotConfiguration
{
    public static readonly string Configuration = "BotConfiguration";

    public string BotToken { get; init; } = default!;
    public string HostAddress { get; init; } = default!;
    public string Route { get; init; } = default!;
    public string SecretToken { get; init; } = default!;
}