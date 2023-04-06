using System.Diagnostics.CodeAnalysis;
using BootstrapBlazor.Components;
using Furion.LinqBuilder;
using GptBlazor.Service;
using Markdig;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Toolbelt.Blazor.HotKeys2;
using Message = GptBlazor.Entity.Message;

namespace GptBlazor.Pages;

public partial class Index : IAsyncDisposable
{
    private List<Message> _messages;
    
    [Inject]
    [NotNull]
    private IJSRuntime? JsRuntime { get; set; }

    [Inject]
    [NotNull]
    private OpenAiService? OpenAiService { get; set; }
    
    [Inject]
    [NotNull]
    private HotKeys HotKeys { get; set; }
    
    private ElementReference Input { get; set; }
    
    private Button? SubmitButton { get; set; }
    
    private string _userInput = string.Empty;
    
    private string _response = string.Empty;
    
    private bool _isThinking = false;

    private HotKeysContext HotKeysContext;
    
    protected override void OnInitialized()
    {
        base.OnInitialized();
        _messages = new List<Message>();
        this.HotKeysContext = this.HotKeys.CreateContext()
            .Add(ModCode.Ctrl, Code.Enter, async () =>
            {
                await SubmitButton.FocusAsync();
                await SendMessage();
            }, exclude: Exclude.None);
    }

    private async Task SendMessage()
    {
        //await JsRuntime.InvokeVoidAsync("focusInput", SubmitButton);
        if (_userInput.IsNullOrEmpty() || _isThinking)
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
        await JsRuntime.InvokeVoidAsync("scrollToBottom");
        await foreach (var str in OpenAiService.StreamResponseEnumerableFromChatbotAsync(input))
        {
            _response += str;
            StateHasChanged();
            await JsRuntime.InvokeVoidAsync("scrollToBottom");
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
        await JsRuntime.InvokeVoidAsync("scrollToBottom");
        await Input.FocusAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await this.HotKeys.DisposeAsync();
    }
}