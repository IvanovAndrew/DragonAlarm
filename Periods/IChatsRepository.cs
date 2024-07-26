using Supabase;

namespace Periods;

public interface IChatsRepository
{
    Task<List<ChatInfo>> GetChats(bool onlyActive = true);
    Task Subscribe(ChatInfo chat);
    Task Unsubscribe(ChatInfo chat);
}

public class ChatsRepository : IChatsRepository
{
    private readonly Client _db;

    public ChatsRepository(Supabase.Client db)
    {
        _db = db;
    }
    
    public async Task<List<ChatInfo>> GetChats(bool onlyActive = true)
    {
        var response = await _db.From<TelegramChats>().Get();
        return response.Models.Where(c => !onlyActive || onlyActive && c.IsSubscribed).Select(c => new ChatInfo(){ChatId = c.ChatId, UserId = c.UserId}).ToList();
    }

    public async Task Subscribe(ChatInfo chat)
    {
        await _db.From<TelegramChats>().Upsert(
            new TelegramChats()
            {
                UserId = chat.UserId, 
                ChatId = chat.ChatId, 
                IsSubscribed = true
            });
    }

    public async Task Unsubscribe(ChatInfo chat)
    {
        await _db.From<TelegramChats>().Upsert(
            new TelegramChats()
            {
                UserId = chat.UserId, 
                ChatId = chat.ChatId, 
                IsSubscribed = false
            });
    }
}

public class ChatInfo
{
    public long UserId { get; init; }
    public long ChatId { get; init; }
}