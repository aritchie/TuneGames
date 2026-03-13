using Shiny.Music;
using TuneGames.Models;

namespace TuneGames.Services;

public interface IGameEngine
{
    Task<GameRound> StartRoundAsync(string categoryName, MusicFilter filter, GameSettings settings, string? playlistId = null);
    Task PlayClipsAsync(GameRound round, Action<int, int>? onProgress = null, CancellationToken ct = default);
    GameResult SubmitAnswers(GameRound round, IReadOnlyList<string> selectedSongIds);
}

public class GameEngine(IMusicService music, IAiSongPicker aiPicker) : IGameEngine
{
    public async Task<GameRound> StartRoundAsync(string categoryName, MusicFilter filter, GameSettings settings, string? playlistId = null)
    {
        var hasPermission = await music.RequestPermissionAsync();
        if (!hasPermission)
            throw new InvalidOperationException("Music library permission is required to play the game.");

        var tracks = playlistId != null
            ? await music.GetPlaylistTracksAsync(playlistId)
            : await music.GetTracksAsync(filter);
        if (tracks.Count < settings.TotalChoices)
            throw new InvalidOperationException(
                $"Not enough songs in this category. Found {tracks.Count}, need at least {settings.TotalChoices}.");

        var picks = await aiPicker.PickSongsAsync(
            categoryName,
            tracks,
            settings.SongsPerRound,
            settings.TotalChoices
        );

        return new GameRound(categoryName, settings, picks);
    }

    public async Task PlayClipsAsync(
        GameRound round,
        Action<int, int>? onProgress = null,
        CancellationToken ct = default)
    {
        var tracks = await music.GetAllTracksAsync();
        var trackLookup = tracks.ToDictionary(t => t.Id);
        var clipDuration = TimeSpan.FromSeconds(round.Settings.ClipDurationSeconds);
        var pauseDuration = TimeSpan.FromSeconds(round.Settings.PauseBetweenClipsSeconds);

        for (var i = 0; i < round.Picks.SongsToPlay.Count; i++)
        {
            ct.ThrowIfCancellationRequested();

            var song = round.Picks.SongsToPlay[i];
            onProgress?.Invoke(i + 1, round.Picks.SongsToPlay.Count);

            if (trackLookup.TryGetValue(song.Id, out var track))
            {
                await music.PlayClipAsync(track, clipDuration);
            }

            if (i < round.Picks.SongsToPlay.Count - 1)
            {
                await Task.Delay(pauseDuration, ct);
            }
        }
    }

    public GameResult SubmitAnswers(GameRound round, IReadOnlyList<string> selectedSongIds)
    {
        var correctIds = round.Picks.SongsToPlay
            .Select(s => s.Id)
            .ToHashSet();

        var correctCount = selectedSongIds.Count(id => correctIds.Contains(id));

        return new GameResult
        {
            Id = Guid.NewGuid().ToString(),
            CategoryName = round.CategoryName,
            DatePlayed = DateTimeOffset.Now,
            CorrectCount = correctCount,
            TotalSongs = round.Settings.SongsPerRound,
            SongsPlayed = round.Picks.SongsToPlay.Select(s => $"{s.Title} - {s.Artist}").ToList(),
            SongsPicked = selectedSongIds
                .Select(id => round.Picks.AllChoices.FirstOrDefault(c => c.Id == id))
                .Where(s => s != null)
                .Select(s => $"{s!.Title} - {s.Artist}")
                .ToList()
        };
    }
}
