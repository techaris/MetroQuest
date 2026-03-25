# Modular Mini-Game Framework — Project Flow Document (MetroQuest SP Milestone 1)

**Purpose:** Technical reference for architecture, initialization order, data flow, and key interactions after **Milestone 1** (expanded save, progression, rewards, session lifecycle, config SOs).

---

## 1. Architecture Overview

Unity project implementing a **modular multi–mini-game framework** with:

- Persistent save (**JSON**, **versioned**, **Milestone 1** nested structure)
- **EconomyManager** (soft/hard currency) + **RewardManager** (coins/stars entry point)
- **ProgressionManager** (progression slice of save)
- **MiniGameSessionManager** (standard start/end + **MiniGameResultData**)
- Scene-based mini-games communicating via shared **DontDestroyOnLoad** managers
- Bootstrap for **direct scene play** (e.g. **DummyGameplay** without Hub first via **QuickClicker** + **AppRoot** prefab)

---

## 2. Core Bootstrap: AppRoot

**File:** `Assets/Core/Managers/AppRoot.cs`

**Behavior:**

- Singleton; duplicate **AppRoot** destroys itself
- **DontDestroyOnLoad**
- **`EnsureComponent<T>()`** adds missing components on the **same GameObject**:
  - **GameHandler**, **SceneLoader**, **SaveManager**, **EconomyManager**, **ProgressionManager**, **RewardManager**, **MiniGameSessionManager**

**Prefab:** Often includes **AudioManager** and **AudioSource** components (not added by `AppRoot` code).

**Typical flow:**

- **Hub** has **AppRoot** → all managers available immediately
- **DummyGameplay** only → **QuickClicker** `Awake` instantiates **AppRoot prefab** if `AppRoot.Instance == null`

---

## 3. Initialization Order (Critical)

Unity: **Awake** (all) → **OnEnable** → **Start** (all) → **Update**

### 3.1 Hub as first scene

| Order | Component | Method | Action |
|-------|-----------|--------|--------|
| 1 | AppRoot | Awake | Instance, DDOL, ensure all managers |
| 2 | SaveManager | Awake | `Load()` → JSON → `SaveData.EnsureIntegrity` → migrate if needed |
| 3 | Progression / Reward / Session managers | Awake | Singleton registration only |
| 4 | EconomyManager | Start | `RefreshFromSave()` from `CurrentSaveData` |
| 5 | CurrencyDisplay | OnEnable | Wait for EconomyManager → subscribe |

### 3.2 DummyGameplay direct play

| Order | Component | Method | Action |
|-------|-----------|--------|--------|
| 1 | QuickClicker | Awake | Instantiate **AppRoot prefab** if needed |
| 2 | AppRoot | Awake | Ensure managers |
| 3 | SaveManager | Awake | `Load()` |
| 4 | EconomyManager | Start | `RefreshFromSave()` |
| 5 | QuickClicker | Start | `TryStartPlaySession()` → **MiniGameSessionManager.StartSession** |

**Rule:** After **SaveManager.Awake**, **`CurrentSaveData`** is usable; **EconomyManager.Start** reads currency.

---

## 4. SaveManager

**File:** `Assets/Core/Managers/SaveManager.cs`

**Responsibilities:**

- `Application.persistentDataPath` + **`save.json`**
- **Current save version = 2** (Milestone 1)
- Migration: **legacy 0 → 1 → 2**
- **`SaveData.EnsureIntegrity`** on load and before every **Save()**
- Public API unchanged: **`CurrentSaveData`**, **`Load()`**, **`Save()`**, **`ResetSave()`**, **`GetSaveFilePath()`**

**Version meaning:**

- **1** — Milestone 0 style on disk (currency-focused)
- **2** — Full **SaveData** tree (stars + progression + hub + cafe + maintenance + map)

---

## 5. Save Data Structure (Milestone 1)

**Root:** `Assets/Core/Data/SaveData.cs`

Nested types:

- `ProgressionSaveData` — lists: unlocked areas, stations, mini-games, completed mini-games
- `HubSaveData` — hub zones, constructed areas, `stationVisualLevel`
- `CafeSaveData` — levels + upgrades + tutorial flag
- `MaintenanceSaveData` — levels + tutorial
- `MapSaveData` — `currentStationId`, `unlockedStationIds`

