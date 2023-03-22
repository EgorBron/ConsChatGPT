using System.Net.Http.Json;
using System.Text.Json.Serialization;

/// <summary>OpenAI API token</summary>
const string OPENAI_API_TOKEN = "";

/// <summary>OpenAI API endpoint</summary>
const string ENDPOINT = "https://api.openai.com/v1/chat/completions";

/// <summary>Log message to console</summary>
/// <param name="role">Role of message author. Can be System, User or Assistant (ChatGPT)</param>
/// <param name="message">Message to log</param>
void Log(string role, string? message = null) {
    // select color depending on role
    Console.ForegroundColor = role.ToLower() switch {
        "user" => ConsoleColor.DarkGray,
        "chatgpt" => ConsoleColor.Cyan,
        "system" => ConsoleColor.DarkRed,
        _ => ConsoleColor.White
    };
    // log role first
    Console.Write($"{role}: ");
    // reset color
    Console.ForegroundColor = ConsoleColor.White;
    // and then log message
    if (message != null) Console.WriteLine(message);
}


/// <summary>Chat history, also known as "context"</summary>
List<Message> messages = new List<Message>();

/// <summary>HttpClient for API interaction</summary>
var httpClient = new HttpClient();

// set the token in request headers
httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {OPENAI_API_TOKEN}");

// greetings!
Log("System", "Welcome to ConsChatGPT. Write your messages, get your answers. Additional commands: /system to add system message, /clearctx to clear context (messages history), /regenerate to regenerate answer.");

// alert if no token
if (string.IsNullOrEmpty(OPENAI_API_TOKEN)) {
    Log("System", "OpenAI token is empty. Obtain it on https://platform.openai.com/account/api-keys and place it in OPENAI_API_TOKEN constant and then rebuild project.");
    Environment.Exit(-1);
}


while (true) {
    // input
    Log("User");
    var content = Console.ReadLine();

    // if message is shorter than 1 symbol
    // end loop
    if (content is not { Length: > 0 }) {
        Log("System", "Empty message, exiting.");
        break;
    }
    // context clearing
    if (content == "/clearctx") { 
        messages.Clear();
        Log("System", "Context cleared.");
        continue;
    }
    // if re-generation of answer is not needed
    if (content != "/regenerate") {
        var role = "user";

        // system message command
        if (content.StartsWith("/system ")) {
            content = content.Substring(8);
            role = "system";
            Log("System", "You have passed a system message. That is just a not important instruction to ChatGPT. Now enter a regular user message.");
        }
        // forming message to send
        var message = new Message() { Role = role, Content = content };

        // and then adding it to history
        messages.Add(message);

        // skipping system messages
        if (role == "system") continue;
    } else messages.RemoveAt(messages.Count - 1); // for re-generation of answer
    // forming request data
    var requestData = new Request() {
        ModelId = "gpt-3.5-turbo",
        Messages = messages
    };
    // sending
    using var response = await httpClient.PostAsJsonAsync(ENDPOINT, requestData);

    // log HTTP errors (by HTTP codes, not C# exceptions)
    if (!response.IsSuccessStatusCode) {
        Log("System", $"{(int)response.StatusCode} {response.StatusCode}");
        break;
    }
    // get response data
    ResponseData? responseData = await response.Content.ReadFromJsonAsync<ResponseData>();

    // and lets check for any choices
    var choices = responseData?.Choices ?? new List<Choice>();
    if (choices.Count == 0) {
        Log("System", "No choices were returned by the API");
        continue;
    }
    var choice = choices[0];
    var responseMessage = choice.Message;

    // adding response to history
    messages.Add(responseMessage);
    var responseText = responseMessage.Content.Trim();
    Log("ChatGPT", responseText);
}

///<summary>Message API model class</summary>
class Message {
    /// <summary>Role of message sender</summary>
    [JsonPropertyName("role")]
    public string Role { get; set; } = "";
    ///<summary>Message content</summary>
    [JsonPropertyName("content")]
    public string Content { get; set; } = "";
}
///<summary>Full request API model class</summary>
class Request {
    ///<summary>GPT model ID</summary>
    [JsonPropertyName("model")]
    public string ModelId { get; set; } = "";
    ///<summary>Chat history</summary>
    [JsonPropertyName("messages")]
    public List<Message> Messages { get; set; } = new();
}
///<summary>Response API model class</summary>
class ResponseData {
    ///<summary>ID</summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";
    ///<summary>Object</summary>
    [JsonPropertyName("object")]
    public string Object { get; set; } = "";
    ///<summary>Timestamp</summary>
    [JsonPropertyName("created")]
    public ulong Created { get; set; }
    ///<summary>Answer choices</summary>
    [JsonPropertyName("choices")]
    public List<Choice> Choices { get; set; } = new();
    ///<summary>Usage</summary>
    [JsonPropertyName("usage")]
    public Usage Usage { get; set; } = new();
}
///<summary>Answer choice API model class</summary>
class Choice {
    ///<summary>Index of choice</summary>
    [JsonPropertyName("index")]
    public int Index { get; set; }
    ///<summary>Message</summary>
    [JsonPropertyName("message")]
    public Message Message { get; set; } = new();
    ///<summary>Finish reason</summary>
    [JsonPropertyName("finish_reason")]
    public string FinishReason { get; set; } = "";
}
///<summary>Message API model class</summary>
class Usage {
    ///<summary>Count of tokens in input(s)</summary>
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }
    ///<summary>Count of tokens in answer(s)</summary>
    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }
    ///<summary>Total count of tokens</summary>
    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}