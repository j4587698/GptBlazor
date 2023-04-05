using Furion.DependencyInjection;
using OpenAI_API;
using OpenAI_API.Chat;

namespace GptBlazor.Service;

public class OpenAiService : IScoped
{
    private const string _resourceName = "jxgpt";

    private const string _deploymentId = "gpt35test";

    private const string _apiKey = "e97ae651d99945e2b3bb7e666442f5e9";
    
    private readonly Conversation conversation;

    public OpenAiService()
    {
        var api = OpenAIAPI.ForAzure(_resourceName, _deploymentId, _apiKey);
        api.ApiVersion = "2023-03-15-preview";
        conversation = api.Chat.CreateConversation();
    }

    public void AddUserInputWithExampleOutput(string userInput, string exampleOutput)
    {
        conversation.AppendUserInput(userInput);
        conversation.AppendExampleChatbotOutput(exampleOutput);
    }

    public IAsyncEnumerable<string> StreamResponseEnumerableFromChatbotAsync(string userInput)
    {
        conversation.AppendUserInput(userInput);
        return conversation.StreamResponseEnumerableFromChatbotAsync();
    }
}