using Shiny.Music;

namespace TuneGames.Services;

public interface IMusicService
{
    Task<bool> RequestPermissionAsync();
    Task<bool> HasStreamingSubscriptionAsync();
    Task<IReadOnlyList<GroupedCount<string>>> GetGenresAsync();
    Task<IReadOnlyList<GroupedCount<int>>> GetDecadesAsync();
    Task<IReadOnlyList<GroupedCount<int>>> GetYearsAsync();
    Task<IReadOnlyList<PlaylistInfo>> GetPlaylistsAsync();
    Task<IReadOnlyList<MusicMetadata>> GetPlaylistTracksAsync(string playlistId);
    Task<IReadOnlyList<MusicMetadata>> GetAllTracksAsync();
    Task<IReadOnlyList<MusicMetadata>> GetTracksAsync(MusicFilter filter);
    Task<IReadOnlyList<MusicMetadata>> SearchTracksAsync(string query);
    Task PlayTrackAsync(MusicMetadata track);
    Task PlayClipAsync(MusicMetadata track, TimeSpan duration);
    void Pause();
    void Resume();
    void Stop();
    event Action? PlaybackCompleted;
}

[Singleton]
public class MusicService : IMusicService
{
    readonly IMediaLibrary library;
    readonly IMusicPlayer player;
    TaskCompletionSource? clipTcs;

    public MusicService(IMediaLibrary library, IMusicPlayer player)
    {
        this.library = library;
        this.player = player;
        this.player.PlaybackCompleted += (sender, e) => this.PlaybackCompleted?.Invoke();
    }

    public event Action? PlaybackCompleted;

    public async Task<bool> RequestPermissionAsync()
    {
        var status = await this.library.RequestPermissionAsync();
        return status == Shiny.Music.PermissionStatus.Granted;
    }

    public Task<bool> HasStreamingSubscriptionAsync()
        => this.library.HasStreamingSubscriptionAsync();

    public Task<IReadOnlyList<GroupedCount<string>>> GetGenresAsync()
        => this.library.GetGenresAsync();

    public Task<IReadOnlyList<GroupedCount<int>>> GetDecadesAsync()
        => this.library.GetDecadesAsync();

    public Task<IReadOnlyList<GroupedCount<int>>> GetYearsAsync()
        => this.library.GetYearsAsync();

    public Task<IReadOnlyList<PlaylistInfo>> GetPlaylistsAsync()
        => this.library.GetPlaylistsAsync();

    public Task<IReadOnlyList<MusicMetadata>> GetPlaylistTracksAsync(string playlistId)
        => this.library.GetPlaylistTracksAsync(playlistId);

    public async Task<IReadOnlyList<MusicMetadata>> GetAllTracksAsync()
        => await this.library.GetAllTracksAsync();

    public async Task<IReadOnlyList<MusicMetadata>> GetTracksAsync(MusicFilter filter)
        => await this.library.GetTracksAsync(filter);

    public async Task<IReadOnlyList<MusicMetadata>> SearchTracksAsync(string query)
        => await this.library.SearchTracksAsync(query);

    public async Task PlayTrackAsync(MusicMetadata track)
        => await this.player.PlayAsync(track);

    public async Task PlayClipAsync(MusicMetadata track, TimeSpan duration)
    {
        this.player.Stop();

        var tcs = new TaskCompletionSource();
        this.clipTcs = tcs;

        using var cts = new CancellationTokenSource(duration);
        cts.Token.Register(() =>
        {
            this.player.Stop();
            tcs.TrySetResult();
        });

        void OnComplete(object? sender, EventArgs e)
        {
            tcs.TrySetResult();
        }
        this.player.PlaybackCompleted += OnComplete;

        try
        {
            await this.player.PlayAsync(track);
            await tcs.Task;
        }
        finally
        {
            this.player.PlaybackCompleted -= OnComplete;
        }
    }

    public void Pause() => this.player.Pause();
    public void Resume() => this.player.Resume();
    public void Stop() => this.player.Stop();
}
