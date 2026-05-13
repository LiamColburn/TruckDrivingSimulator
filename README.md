# Highway Havoc — Truck Driving Simulator

An endless runner where you are a trucker on the highway. Change lanes to dodge traffic, eat hotdogs, chug road beers, and survive as long as possible.

---

## Team

| Name | Role |
|------|------|
| Emiliano Luna | UI/UX & Polish |
| Matthew Sprague | AI & Procedural Generation |
| Liam Colburn | Gameplay Programming |
| Kenny Melvin | Art & Animation |

---

## Game Description

Highway Havoc is a score-attack endless runner. The truck sits stationary at a fixed world position while the road, traffic, and environment scroll toward it. The player changes lanes to avoid oncoming vehicles and performs trucker activities to pass the time. Score is calculated from time survived plus distance traveled (simulated at 20 units/sec). A crash ends the run; pressing R restarts immediately.

---

## Controls

| Key | Action |
|-----|--------|
| **A** or **Left Arrow** | Change to left lane |
| **D** or **Right Arrow** | Change to right lane |
| **H** | Eat hotdog — plays hotdog animation video |
| **G** | Drink Big Gulp — plays drink animation video |
| **B** | Crack open road beer — causes X-axis wobble for 5 seconds + FOV pulse on camera |
| **Space** | Honk horn |
| **Escape** | Pause / unpause |
| **R** | Restart after crash (also works from pause menu) |
| **Tab** | Toggle controls overlay (auto-hides after 5 seconds) |

---

## Current Architecture

### TruckPlayerController.cs
Central player logic. Manages three lanes at X positions `{-4.66, 0, 4.66}`. Lane changes are smooth lerps over `laneChangeDuration` (2.5 s). On lane change, calls `dashOverlay.OnLaneChange(direction)` and `cameraFollow.TriggerLaneTilt(direction)`. Activities (H/G/B/Space) consume inventory items, play audio via `AudioManager`, spawn `ActivityParticles` bursts, and call into `TruckerVideoOverlay` for the video animations. `videoOverlay` and `dashOverlay` are auto-found via `FindFirstObjectByType` at `Start()` if not assigned in the Inspector. Uses Legacy Input System (`Input.GetKeyDown`).

### TruckCollision.cs
Detects collisions with objects tagged **Vehicle**. On crash: plays crash audio, spawns a `TruckExplosionEffect` at the truck position, builds a code-driven game-over panel via `UITheme.EnsureGameOverPanel()`, and freezes time (`Time.timeScale = 0`). R-key restart (New Input System — `Keyboard.current.rKey.wasPressedThisFrame`) sets `MainMenuUI.SkipOnNextLoad = true` then reloads the scene. This is the one place New Input System is used; everywhere else is Legacy.

### AudioManager.cs
Singleton with `DontDestroyOnLoad`. Exposes `PlayHorn()`, `PlayEat()`, `PlayDrink()`, `PlayBurp()`, `PlayCrash()`. Five AudioSources are wired in the scene with their respective mp3 clips. The horn source uses `PlayOneShot` with a configurable volume; the rest use a shared `Play(AudioClip)` helper.

### TruckerVideoOverlay.cs
Creates a Canvas, RenderTexture (1920×1080), and VideoPlayer entirely at runtime in `SetupVideoOverlay()`. `PlayHotdogVideo()`, `PlayBeerVideo()`, `PlayBigGulpVideo()`, `PlaySmokingVideo()` each null-check their respective `VideoClip` field before playing. The canvas fades in at `videoAlpha` (0.8) when a video plays and hides automatically via `videoPlayer.loopPointReached`. Currently only `hotdogVideo` is wired in the prefab.

### TruckerDash.cs
Renders two Image components on a shared HUD canvas: a fullscreen dash overlay (`Truck_Dash.png`) at sibling index 0, and a steering wheel image at bottom-center (offset configurable via `wheelPosition`). The wheel rotates via `Mathf.Lerp` toward `targetWheelAngle` (set by `OnLaneChange(int direction)` which TruckPlayerController calls). Returns to 0° when `TruckPlayerController.IsChangingLanes()` is false.

### TruckerInventoryUI.cs
Top-left HUD panel auto-built at runtime if Inspector fields are null. Polls `TruckPlayerController.GetHotdogsRemaining()`, `GetBigGulpsRemaining()`, `GetRoadBeersRemaining()` each Update and renders them with emoji labels. Items go grey when count reaches 0.

