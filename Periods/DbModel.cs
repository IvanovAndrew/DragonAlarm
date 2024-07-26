using Postgrest.Attributes;
using Postgrest.Models;

namespace Periods;

[Table("periods")]
public class Periods : BaseModel
{
    [PrimaryKey("id")]
    public long Id { get; set; }
    [Column("first_date")]
    public DateOnly Date { get; set; }
}

[Table("telegram_chats")]
public class TelegramChats : BaseModel
{
    [PrimaryKey("user_id")]
    public long UserId { get; set; }
    
    [Column("chat_id")]
    public long ChatId { get; set; }
    
    [Column("is_subscribed")]
    public bool IsSubscribed { get; set; }
}