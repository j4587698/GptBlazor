using System.Diagnostics.CodeAnalysis;
using Furion.LinqBuilder;
using GptBlazor.Entity;
using GptBlazor.Service;
using Markdig;
using Microsoft.AspNetCore.Components;

namespace GptBlazor.Pages;

public partial class Index
{
    private List<Message> _messages;

    [Inject]
    [NotNull]
    private OpenAiService? OpenAiService { get; set; }
    
    private string _userInput = string.Empty;
    
    private string _response = string.Empty;
    
    private bool _isThinking = false;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        _messages = new List<Message>();
    }

    private async Task SendMessage()
    {
        if (_userInput.IsNullOrEmpty())
        {
            return;
        }

        var input = _userInput;
        _userInput = string.Empty;;
        _messages.Add(new Message
        {
            IsBoot = false,
            ChatMessage = input,
            CreateTime = DateTime.Now
        });
        _isThinking = true;
        StateHasChanged();
        await foreach (var str in OpenAiService.StreamResponseEnumerableFromChatbotAsync(input))
        {
            _response += str;
            StateHasChanged();
        }
        
        _messages.Add(new Message
        {
            IsBoot = true,
            ChatMessage = Markdown.ToHtml(_response),
            CreateTime = DateTime.Now
        });
        _isThinking = false;
        _response = string.Empty;
        StateHasChanged();
    }
}