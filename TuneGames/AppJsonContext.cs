using System.Text.Json.Serialization;
using TuneGames.Models;
using TuneGames.Services;

namespace TuneGames;

[JsonSerializable(typeof(Category))]
[JsonSerializable(typeof(GameSettings))]
[JsonSerializable(typeof(GameResult))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
partial class AppJsonContext : JsonSerializerContext;