using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using OpenAI;
using Shiny;
using Shiny.Maui.TableView;
using Shiny.Music;
using Shiny.SqliteDocumentDb;
using TuneGames.Services;

namespace TuneGames;

public static class MauiProgram
{
    // TODO: Replace with your OpenAI API key or move to secure storage
    const string OpenAiApiKey = "YOUR-OPENAI-KEY-HERE";
    const string OpenAiModel = "gpt-4o-mini";

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseShinyShell(x => x.AddGeneratedMaps())
            .UseShinyTableView()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Music
        builder.Services.AddShinyMusic();

        // SQLite Document Store
        builder.Services.AddSqliteDocumentStore(config =>
        {
            config.JsonSerializerOptions = AppJsonContext.Default.Options;
            config.UseReflectionFallback = false;
            config.ConnectionString = $"Data Source={Path.Combine(FileSystem.AppDataDirectory, "tunegames.db")}";
        });

        // AI / OpenAI
        builder.Services.AddSingleton<IChatClient>(_ =>
            new OpenAIClient(OpenAiApiKey)
                .GetChatClient(OpenAiModel)
                .AsIChatClient()
        );

        // App Services
        builder.Services.AddSingleton<IMusicService, MusicService>();
        builder.Services.AddSingleton<IAiSongPicker, AiSongPicker>();
        builder.Services.AddSingleton<IGameEngine, GameEngine>();
        builder.Services.AddSingleton<IGameStore, GameStore>();

#if DEBUG
        builder.Logging.SetMinimumLevel(LogLevel.Trace);
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
