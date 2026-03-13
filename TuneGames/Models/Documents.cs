namespace TuneGames.Models;

public class Category
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}

public record CategoryItem(string DisplayName, int TrackCount, string? Genre = null, int? Decade = null, int? Year = null, string? PlaylistId = null);

public class GameSettings
{
    public string Id { get; set; } = "default";
    public int SongsPerRound { get; set; } = 4;
    public int ClipDurationSeconds { get; set; } = 15;
    public int TotalChoices { get; set; } = 12;
    public int AnswerTimeLimitSeconds { get; set; } = 30;
    public int PauseBetweenClipsSeconds { get; set; } = 2;
    
    // public string? AzureOpenAiEndpoint { get; set; } = "";
    // public string? AzureOpenAiApiKey { get; set; } = "";
    // public string AzureOpenAiDeployment { get; set; } = "gpt-4.1";
}

public class GameResult
{
    public string Id { get; set; } = null!;
    public string CategoryName { get; set; } = null!;
    public DateTimeOffset DatePlayed { get; set; }
    public int CorrectCount { get; set; }
    public int TotalSongs { get; set; }
    public List<string> SongsPlayed { get; set; } = [];
    public List<string> SongsPicked { get; set; } = [];
}
