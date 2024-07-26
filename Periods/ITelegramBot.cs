using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace Periods;

public interface ITelegramBot
{
    Task SendTextMessageAsync(long chatId, string markdownText);
}

public class TelegramBot : ITelegramBot
{
    private readonly TelegramBotClient _telegramBot;

    public TelegramBot(TelegramBotClient telegramBot)
    {
        _telegramBot = telegramBot;
    }

    public Task SendTextMessageAsync(long chatId, string markdownText)
    {
        return _telegramBot.SendTextMessageAsync(chatId, markdownText, parseMode: ParseMode.Html);
    }
}

