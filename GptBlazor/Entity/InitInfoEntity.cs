namespace GptBlazor.Entity;

public class InitInfoEntity
{
    public string? SystemInfo { get; set; }
    
    public List<SimpleBot> SimpleBots { get; set; } = new();

    public class SimpleBot
    {
        public string? UserInput { get; set; }

        public string? SimpleOutput { get; set; }
    }
}