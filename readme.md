# 🎵 TuneGames

> **Can you name that tune?** A .NET MAUI music trivia game powered by AI 🤖🎶

TuneGames reads your device music library, uses OpenAI to pick songs for a category, plays short clips, and challenges you to identify them from a shuffled list of choices. How sharp are your ears? 👂

---

## 🎮 How It Works

```
 1️⃣  Pick a category (e.g. "80s Rock")
 2️⃣  AI selects songs from YOUR music library
 3️⃣  Listen to short clips played back-to-back
 4️⃣  Race the clock to pick the songs you heard
 5️⃣  See your score — brag to your friends 🏆
```

### Round Flow

| Phase | What Happens |
|-------|-------------|
| 🎯 **Category** | Choose or create a music category |
| 🤖 **AI Picks** | GPT-4o-mini scans your library and selects songs + decoys |
| 🎧 **Listen** | 4 clips play for 15 seconds each with a 2-second pause between |
| 🧠 **Answer** | 12 shuffled choices appear — pick the 4 you heard before time runs out! |
| 🏅 **Results** | See what you got right, missed, or guessed wrong |

---

## ✨ Features

- 🎵 **Device Music Integration** — plays real songs from your library via [Shiny.Music](https://github.com/shinyorg/music)
- 🤖 **AI-Powered Song Picking** — OpenAI selects genre-appropriate songs & convincing decoys
- 🍎 **Streaming Aware** — detects Apple Music subscriptions for expanded song access
- ⏱️ **Countdown Timer** — configurable answer time limit with auto-submit
- 📊 **Game History** — track your scores over time
- 🏷️ **Custom Categories** — create your own or use the 5 built-in defaults
- ⚙️ **Fully Configurable** — tweak songs per round, clip length, choices, timer & more
- 💾 **Local Storage** — all data persisted with [Shiny.SqliteDocumentDb](https://github.com/shinyorg/sqlitedocumentdb) (AOT-friendly, no reflection)
- 📱 **Cross-Platform** — iOS 15+ and Android 5.0+

---

## 🚀 Getting Started

### Prerequisites

| Requirement | Details |
|-------------|---------|
| 🛠️ .NET SDK | **10.0** or later |
| 📱 Target | iOS 15.0+ / Android 21.0+ |
| 🔑 OpenAI Key | Required for AI song picking |
| 🎵 Music | Songs on your device (the more the better!) |

### Setup

1. **Clone the repo**
   ```bash
   git clone https://github.com/user/TuneGames.git
   cd TuneGames
   ```

2. **Add your OpenAI API key** in `TuneGames/MauiProgram.cs`:
   ```csharp
   const string OpenAiApiKey = "sk-your-key-here";
   ```

3. **Build & run**
   ```bash
   # 🤖 Android
   dotnet build TuneGames -f net10.0-android

   # 🍎 iOS
   dotnet build TuneGames -f net10.0-ios
   ```

4. **Grant permissions** when prompted — the app needs access to your music library 🎶

---

## ⚙️ Game Settings

All settings are configurable from the in-app Settings page and persisted locally:

| Setting | Default | Description |
|---------|:-------:|-------------|
| 🎵 Songs per round | **4** | How many clips to play |
| ⏳ Clip duration | **15s** | Length of each song clip |
| 📋 Total choices | **12** | Answer options shown (songs + decoys) |
| ⏱️ Answer time limit | **30s** | Seconds to submit your picks |
| ⏸️ Pause between clips | **2s** | Silence between songs |

---

## 🏷️ Default Categories

Out of the box, TuneGames comes with:

- 🎸 **80s Rock**
- 🎤 **Pop Hits**
- 🎧 **Hip Hop**
- 🤠 **Country**
- 🎷 **Jazz**

➕ Create your own from the Category Select screen!

---

## 🏗️ Architecture

Built with **MVVM** using .NET MAUI Shell navigation and source generators.

```
📁 TuneGames/
├── 📁 Pages/
│   ├── 🏠 Main/              Home screen & quick play
│   ├── 🏷️ CategorySelect/    Browse & create categories
│   ├── 🎧 GamePlay/          Clip playback with progress
│   ├── 🧠 Answer/            Multi-select with countdown
│   ├── 🏅 Results/           Score breakdown
│   ├── ⚙️ Settings/          Tweak game parameters
│   └── 📊 History/           Past game scores
├── 📁 Services/
│   ├── 🎵 MusicService       Device music library access
│   ├── 🤖 AiSongPicker       OpenAI-powered song selection
│   ├── 🎮 GameEngine         Round orchestration & scoring
│   └── 💾 GameStore          SQLite document persistence
├── 📁 Models/
│   ├── GameModels.cs          SongChoice, GameRound, AiPickResult
│   └── Documents.cs           Category, GameSettings, GameResult
├── 📁 Converters/             UI value converters
├── AppJsonContext.cs           AOT-safe JSON serialization
└── MauiProgram.cs              DI & service registration
```

---

## 📦 Tech Stack

| Package | Purpose |
|---------|---------|
| [Shiny.Music](https://github.com/shinyorg/music) | 🎵 Device music library & playback |
| [Shiny.SqliteDocumentDb](https://github.com/shinyorg/sqlitedocumentdb) | 💾 Local document storage |
| [Shiny.Maui.Shell](https://github.com/shinyorg/shellextensions) | 🧭 Typed shell navigation with source gen |
| [Shiny.Maui.TableView](https://github.com/nicebear-dev/mauitable) | 📋 Native table/list views |
| [Microsoft.Extensions.AI.OpenAI](https://github.com/dotnet/extensions) | 🤖 OpenAI chat client |
| [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) | ⚡ MVVM source generators |

---

## 📱 Permissions

| Platform | Permission | Why |
|----------|-----------|-----|
| 🍎 iOS | `NSAppleMusicUsageDescription` | Access your music library |
| 🤖 Android | `READ_MEDIA_AUDIO` | Read audio files (Android 13+) |
| 🤖 Android | `READ_EXTERNAL_STORAGE` | Fallback for older Android |
| 🌐 Both | Internet | OpenAI API calls for song picking |

---

## 🔧 CI/CD

GitHub Actions workflows included for both platforms:

- **`build-ios.yml`** — Builds on macOS 15 with Xcode 26, publishes to TestFlight ✈️
- **`build-android.yml`** — Builds with keystore signing, ready for Google Play 🚀

Triggers: push to `main` / `v*` branches, or manual dispatch.

---

## 📝 Notes

- 🔑 **API Key** — Currently set in source code. For production, move to secure storage or a settings UI.
- 🎵 **Music Required** — You need at least as many songs on your device as the "Total Choices" setting (default 12).
- 🌐 **Internet Required** — AI song picking calls OpenAI. The app handles offline gracefully with error messages.
- 🍎 **Apple Music** — If a streaming subscription is detected, the AI can work with a broader range of songs.

---

## 📄 License

MIT

---

*Built with ❤️ and .NET MAUI*