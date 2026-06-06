namespace TuneGames.Services;

[Singleton]
[BindNotify]
public partial class GameSettings 
{
    [Bind(Default = 4)]
    public partial int SongsPerRound { get; set; }
    
    [Bind(Default = 3)]
    public partial int ClipDurationSeconds { get; set; }
    
    [Bind(Default = 12)]
    public partial int TotalChoices { get; set; }
    
    [Bind(Default = 30)]
    public partial int AnswerTimeLimitSeconds { get; set; }
    
    [Bind(Default = 1)]
    public partial int PauseBetweenClipsSeconds { get; set; }
    
    // public string? AzureOpenAiEndpoint  = "";
    // public string? AzureOpenAiApiKey  = "";
    // public string AzureOpenAiDeployment  = "gpt-4.1";
}