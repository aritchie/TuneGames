using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Shiny;
using Shiny.Maui.TableView;
using Shiny.Music;
using Shiny.SqliteDocumentDb;
using TuneGames.Services;

namespace TuneGames;

public static class MauiProgram
{
    const string AzureOpenAiEndpoint = "";
    const string AzureOpenAiApiKey = "";
    const string AzureOpenAiDeployment = "gpt-4.1";

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

        // AI / Azure OpenAI
        builder.Services.AddSingleton<IChatClient>(_ =>
            new AzureOpenAIClient(
                    new Uri(AzureOpenAiEndpoint),
                    new AzureKeyCredential(AzureOpenAiApiKey))
                .GetChatClient(AzureOpenAiDeployment)
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
