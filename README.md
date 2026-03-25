# ModularMiniGameFramework_m0 — MetroQuest SP (Milestone 1)

Unity project for a **modular multi–mini-game framework**, evolved from the original **Milestone 0** architecture validation into **MetroQuest SP Milestone 1**: expanded persistence, progression plumbing, session/reward pipeline, and config-ready ScriptableObjects—while keeping the **dummy mini-game** playable end-to-end.

Focus: **structure, separation of concerns, save versioning, and scalable mini-game entry** (not final MetroQuest content).

---

## Milestone 1 — What Was Added (Summary)

| Area | Change |
|------|--------|
| **Save** | `SaveData` v2: `stars`, nested `ProgressionSaveData`, `HubSaveData`, `CafeSaveData`, `MaintenanceSaveData`, `MapSaveData`; migration from legacy v0/v1 |
| **Managers** | `ProgressionManager`, `RewardManager`, `MiniGameSessionManager` (all DDOL singletons on AppRoot) |
| **Session / rewards** | `MiniGameResultData`; dummy flow uses `MiniGameSessionManager` + per-click `RewardManager.GrantCoins` |
| **Constants / enums** | `Core.Constants` (`MiniGameIds`, `HubAreaIds`, `StationIds`); `RewardType`, `UpgradeType` enums |
| **Config** | `MiniGameDefinitionSO`, `HubAreaDefinitionSO`, `StationDefinitionSO` under `Assets/Configs/ScriptableObjects/` |
| **Folders** | `Gameplay/…`, `UI/Screens/…`, `Art/…`, `Audio/Music|SFX`, `Core/Systems`, `Core/Constants`, etc. (see structure below) |
| **AppRoot** | Ensures all core components via `EnsureComponent<T>()` |
| **Git** | `.gitignore` extended for Cursor-generated docs (e.g. `PROJECT_FLOW_DOCUMENT.md`); no `.gitkeep` policy |

**Still lightweight in M1:** ScriptableObject **assets** are optional until you wire the hub; `hub` / `cafe` / `maintenance` / `map` save slices are **persisted** but most **gameplay systems** do not write them yet except progression lists via `ProgressionManager`.

---

## Project Overview

- **Hub** scene — menu, economy UI, launch mini-game.
- **DummyGameplay** scene — **QuickClicker** (clicker + result panel + return to Hub).
- **AppRoot** — single persistent bootstrap with managers and (on prefab) **AudioManager** + AudioSources.

Mini-games should use **RewardManager** / **MiniGameSessionManager** rather than touching save JSON directly.

---

## Project Structure (Milestone 1)

```
Assets
├── Art/                    (UI, Characters, Environment, Icons, VFX + legacy Sprites)
├── Audio/                  (clips; Music/, SFX/ for organization)
├── Configs/
│   └── ScriptableObjects/  (MiniGameDefinitionSO, HubAreaDefinitionSO, StationDefinitionSO — namespace Core.Config)
├── Core/
│   ├── Constants/          (MiniGameIds, HubAreaIds, StationIds)
│   ├── Data/               (SaveData, nested save types, MiniGameResultData, SceneEntryData, …)
│   ├── Enums/              (CurrencyType, RewardType, UpgradeType)
│   ├── Interfaces/       (IMiniGame — optional contract)
│   ├── Managers/           (AppRoot, SaveManager, EconomyManager, GameHandler, AudioManager,
│   │                        ProgressionManager, RewardManager, MiniGameSessionManager)
│   ├── Services/           (SceneLoader)
│   ├── Systems/            (placeholder for future systems)
│   ├── Utilities/
│   └── Prefabs/            (AppRoot prefab)
├── Gameplay/               (Common, Hub, Map, Cafe, Maintenance — scripts/prefabs/SO placeholders)
├── MiniGames/
│   └── DummyMiniGame/      (QuickClicker + UI)
├── UI/
│   ├── Hub/                (HubMenuController, GameButton, HubTopBarController, …)
│   ├── Screens/            (Hub, Map, Results, Popups — placeholders)
│   └── Shared/             (CurrencyDisplay, RewardPopup, …)
├── Scenes/                 (Hub, DummyGameplay, …)
├── Resources/
├── StreamingAssets/
└── ExtractedAssets/        (third-party; trim for Android size if needed)
```

---

## Scenes

| Scene | Role |
|-------|------|
| **Hub** | Main menu; **AppRoot**; currency display; buttons to open **DummyGameplay** (ensure **scene name** in **GameHandler** / **HubTopBarController** matches build: **`DummyGameplay`**, not legacy `DummyMiniGame`). |
| **DummyGameplay** | **QuickClicker**; spawns **AppRoot** prefab if missing (direct play). |

**Build Settings:** Include **Hub** and **DummyGameplay** in the correct order for your entry flow.