### TruckerUI.cs
General-purpose HUD manager. Renders a speedometer and score readout. Connects to `TruckPlayerController` and `ScoreTracker` via `FindFirstObjectByType` fallbacks. Lives on the `UI_Manager` GameObject alongside several other UI scripts.

### ScoreTracker.cs
Simulates distance traveled at `distancePerSecond = 20f` (matching road/traffic speed) since the truck never actually moves. Final score formula: `(timeSurvived × pointsPerSecond) + (distanceTraveled × distanceMultiplier)`. Auto-builds a score display panel in the top-right corner.

### TrafficSpawner.cs
Object pool of pre-instantiated vehicles. Vehicles spawn 40 units ahead and despawn 30 units behind the player. `vehicleSpeed = 20f` must stay in sync with `InfiniteRoadManager.roadSpeed`. Lane X positions `{-4.66, 0, 4.66}` must stay in sync with `TruckPlayerController`. All pool vehicles are tagged **Vehicle** at creation so `TruckCollision` can detect them.

### InfiniteRoadManager.cs
Spawns and recycles road chunk prefabs to create the illusion of forward motion. `roadSpeed = 20f`. Requires `roadChunkPrefab` (wired to `Assets/Terrains/RoadChunk.prefab`) and `playerTruck` (wired to the Cube's Transform in the scene). Keeps `chunksAhead = 3` and `chunksBehind = 1` chunks alive at a time, each 100 units long.

### CameraFollow.cs (file: Camerafollow.cs)
Attaches to Main Camera. Auto-finds the object tagged **Player**. In `LateUpdate`, applies additive deltas over the base follow position: idle Y/X bob (sine wave), lane tilt (`TriggerLaneTilt(int direction)` — lerps `targetTilt`), and road-beer FOV pulse (`TriggerRoadBeerEffect()` — sine pulse on `cam.fieldOfView`). Called by `TruckPlayerController` on lane change and road beer.

### MainMenuUI.cs
Shown at scene start; pauses game (`Time.timeScale = 0`). Space or Enter starts the game. Static bool `SkipOnNextLoad` prevents the menu from re-showing when the scene reloads after a crash or restart. Built via `UITheme.BuildCanvas()` and `UITheme.AddImageButton()`.

### PauseMenuUI.cs
Escape toggles pause. Blocked while main menu is active or game over panel is showing. **Resume** unpauses. **Restart** sets `MainMenuUI.SkipOnNextLoad = true` then reloads. **Main Menu** reloads without setting the flag (so the menu appears again).

### UITheme.cs
Static factory for all in-game UI. `BuildCanvas(name, sortOrder)` creates Screen Space Overlay canvases. `AddImageButton()` loads button sprites from `Resources/UI/Buttons/` and strips white backgrounds via pixel manipulation — requires textures to have **Read/Write Enabled** in Import Settings. `EnsureEventSystem()` handles the New Input System vs Legacy InputModule swap via reflection. `EnsureGameOverPanel()` builds the crash overlay.

### ControlsUI.cs
Tab-key toggle (Legacy Input). Shows on scene load, auto-hides after 5 seconds. Displays all controls in a semi-transparent overlay panel.

### ActivityParticles.cs
No prefab required. Spawns temporary GameObjects with `ParticleSystem` components for eat bursts, drink bursts, and horn smoke puffs. Shader fallback chain: `URP/Particles/Unlit` → `Sprites/Default` → `Particles/Standard Unlit` → Legacy Particles shader.

### TruckExplosionEffect.cs
Code-driven explosion spawned by `TruckCollision` on crash. Creates a URP `Particles/Unlit` material with a procedural soft-circle texture. Three particle layers: fire (root), smoke (child), sparks with world collision (child). Self-destructs via `Destroy(gameObject, 3f)`.

---

## Scene Structure

| GameObject | Key Components | Notes |
|------------|----------------|-------|
| `Main Camera` | `Camera`, `CameraFollow` + 2 other components | Auto-finds Player tag |
| `Cube` (truck) | `TruckCollision`, `ScoreTracker`, `ActivityParticles`, `TruckPlayerController` | Stationary; world moves past it |
| `UI_Manager` | `TruckerUI`, `ControlsUI`, `TruckerInventoryUI`, `TruckerDash` | All HUD scripts live here |
| `AudioManager` | `AudioManager` | DontDestroyOnLoad singleton |
| `RoadManager` | `InfiniteRoadManager` | roadChunkPrefab + playerTruck wired |
| `TrafficSpawner` | `TrafficSpawner` (prefab instance) | Object pool; vehicle prefabs must be assigned |
| `VideoOverlayManager` | `TruckerVideoOverlay` (prefab instance) | hotdogVideo wired; others null |
| `MainMenuUI` | `MainMenuUI` | Root GO; builds its own canvas |
| `PauseMenuUI` | `PauseMenuUI` | Root GO; builds its own canvas |
| `Directional Light` | `Light` | Scene lighting |
| `Global Volume` | `Volume` | URP post-processing |

---

## Development Status

**Done:**
- Player controller: lane changing (3 lanes), all trucker activities (H/G/B/Space)
- Collision detection and game over flow (explosion + panel + R-restart)
- Infinite road scrolling (InfiniteRoadManager + RoadChunk prefab wired)
- Traffic spawner with object pooling
- Full audio system (5 clips wired in scene)
- Dashboard overlay (dash image + steering wheel rotation)
- Video overlay system (infrastructure complete; hotdog video wired)
- All menu screens: main menu, pause menu, game over panel
- Score tracker (time + simulated distance)
- Inventory HUD (hotdog/bigGulp/beer counts)
- Camera polish: idle bob, lane tilt, FOV pulse on road beer
- Particle effects: activity bursts + explosion on crash
- Controls overlay (Tab, auto-hides)

**In Progress:**
- Video clips for beer, big gulp, and smoking not yet assigned in VideoOverlayManager prefab
- TrafficSpawner vehicle prefabs need to be assigned in Inspector (pool size depends on this)

**Known Issues:**
- `TruckCollision` uses New Input System for R key while everything else uses Legacy Input — may fail if Input System package config changes
- UITheme button sprites require Read/Write enabled on textures; missing this causes a silent black button
- If `AudioManager` is destroyed between scenes (e.g., during editor stop/start), singletons in other scripts that cached `.Instance` will throw null refs on next play

---

## Important Technical Notes

1. **URP only** — All shaders and particles use URP variants. Standard/Legacy shaders will show pink in play mode.

2. **Stationary player / moving world** — The truck (`Cube`) never moves. `InfiniteRoadManager` scrolls road chunks backward; `TrafficSpawner` moves vehicles at `vehicleSpeed = 20f`. Both values and the `distancePerSecond` in `ScoreTracker` must stay at `20f` or the speed/score will desync.

3. **Lane X positions must match in three places** — `TruckPlayerController.lanePositions`, `TrafficSpawner.laneXPositions`, and any future lane-aware system must all use `{-4.66f, 0f, 4.66f}`.

4. **Always commit `.meta` files** — Unity generates GUIDs from `.meta` files. If a `.meta` is not committed, every developer gets a different auto-generated GUID and asset references in prefabs/scenes from other machines will show "Missing Script" at runtime. This was the root cause of the hotdog video not playing.

5. **TruckerVideoOverlay GUID** — `Assets/Scripts/TruckerVideoOverlay.cs.meta` is now committed with GUID `3a0c3d8683410404589a2b96f42f45f8`. The `VideoOverlayManager.prefab` and `SampleScene.unity` reference this GUID. Do not delete or regenerate this meta file.

6. **New Input System vs Legacy** — `TruckCollision.cs` uses `Keyboard.current.rKey.wasPressedThisFrame` (New Input System). Every other script uses `Input.GetKeyDown` (Legacy). Both packages are installed. Keep it this way or update all scripts together.

7. **UITheme texture Read/Write** — Button PNGs in `Assets/Resources/UI/Buttons/` must have **Read/Write Enabled** in their Texture Import Settings. `UITheme.AddImageButton()` does pixel manipulation to remove white backgrounds; it silently produces a black button if the texture is not readable.

8. **TrafficSpawner pool** — Vehicle prefabs must be assigned in the TrafficSpawner Inspector. The pool pre-instantiates them at startup; an empty list means no traffic spawns and the game is trivially easy.

---

## Setup

1. Clone this repository
2. Open the project in **Unity 6** (6000.x)
3. Open `Assets/Scenes/SampleScene.unity`
4. Press **Play**

The main menu appears automatically. Press **Space** or **Enter** to start.
