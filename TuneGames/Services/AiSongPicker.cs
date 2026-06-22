using System.Text.Json;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Shiny.Music;
using TuneGames.Models;

namespace TuneGames.Services;

record TrackInfo(string Id, string? Title, string? Artist, string? Album);

record AiResponse(List<string> PlayIds, List<string> DecoyIds);

public interface IAiSongPicker
{
    Task<AiPickResult> PickSongsAsync(
        string category,
        IReadOnlyList<MusicMetadata> availableTracks,
        int songsToPlay,
        int totalChoices
    );
}

[Singleton]
public class AiSongPicker(IChatClient chatClient) : IAiSongPicker
{
    // async Task<IChatClient> GetChatClientAsync()
    // {
    //     var settings = await store.GetSettingsAsync();
    //     if (string.IsNullOrWhiteSpace(settings.AzureOpenAiEndpoint) ||
    //         string.IsNullOrWhiteSpace(settings.AzureOpenAiApiKey))
    //         throw new InvalidOperationException("Azure OpenAI endpoint and API key must be configured in Settings.");
    //
    //     var client = new AzureOpenAIClient(
    //         new Uri(settings.AzureOpenAiEndpoint),
    //         new AzureKeyCredential(settings.AzureOpenAiApiKey)
    //     );
    //     return client.GetChatClient(settings.AzureOpenAiDeployment).AsIChatClient();
    // }

    public async Task<AiPickResult> PickSongsAsync(
        string category,
        IReadOnlyList<MusicMetadata> availableTracks,
        int songsToPlay,
        int totalChoices)
    {
        //var chatClient = await GetChatClientAsync();
        var shuffled = availableTracks
            .OrderBy(_ => Random.Shared.Next())
            .ToList();
        
        var trackList = shuffled
            .Select(t => new TrackInfo(t.Id, t.Title, t.Artist, t.Album))
            .ToList();

        var trackJson = JsonSerializer.Serialize(trackList, AppJsonContext.Default.ListTrackInfo);

        var prompt = $"""
            You are a music trivia game assistant. Given a category and a list of available songs,
            select songs that best match the category.

            Category: "{category}"

            Available songs (JSON array):
            {trackJson}

            Instructions:
            1. Select exactly {songsToPlay} songs that match the category "{category}" - these will be PLAYED. Vary your picks each time - do not always choose the most obvious matches.
            2. Select {totalChoices - songsToPlay} additional songs as DECOYS that do NOT match as well but are plausible
            3. Return a JSON object with:
               - "playIds": array of {songsToPlay} song IDs to play (best matches)
               - "decoyIds": array of {totalChoices - songsToPlay} song IDs as decoys

            Return ONLY valid JSON, no markdown, no explanation.
            """;

        var response = await chatClient.GetResponseAsync(prompt);
        var content = response.Text.Trim();

        // Strip markdown code fences if present
        if (content.StartsWith("```"))
        {
            var lines = content.Split('\n');
            content = string.Join('\n', lines.Skip(1).TakeWhile(l => !l.StartsWith("```")));
        }

        var result = JsonSerializer.Deserialize(content, AppJsonContext.Default.AiResponse);

        if (result == null)
            throw new InvalidOperationException("AI returned invalid response");

        var trackLookup = availableTracks.DistinctBy(t => t.Id).ToDictionary(t => t.Id);

        var songsToPlayList = result.PlayIds
            .Where(trackLookup.ContainsKey)
            .Select(id => ToSongChoice(trackLookup[id], isCorrect: true))
            .ToList();

        var decoys = result.DecoyIds
            .Where(trackLookup.ContainsKey)
            .Select(id => ToSongChoice(trackLookup[id], isCorrect: false))
            .ToList();

        // Backfill if AI returned fewer valid play IDs than requested
        if (songsToPlayList.Count < songsToPlay)
        {
            var usedIds = new HashSet<string>(
                songsToPlayList.Select(s => s.Id)
                    .Concat(decoys.Select(s => s.Id))
            );
            var fillers = availableTracks
                .Where(t => !usedIds.Contains(t.Id))
                .OrderBy(_ => Random.Shared.Next())
                .Take(songsToPlay - songsToPlayList.Count)
                .Select(t => ToSongChoice(t, isCorrect: true))
                .ToList();
            songsToPlayList.AddRange(fillers);
        }

        var allChoices = songsToPlayList
            .Concat(decoys)
            .OrderBy(_ => Random.Shared.Next())
            .ToList();

        return new AiPickResult(songsToPlayList, allChoices);
    }

    static SongChoice ToSongChoice(MusicMetadata track, bool isCorrect) => new(
        Id: track.Id,
        Title: track.Title ?? "Unknown",
        Artist: track.Artist ?? "Unknown",
        Album: track.Album,
        Duration: track.Duration,
        IsCorrect: isCorrect
    );
}