**`SaveData.EnsureIntegrity(SaveData)`** — null-safe nested objects and **List&lt;string&gt;** fields.

**Example JSON (illustrative):**

```json
{
  "saveVersion": 2,
  "softCurrency": 100,
  "hardCurrency": 0,
  "stars": 0,
  "progression": { ... },
  "hub": { ... },
  "cafe": { ... },
  "maintenance": { ... },
  "map": { ... }
}
```

---

## 6. EconomyManager

**File:** `Assets/Core/Managers/EconomyManager.cs`

Unchanged role: **soft/hard** currency mirror of **`SaveData`**, **`OnCurrencyChanged`**, **`SaveManager.Save()`** on change.

Does **not** own **stars** (**RewardManager** / save field).

---

## 7. ProgressionManager

**File:** `Assets/Core/Managers/ProgressionManager.cs`

- Reads/writes **`SaveData.progression`**
- **`SaveManager.Save()`** after successful unlock/complete mutations
- **`OnProgressionChanged`**
- Query/unlock APIs use **string ids** (align with **`Core.Constants`**)

**Not auto-wired** to **MiniGameSessionManager** in M1 — subscribe to **`OnSessionEnded`** when you want completion/unlock side effects.

---

## 8. RewardManager

**File:** `Assets/Core/Managers/RewardManager.cs`

- **`GrantCoins`** → **EconomyManager.AddSoftCurrency**
- **`GrantStars`** → **`SaveData.stars`** + **Save**
- **`GrantMiniGameRewards(MiniGameResultData)`** — uses `coinsEarned` / `starsEarned` (non-negative)

---

## 9. MiniGameSessionManager

**File:** `Assets/Core/Managers/MiniGameSessionManager.cs`

- **`StartSession(string miniGameId, int levelIndex = 0)`** — rejects if already active
- **`EndSession(MiniGameResultData result)`** — clones result, **`GrantMiniGameRewards`** if result non-null, **`OnSessionEnded`**
- **DontDestroyOnLoad** singleton

**Design note (QuickClicker):** Per-click coins go through **RewardManager.GrantCoins**; **EndSession** payload uses **`coinsEarned` / `starsEarned` = 0** so session end does **not** double-apply coins.

---

## 10. MiniGameResultData

**File:** `Assets/Core/Data/MiniGameResultData.cs`

Fields: `miniGameId`, `success`, `coinsEarned`, `starsEarned`, `score`, `levelIndex`

---

## 11. GameHandler

**File:** `Assets/Core/Managers/GameHandler.cs`

- **Hub scene catalog**: **`SceneEntryData`** (sceneName, displayName, icon)
- **`OneClickResult`** — QuickClicker one-click result mode
- **No progression logic**

---

## 12. SceneLoader

**File:** `Assets/Core/Services/SceneLoader.cs`

**`LoadSceneByName(string)`** — must match **File → Build Settings** scene names (e.g. **`DummyGameplay`**, **`Hub`**).

---

## 13. Constants & Config SOs

**Constants:** `Assets/Core/Constants/` — **MiniGameIds**, **HubAreaIds**, **StationIds** (`mq.*` string ids)

**Enums:** `RewardType`, `UpgradeType` (optional / may be unused until features consume them)

**ScriptableObjects:** `Assets/Configs/ScriptableObjects/` — **MiniGameDefinitionSO**, **HubAreaDefinitionSO**, **StationDefinitionSO** (`Core.Config` namespace). **Create → MetroQuest → …** in Editor. Not required at runtime until wired to UI/catalog.

---

## 14. QuickClicker (DummyGameplay)

**File:** `Assets/MiniGames/DummyMiniGame/Scripts/QuickClicker.cs`

1. **Awake** — spawn **AppRoot prefab** if needed  
2. **Start** — **`MiniGameSessionManager.StartSession(MiniGameIds.QuickTap, 0)`**  
3. **Click** — **`RewardManager.GrantCoins(rewardPerClick)`** (not direct EconomyManager)  
4. **ShowResultDisplay** — **`TryEndPlaySession`** → **`EndSession`** with snapshot (`score` = `currentGained`, coin/star grants 0 in payload)  
5. **HandleReturnToHub** — **`TryEndPlaySession`** if still active, then **SceneLoader**  
6. **CloseResult** (one-click mode) — reset stats, **`StartSession`** again  
7. **OnDestroy** — end Quick Tap session if still active (prevents stuck session on DDOL manager)

