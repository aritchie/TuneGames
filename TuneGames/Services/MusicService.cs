using Shiny.Music;

namespace TuneGames.Services;

public interface IMusicService
{
    Task<bool> RequestPermissionAsync();
    Task<bool> HasStreamingSubscriptionAsync();
    Task<IReadOnlyList<MusicMetadata>> GetAllTracksAsync();
    Task<IReadOnlyList<MusicMetadata>> SearchTracksAsync(string query);
    Task PlayTrackAsync(MusicMetadata track);
    Task PlayClipAsync(MusicMetadata track, TimeSpan duration);
    void Pause();
    void Stop();
    event Action? PlaybackCompleted;
}

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

    public async Task<IReadOnlyList<MusicMetadata>> GetAllTracksAsync()
        => await this.library.GetAllTracksAsync();

    public async Task<IReadOnlyList<MusicMetadata>> SearchTracksAsync(string query)
        => await this.library.SearchTracksAsync(query);

    public async Task PlayTrackAsync(MusicMetadata track)
        => await this.player.PlayAsync(track);

    public async Task PlayClipAsync(MusicMetadata track, TimeSpan duration)
    {
        this.clipTcs = new TaskCompletionSource();

        await this.player.PlayAsync(track);

        using var cts = new CancellationTokenSource(duration);
        cts.Token.Register(() =>
        {
            this.player.Stop();
            this.clipTcs?.TrySetResult();
        });

        void OnComplete(object? sender, EventArgs e)
        {
            this.clipTcs?.TrySetResult();
        }
        this.player.PlaybackCompleted += OnComplete;

        try
        {
            await this.clipTcs.Task;
        }
        finally
        {
            this.player.PlaybackCompleted -= OnComplete;
        }
    }

    public void Pause() => this.player.Pause();
    public void Stop() => this.player.Stop();
}
