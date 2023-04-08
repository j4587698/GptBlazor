using Furion.DynamicApiController;
using GptBlazor.Service;
using Microsoft.AspNetCore.Mvc;

namespace GptBlazor.Controllers;

public class ChatController : IDynamicApiController
{
    private readonly OpenAiService _openAiService;
    
    public ChatController(OpenAiService service)
    {
        _openAiService = service;
    }
    
    
    public async Task PostStreamResponseEnumerableFromChatbotAsync([FromBody]string userInput)
    {
        var httpContext = Furion.App.HttpContext;
        var response = httpContext.Response;
        response.ContentType = "text/plain";

        var writer = new StreamWriter(response.Body);
        
        await foreach(var res in _openAiService.StreamResponseEnumerableFromChatbotAsync(userInput))
        {
            await writer.WriteLineAsync(res);
            await writer.FlushAsync();
        }
        
        await writer.FlushAsync();
    }
    
}