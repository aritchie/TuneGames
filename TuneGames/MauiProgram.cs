using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shiny.Maui.TableView;
using Shiny.Music;
using Shiny.SqliteDocumentDb;
using TuneGames.Services;

namespace TuneGames;

public static class MauiProgram
{
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

        builder.Configuration.AddJsonPlatformBundle();
        builder.Services.AddShinyMusic();

        builder.Services.AddSqliteDocumentStore(config =>
        {
            config.JsonSerializerOptions = AppJsonContext.Default.Options;
            config.UseReflectionFallback = false;
            config.ConnectionString = $"Data Source={Path.Combine(FileSystem.AppDataDirectory, "tunegames.db")}";
        });

        builder.Services.AddSingleton(_ =>
            new AzureOpenAIClient(
                    new Uri(builder.Configuration["AzureOpenAiEndpoint"]!),
                    new AzureKeyCredential(builder.Configuration["AzureOpenAiApiKey"]!))
                .GetChatClient(builder.Configuration["AzureOpenAiModel"]!)
                .AsIChatClient()
        );
        builder.Services.AddShinyStores();
        builder.Services.AddPersistentService<GameSettings>();

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
