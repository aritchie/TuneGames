using CommunityToolkit.Mvvm.ComponentModel;

namespace TuneGames.Services;

[Reflector]
public partial class GameSettings : ObservableObject 
{
    [ObservableProperty] int songsPerRound = 4;
    [ObservableProperty] int clipDurationSeconds  = 3;
    [ObservableProperty] int totalChoices  = 12;
    [ObservableProperty] int answerTimeLimitSeconds  = 30;
    [ObservableProperty] int pauseBetweenClipsSeconds  = 1;
    
    // public string? AzureOpenAiEndpoint  = "";
    // public string? AzureOpenAiApiKey  = "";
    // public string AzureOpenAiDeployment  = "gpt-4.1";
}