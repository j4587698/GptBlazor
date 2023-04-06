using Furion.DependencyInjection;
using OpenAI_API;
using OpenAI_API.Chat;

namespace GptBlazor.Service;

public class OpenAiService : IScoped
{

    private readonly Conversation conversation;

    public OpenAiService()
    {
        var api = OpenAIAPI.ForAzure(Constant.ResourceName, Constant.DeploymentId, Constant.ApiKey);
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