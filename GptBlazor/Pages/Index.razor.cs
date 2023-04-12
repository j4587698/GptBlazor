using System.Diagnostics.CodeAnalysis;
using System.Text;
using BootstrapBlazor.Components;
using Furion.LinqBuilder;
using GptBlazor.Components;
using GptBlazor.Entity;
using GptBlazor.Service;
using Markdig;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Toolbelt.Blazor.HotKeys2;
using Message = GptBlazor.Entity.Message;

namespace GptBlazor.Pages;

public partial class Index : IAsyncDisposable
{
    private readonly List<Message> _messages = new List<Message>();
    
    [Inject]
    [NotNull]
    private IJSRuntime? JsRuntime { get; set; }

    [Inject]
    [NotNull]
    private OpenAiService? OpenAiService { get; set; }
    
    [Inject]
    [NotNull]
    private HotKeys? HotKeys { get; set; }
    
    [Inject]
    [NotNull]
    private DialogService? DialogService { get; set; }
    
    private ElementReference Input { get; set; }
    
    [NotNull]
    private Button? SubmitButton { get; set; }
    
    [Inject]
    [NotNull]
    private DownloadService? DownloadService { get; set; }
    
    private string _userInput = string.Empty;
    
    private string _response = string.Empty;
    
    private bool _isThinking = false;

    private HotKeysContext? _hotKeysContext;

    private InitInfoEntity _initInfoEntity = new();

    private MarkdownPipeline? _pipeline;
    
    protected override void OnInitialized()
    {
        base.OnInitialized();
        _pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().UseBootstrap().Build();
        this._hotKeysContext = this.HotKeys.CreateContext()
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
        var now = DateTime.Now;
        await foreach (var str in OpenAiService.StreamResponseEnumerableFromChatbotAsync(input))
        {
            _response += str;
            if (DateTime.Now - now >= TimeSpan.FromSeconds(1))
            {
                now = DateTime.Now;
                StateHasChanged();
                await JsRuntime.InvokeVoidAsync("scrollToBottom");
            }
            
        }
        
        StateHasChanged();
        await JsRuntime.InvokeVoidAsync("scrollToBottom");
        
        _messages.Add(new Message
        {
            IsBoot = true,
            ChatMessage = _response,
            CreateTime = DateTime.Now
        });
        _isThinking = false;
        _response = string.Empty;
        StateHasChanged();
        await JsRuntime.InvokeVoidAsync("highlight");
        await JsRuntime.InvokeVoidAsync("scrollToBottom");
        await Input.FocusAsync();
    }

    private async Task CreateNewChat()
    {
        OpenAiService.CreateNewConversation();
        _messages.Clear();
        await Input.FocusAsync();
    }

    private async Task InitBot()
    {
        await DialogService.ShowModal<InitInfoComponent>(new ResultDialogOption()
        {
            Title = "添加初始设置",
            ComponentParamters = new Dictionary<string, object>()
            {
                [nameof(InitInfoComponent.InitInfo)] = _initInfoEntity
            }
        });
        await CreateNewChat();
        if (!_initInfoEntity.SystemInfo.IsNullOrEmpty())
        {
            OpenAiService.AddSystemInput(_initInfoEntity.SystemInfo!);
        }
        
        foreach (var simpleBot in _initInfoEntity.SimpleBots)
        {
            OpenAiService.AddUserInputWithExampleOutput(simpleBot.UserInput, simpleBot.SimpleOutput);
        }
    }
    
    private async Task Download()
    {
        var ms = new MemoryStream();
        var writer = new StreamWriter(ms);
        foreach (var message in _messages)
        {
            await writer.WriteLineAsync(message.IsBoot ? "# ChatGPT" : "# User");
            await writer.WriteLineAsync(message.ChatMessage);
            await writer.WriteLineAsync(message.CreateTime.ToString("yyyy-MM-dd HH:mm:ss"));
            await writer.FlushAsync();
        }

        ms.Position = 0;
        await DownloadService.DownloadFromStreamAsync("ChatGPT.txt", ms);
    }

    public async ValueTask DisposeAsync()
    {
        await this.HotKeys.DisposeAsync();
    }
}