---

## Core Systems (Milestone 1)

### AppRoot
- Singleton, **DontDestroyOnLoad**
- Adds if missing: **GameHandler**, **SceneLoader**, **SaveManager**, **EconomyManager**, **ProgressionManager**, **RewardManager**, **MiniGameSessionManager**
- Prefab often also has **AudioManager** + **AudioSources** (not added by code)

### SaveManager
- `Application.persistentDataPath` + **`save.json`**
- **Current save version: 2** (Milestone 1); migrates **0 → 1 → 2**
- **`SaveData.EnsureIntegrity`** after load/save so nested lists/objects are never null

### EconomyManager
- Soft/hard currency; syncs with `SaveData`; **`OnCurrencyChanged`**
- Unchanged public API; works with expanded save (SaveManager writes full JSON)

### ProgressionManager
- Reads/writes **`SaveData.progression`** (unlocked areas, stations, mini-games, completed mini-games)
- Saves after mutations; **`OnProgressionChanged`**

### RewardManager
- **`GrantCoins`** → EconomyManager (soft currency)
- **`GrantStars`** → `SaveData.stars` + Save
- **`GrantMiniGameRewards(MiniGameResultData)`**

### MiniGameSessionManager
- **`StartSession(miniGameId, levelIndex)`** / **`EndSession(result)`**
- **`EndSession`** calls **RewardManager** for non-zero coin/star fields in result
- Events: **`OnSessionStarted`**, **`OnSessionEnded`**

### GameHandler
- Hub **scene catalog** (`SceneEntryData` list); **`OneClickResult`** for QuickClicker dev mode
- Not responsible for progression

### SceneLoader
- **`LoadSceneByName(string)`**

---

## Save Data (v2) — Sketch

```json
{
  "saveVersion": 2,
  "softCurrency": 0,
  "hardCurrency": 0,
  "stars": 0,
  "progression": { "unlockedAreas": [], "unlockedStations": [], "unlockedMiniGames": [], "completedMiniGames": [] },
  "hub": { "unlockedHubZones": [], "constructedAreas": [], "stationVisualLevel": 0 },
  "cafe": { ... },
  "maintenance": { ... },
  "map": { "currentStationId": "", "unlockedStationIds": [] }
}
```

Legacy files with only currency migrate automatically.

---

## Dummy Mini-Game Flow (QuickClicker)

1. **Start:** `MiniGameSessionManager.StartSession(MiniGameIds.QuickTap, 0)`
2. **Each click:** `RewardManager.GrantCoins(rewardPerClick)` (live wallet + save, same feel as before)
3. **Result / leave:** `EndSession` with **`MiniGameResultData`** (`coinsEarned` / `starsEarned` = **0** so session does not double-grant; **`score`** = run total for listeners)
4. **Return to Hub:** `SceneLoader`
5. **OnDestroy:** ends Quick Tap session if scene unloads (avoids stuck DDOL session)

Subscribe to **`OnSessionEnded`** elsewhere when you want **ProgressionManager** updates (not wired automatically in M1).

---

## ScriptableObjects (Config Base)

Create assets: **Right-click → Create → MetroQuest → …**  
Suggested folders: `Assets/Configs/ScriptableObjects/MiniGames/`, `HubAreas/`, `Stations/` (or one `MetroQuest/` folder).  
**Runtime wiring to hub is optional** until you replace/extend **GameHandler** / **HubMenuController**.

---

## Android Build

1. Unity **2022 LTS** (or your project version)
2. **File → Build Settings → Android**
3. Scenes: **Hub**, **DummyGameplay** (and others as needed)
4. **Player Settings:** package name, min SDK, IL2CPP/Mono per team standard
5. **Build** / **Build And Run**

**Checks:** `persistentDataPath` save path; trim unused **ExtractedAssets** if APK size or plugin conflicts appear.

---

## Validation Checklist (Quick)

1. Play **Hub** → currency loads  
2. Open **DummyGameplay** → click → coins increase  
3. Result / **Return to Hub** → currency persisted  
4. Inspect **`save.json`** → `saveVersion` **2**, nested sections present  
5. (Optional) Call **ProgressionManager** unlock from debug → lists appear in save  
6. Android smoke test on device  

---

## Future Expansion

- Wire **MiniGameDefinitionSO** (and friends) into hub / map UI  
- **Hub / cafe / maintenance / map** managers reading their save slices  
- Backend sync, analytics, more mini-games under **MiniGames/** or **Gameplay/**

---

## Milestone Tags

- **m0** — Initial architecture validation (hub + dummy + economy + save v1)  
- **Milestone 1 (MetroQuest SP)** — Expanded save, progression/reward/session pipeline, SO config base, QuickClicker on new stack  

---

*README updated for MetroQuest SP Milestone 1.*
