using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Supabase;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Client = Supabase.Client;

namespace Periods;

public class HannaPeriods
{
    private readonly ILogger _logger;

    public HannaPeriods(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<HannaPeriods>();
    }
    
    [Function("PeriodsByTimer")]
    public async Task PeriodsByTimer([TimerTrigger("0 0 * * * *")] TimerInfo timerInfo)
    {
        _logger.LogInformation($"It's {DateTime.UtcNow} UTC.");
        
        try
        {
            var db = await InitDbReader();
            var periodsRepository = new PeriodsRepositoryDecorator(new PeriodsRepository(db), _logger);
            var chatRepository = new ChatsRepository(db);
            var telegramBot = await GetTelegramBot();

            _logger.LogInformation($"Getting chats");
            var chats = await chatRepository.GetChats();
            _logger.LogInformation($"Number of active chats: {chats.Count}");
            
            var periodInfo = PeriodInfo.Create(await periodsRepository.LastDate());

            foreach (var chat in chats)
            {
                await telegramBot.SendTextMessageAsync(chat.ChatId, $"<b>День {periodInfo.Day}</b>\n\n<i>{periodInfo.Description}</i>");
            }
        }
        catch (Exception e)
        {
            _logger.LogError($"Couldn't run a timer {e}");
        }
    }
    
    [Function("PeriodsByRequest")]
    public async Task<HttpResponseData> PeriodsByRequest(
        [HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequestData req,
        FunctionContext executionContext)
    {
        try
        {
            string content = await new StreamReader(req.Body).ReadToEndAsync();
        
            _logger.LogInformation($"Request is received:{content}");
            
            Update update = JsonConvert.DeserializeObject<Update>(content);
            _logger.LogInformation($"Update.Type is {update.Type}");

            ChatInfo chatInfo = default;
            string text = string.Empty;
            
            if (update.Type == UpdateType.Message)
            {
                chatInfo = new ChatInfo() { UserId = update.Message.From!.Id, ChatId = update.Message.Chat.Id };
                text = update.Message.Text;
            }
            else if (update.Type == UpdateType.CallbackQuery)
            {
                chatInfo = new ChatInfo() { UserId = update.CallbackQuery.From.Id, ChatId = update.CallbackQuery.Message.Chat.Id };
                text = update.CallbackQuery.Data;
            }
            
            _logger.LogInformation($"User id {chatInfo.UserId} Chat id {chatInfo.ChatId} Text is {text}");

            var db = await InitDbReader();
            var periodRepository = new PeriodsRepositoryDecorator(new PeriodsRepository(db), _logger);
            var chatRepository = new ChatsRepository(db);
            var telegramBot = await GetTelegramBot();
            
            if (string.Equals(text, "/newperiod", StringComparison.InvariantCultureIgnoreCase))
            {
                var periodDate = DateOnly.FromDateTime(DateTime.Today);
                await periodRepository.Add(periodDate);

                await telegramBot.SendTextMessageAsync(chatInfo.ChatId, $"{periodDate} is saved");
            }
            else if (string.Equals(text, "/subscribe", StringComparison.InvariantCultureIgnoreCase))
            {
                await chatRepository.Subscribe(chatInfo);
                await telegramBot.SendTextMessageAsync(chatInfo.ChatId, $"Subscribed");
            }
            else if (string.Equals(text, "/unsubscribe", StringComparison.InvariantCultureIgnoreCase))
            {
                await chatRepository.Unsubscribe(chatInfo);
                await telegramBot.SendTextMessageAsync(chatInfo.ChatId, $"Unsubscribed");
            }
            else
            {
                _logger.LogInformation("Calculating a period day");
                var periodInfo = PeriodInfo.Create(await periodRepository.LastDate());
                _logger.LogInformation($"Period day is {periodInfo.Day} Description is {periodInfo.Description}");
        
                await telegramBot.SendTextMessageAsync(chatInfo.ChatId, $"<b>День {periodInfo.Day}</b>\n\n<i>{periodInfo.Description}</i>");
            }

            return req.CreateResponse(HttpStatusCode.OK);
        }
        catch (Exception e)
        {
            _logger.LogError($"Error: {e}");
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }

    private async Task<ITelegramBot> GetTelegramBot()
    {
        string telegramToken = Environment.GetEnvironmentVariable("TELEGRAM_TOKEN");
        var telegramBot = new TelegramBotClient(telegramToken);
        await telegramBot.SetMyCommandsAsync(
            new[]
            {
                new BotCommand() { Command = "/info", Description = "What day is today?" },
                new BotCommand() { Command = "/newperiod", Description = "Today is the first day" },
                new BotCommand() { Command = "/subscribe", Description = "Subscribe" },
                new BotCommand() { Command = "/unsubscribe", Description = "Unsubscribe" },
            }
        );

        return new TelegramBot(telegramBot);
    }

    private async Task<Client> InitDbReader()
    {
        var url = Environment.GetEnvironmentVariable("SUPABASE_URL");
        var key = Environment.GetEnvironmentVariable("SUPABASE_KEY");
        
        _logger.LogInformation($"supabase url: {url}");
        _logger.LogInformation($"supabase key: {key}");

        SupabaseOptions options = new SupabaseOptions() { AutoConnectRealtime = true };
        
        var supabase = new Supabase.Client(url, key, options);
        await supabase.InitializeAsync();
        return supabase;
    }
}