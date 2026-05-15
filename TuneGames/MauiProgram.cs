using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shiny.DocumentDb;
using Shiny.DocumentDb.Sqlite;
using Shiny.Maui.TableView;
using Shiny.Music;
using TuneGames.Services;

namespace TuneGames;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.Configuration.AddJsonStream(
            typeof(MauiProgram)
                .Assembly
                .GetManifestResourceStream("BeatTheBank.appsettings.json")!
        );
        
        builder
            .UseMauiApp<App>()
            .UseShinyShell(x => x.AddGeneratedMaps())
            .UseShinyControls(x => x.AddDefaultMauiControlFeedback())
#if !DEBUG
            .UseSentry(x => x.Dsn = builder.Configuration["SentryDsn"]!)
#endif
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });
        
#if DEBUG
        builder.Logging.SetMinimumLevel(LogLevel.Trace);
        builder.Logging.AddDebug();
#endif

        builder.Services.AddShinyMusic();
        builder.Services.AddDocumentStore(config =>
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "tunegames.db");
            config.DatabaseProvider = new SqliteDatabaseProvider($"Data Source={dbPath}");
            config.JsonSerializerOptions = AppJsonContext.Default.Options;
            config.UseReflectionFallback = false;
            
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

        return builder.Build();
    }
}
