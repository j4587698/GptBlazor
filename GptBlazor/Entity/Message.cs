namespace GptBlazor.Entity;

public class Message
{
    public bool IsBoot { get; set; }

    public string? ChatMessage { get; set; }

    public DateTime CreateTime { get; set; }
}