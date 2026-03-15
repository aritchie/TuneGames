using TuneGames.Services;

namespace TuneGames.Models;

public record SongChoice(
    string Id,
    string Title,
    string Artist,
    string? Album,
    TimeSpan Duration,
    bool IsCorrect = false
);

public record AiPickResult(
    List<SongChoice> SongsToPlay,
    List<SongChoice> AllChoices
);

public record GameRound(
    string CategoryName,
    GameSettings Settings,
    AiPickResult Picks
);

public enum GamePhase
{
    Loading,
    Playing,
    Answering,
    Results
}
