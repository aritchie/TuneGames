namespace TuneGames.Models;

public class Category
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}

public class GameSettings
{
    public string Id { get; set; } = "default";
    public int SongsPerRound { get; set; } = 4;
    public int ClipDurationSeconds { get; set; } = 15;
    public int TotalChoices { get; set; } = 12;
    public int AnswerTimeLimitSeconds { get; set; } = 30;
    public int PauseBetweenClipsSeconds { get; set; } = 2;
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
