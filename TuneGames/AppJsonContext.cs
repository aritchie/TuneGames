using System.Text.Json.Serialization;
using TuneGames.Models;
using TuneGames.Services;

namespace TuneGames;

[JsonSerializable(typeof(Category))]
[JsonSerializable(typeof(GameSettings))]
[JsonSerializable(typeof(GameResult))]
[JsonSerializable(typeof(List<TrackInfo>))]
[JsonSerializable(typeof(AiResponse))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, PropertyNameCaseInsensitive = true)]
partial class AppJsonContext : JsonSerializerContext;