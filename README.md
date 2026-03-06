# ModularMiniGameFramework_m0

Architecture validation milestone for a scalable **multi–mini-game Unity framework** designed to support future expansion including backend integration, login systems, GPS features, and additional mini-games.

This milestone demonstrates **clean modular architecture**, persistent economy systems, and isolated mini-game modules communicating with shared core services.

The focus of this submission is **structure, separation of concerns, and scalability**, not visual polish.

---

# Project Overview

The project contains a **Hub scene** and a **Dummy MiniGame module** connected through shared core systems.

Core systems are responsible for:

* Save persistence
* Currency/economy state
* Data versioning
* Cross-scene system availability

Mini-games operate as **isolated modules** that interact with the core systems through controlled interfaces rather than tightly coupled scene dependencies.

This architecture allows new mini-games to be added without modifying core systems.

---

# Project Structure

```
Assets
│
├── Core
│   ├── Managers
│   │    EconomyManager.cs
│   │    SaveManager.cs
│   │
│   ├── Data
│   │    SaveData.cs
│   │
│   └── Utilities
│
├── MiniGames
│   └── DummyMiniGame
│        DummyMiniGameController.cs
│
├── UI
│   ├── Hub
│   │    HubUIController.cs
│   │
│   └── Shared
│        CurrencyDisplay.cs
│
├── Scenes
│   Hub.unity
│   DummyMiniGame.unity
│
└── Configs
```

### Core

Contains shared systems used by the entire application.

Responsibilities include:

* persistent economy state
* save/load management
* shared data models

### MiniGames

Each mini-game exists as an isolated module containing its own scripts and scene.

Adding a new mini-game requires only creating a new module inside this directory.

### UI

Contains hub UI controllers and shared UI components such as currency display bindings.

### Scenes

Contains application scenes.

* **Hub** – main menu and economy display
* **DummyMiniGame** – validation mini-game module

---

# Scenes

## Hub Scene

Displays the current currency state and provides access to the mini-game.

UI includes:

* Soft currency counter
* Secondary currency counter
* Play Dummy MiniGame button
* Reset Save debug button

Currency counters are displayed within the **device safe area** to support different screen types.

---

## Dummy MiniGame Scene

A minimal mini-game module used to validate the architecture.

Flow:

1. Player presses **Complete MiniGame**
2. +10 soft currency is awarded
3. Result panel displays earned reward
4. Player returns to the Hub

The mini-game does not manipulate save files directly and instead communicates with shared core systems.

---

# Core Systems

## EconomyManager

The single source of truth for all currency values.

Responsibilities:

* managing currency state
* providing methods for currency modification
* triggering save operations when economy changes
* notifying UI systems when currency updates

Example operations include:

```
AddSoftCurrency(int amount)
GetSoftCurrency()
GetHardCurrency()
```

All currency changes flow through this manager to maintain consistency.

---

## SaveManager

Handles persistence of game data using a JSON file.

Responsibilities:

* loading save data at application start
* writing save data to disk
* resetting save data
* managing save versioning

The save file is stored using:

```
Application.persistentDataPath
```

Example location (Android):

```
/storage/emulated/0/Android/data/<package>/files/save.json
```

---

# Save Data Structure

Save data is stored as JSON and contains a **saveVersion field** to support future migrations.

Example:

```json
{
  "saveVersion": 1,
  "softCurrency": 110,
  "hardCurrency": 50
}
```

This structure allows safe upgrades if the save format evolves in future versions.

---

# Save Version Migration Strategy

The `saveVersion` field allows controlled data migrations when the save format changes.

Example approach:

1. Load save file
2. Check `saveVersion`
3. If older than current version:

   * run migration steps
   * upgrade data format
4. Save upgraded version

Pseudo-flow:

```
if (saveVersion < CURRENT_VERSION)
{
    RunMigration();
}
```

This approach ensures backward compatibility when introducing new fields or systems.

---

# MiniGame → Core Communication

Mini-games communicate with shared systems through **central services** rather than modifying save data directly.

Example flow:

```
MiniGameController
      ↓
EconomyManager
      ↓
SaveManager
      ↓
JSON save file
```

This separation ensures:

* mini-games remain isolated modules
* core systems remain centralized
* future backend sync can be implemented without modifying mini-game logic

---

# Android Build Instructions

1. Open the project in **Unity 2022 LTS**
2. Open **File → Build Settings**
3. Select **Android**
4. Ensure scenes are included in build order:

```
Hub
DummyMiniGame
```

5. Switch platform if necessary
6. Click **Build**

The generated APK demonstrates the full architecture.

---

# Validation Steps

To verify functionality:

1. Launch application
2. Observe currency counters in Hub
3. Tap **Play Dummy MiniGame**
4. Tap **Complete MiniGame (+10)**
5. Return to Hub
6. Confirm currency increased
7. Close the application completely
8. Reopen the application
9. Confirm currency persists
10. Use **Reset Save** button if needed

---

# Future Expansion

The architecture supports future features such as:

* multiple mini-games
* backend save sync
* authentication systems
* live economy balancing
* analytics
* GPS/location-based gameplay

Adding a new mini-game requires only:

1. creating a new module in `MiniGames`
2. creating a scene
3. using the shared core services

No modifications to existing mini-games are required.

---

# Milestone Tag

```
m0
```

This tag represents the **initial architecture validation milestone**.

---