Also: **GameHandler.OneClickResult**, **AudioManager**, **RewardPopupPool**, etc.

---

## 15. Data Flow Summary (Click → Wallet)

```
QuickClicker click
  → RewardManager.GrantCoins(n)
    → EconomyManager.AddSoftCurrency(n)
      → SaveData.softCurrency updated
      → SaveManager.Save()  (EnsureIntegrity → full JSON)
      → OnCurrencyChanged
  → CurrencyDisplay refreshes (if present in scene)
```

---

## 16. Session End Flow (Dummy)

```
ShowResultDisplay / Return to Hub (if session active)
  → MiniGameSessionManager.EndSession(MiniGameResultData)
    → GrantMiniGameRewards (no extra coins if amounts are 0)
    → OnSessionEnded.Invoke(result)
```

---

## 17. Scene Flow (Current)

```
Hub
  → AppRoot + managers
  → GameHandler.AvailableScenes / HubTopBar → SceneLoader.LoadSceneByName("DummyGameplay")

DummyGameplay
  → QuickClicker + optional AppRoot spawn
  → Session start → clicks → RewardManager → result / EndSession → LoadScene("Hub")
```

**Inspector pitfall:** **HubTopBarController** default string may still say **`DummyMiniGame`**; build scene is **`DummyGameplay`** — align in Inspector.

---

## 18. File Locations (Key Paths)

| Asset | Path |
|-------|------|
| AppRoot | `Assets/Core/Managers/AppRoot.cs` |
| SaveManager | `Assets/Core/Managers/SaveManager.cs` |
| EconomyManager | `Assets/Core/Managers/EconomyManager.cs` |
| ProgressionManager | `Assets/Core/Managers/ProgressionManager.cs` |
| RewardManager | `Assets/Core/Managers/RewardManager.cs` |
| MiniGameSessionManager | `Assets/Core/Managers/MiniGameSessionManager.cs` |
| SaveData + nested | `Assets/Core/Data/*.cs` |
| MiniGameResultData | `Assets/Core/Data/MiniGameResultData.cs` |
| SceneLoader | `Assets/Core/Services/SceneLoader.cs` |
| QuickClicker | `Assets/MiniGames/DummyMiniGame/Scripts/QuickClicker.cs` |
| Definition SOs | `Assets/Configs/ScriptableObjects/*DefinitionSO.cs` |
| AppRoot prefab | `Assets/Core/Prefabs/AppRoot.prefab` |

---

## 19. Save File Location

- **Path:** `Application.persistentDataPath/save.json`
- **Windows:** `AppData/LocalLow/<CompanyName>/<ProductName>/save.json`
- **Android:** app-specific files directory under package

---

## 20. Common Pitfalls (Milestone 1)

1. **Stuck session:** Next **StartSession** rejected — ensure **EndSession** on leave/result or **QuickClicker OnDestroy** for Quick Tap.  
2. **Double coins:** Do not put per-click amounts **and** non-zero **`coinsEarned`** in the same **EndSession** unless you intend two grants.  
3. **Wrong scene name:** **GameHandler** / **HubTopBar** must use **`DummyGameplay`** if that is the built scene.  
4. **Currency 0 on direct play:** Assign **AppRoot prefab** on **QuickClicker**; wait for **EconomyManager** in UI.  
5. **Progression never updates:** Nothing subscribes to **OnSessionEnded** until you add a listener (M1 by design).

---

## 21. Dependency Graph (Simplified)

```
AppRoot
  ├── GameHandler
  ├── SceneLoader
  ├── SaveManager
  ├── EconomyManager
  ├── ProgressionManager
  ├── RewardManager
  └── MiniGameSessionManager

QuickClicker
  ├── AppRoot (prefab)
  ├── RewardManager (per-click coins)
  ├── MiniGameSessionManager (session lifecycle)
  ├── SceneLoader
  ├── GameHandler (OneClickResult)
  └── RewardPopupPool / AudioManager

CurrencyDisplay → EconomyManager
```

---

*Document updated for MetroQuest SP Milestone 1.*
