# UI Setup Instructions for CSE 457 Final Project
## Nathnael's UI/Menu Responsibilities

---

## Scripts Overview
| Script | Purpose |
|---|---|
| `GameManager.cs` | Central state: difficulty, score, speed, scene loading |
| `UIManager.cs` | All UI panels: main menu, HUD, game over |

---

## Unity Setup Steps

### 1. Add Scenes to Build Settings
- File → Build Settings → Add Open Scenes
- You need two scenes: **MainMenu** and **GameScene**
- Or you can use one scene with panel switching (easier for now)

### 2. Create a UIManager GameObject
- In your scene hierarchy: **GameObject → Create Empty** → name it `UIManager`
- Drag `UIManager.cs` onto it
- Do the same for `GameManager.cs` on a `GameManager` empty object

### 3. Create the Canvas + Panels
In the Hierarchy, create:
```
Canvas
├── MainMenuPanel
│   ├── Title (TextMeshPro)
│   ├── PlayButton
│   ├── EasyButton
│   ├── MediumButton
│   ├── HardButton
│   ├── SelectedDifficultyText (TMP)
│   └── QuitButton
├── HUDPanel
│   ├── ScoreText (TMP)
│   └── TimerText (TMP)
└── GameOverPanel
    ├── FinalScoreText (TMP)
    ├── ReplayButton
    └── MainMenuButton
```

### 4. Wire Up References in UIManager Inspector
- Drag each Panel/Button/Text into the corresponding slot on the UIManager component

### 5. TextMeshPro Setup
- When you first add a TMP component, Unity will prompt you to import TMP Essentials — click Import

### 6. Calling Game Over from Player Script
When the player falls into the pit, call:
```csharp
GameManager.Instance.TriggerGameOver();
```
This will automatically show the Game Over screen with the final score.

### 7. Getting Obstacle Speed in Spawner Script (for teammates)
```csharp
float speed = GameManager.Instance.ObstacleSpeed;
```

---

## Difficulty Settings (Configurable in Inspector)
| Difficulty | Start Speed |
|---|---|
| Easy | 3 units/sec |
| Medium | 5 units/sec |
| Hard | 8 units/sec |

Speed increases by 0.5 units/sec over time (adjustable in GameManager inspector).
