# Trucker Simulator

## Game Name (TBD)
**Trucker SImulator**

## Group Members
- Emiliano Luna, Kenny Melvin, Liam Colburn, Matthew Sprague

## Game Description

**Current Target:**
Highway Havoc is an endless runner-style truck driving simulator where players navigate a highway while managing the challenges of long-haul trucking. The player must change lanes to avoid traffic while simultaneously eating hotdogs, drinking Big Gulps, chugging road beers, and honking at other drivers. The goal is to survive as long as possible without crashing while maintaining your trucker lifestyle.

**Core Gameplay:**
- Drive automatically forward on a procedurally generated highway
- Change lanes (A/D keys) to avoid AI traffic
- Eat hotdogs (H key) to stay energized
- Drink Big Gulps (G key) to stay hydrated
- Crack open road beers (B key) for that authentic trucker experience (causes slight wobble effect)
- Honk your horn (Space) at everything
- Avoid crashing into other vehicles
- Score points based on distance traveled and time survived

**Win Condition:** Achieve the highest score possible
**Lose Condition:** Crash into another vehicle

## General Goals for Each Person

### [Person 1 - Gameplay Programming]
- Implement player controller with lane changing mechanics
- Create collision detection system
- Develop scoring system (distance + time tracking)
- Handle game over/restart logic
- Implement trucker activities (eating, drinking, honking)

### [Person 2 - AI & Procedural Generation]
- Create AI traffic system with random car spawning
- Implement traffic vehicle movement and pathfinding
- Design procedural road/map generation system
- Develop road network with intersections and highways
- Place environmental props (buildings, trees, street lights)

### [Person 3 - UI/UX & Polish]
- Design and implement game UI (HUD, inventory, score display)
- Create game over screen with restart functionality
- Add visual feedback for player actions
- Implement sound effects and audio management
- Create particle effects (explosions, etc.)
- Polish camera work and visual presentation

### [Person 4 - Art & Animation] *(if applicable)*
- Create 3D models for truck and traffic vehicles
- Design environment assets (buildings, trees, road props)
- Implement vehicle animations
- Create visual effects for activities (eating, drinking)
- Design road textures and materials
- Model street furniture and scenery

## Current Development Status

**Completed:**
- Player controller with lane changing
- Trucker activities (eat, drink, honk)
- Basic collision detection
- Score tracking system

**In Progress:**
- Procedural map generation
- UI implementation
- Audio integration

**Planned:**
- Visual effects and polish
- Additional gameplay features
- Main menu and settings
- High score system

## Controls

| Key | Action |
|-----|--------|
| **A** | Change to left lane |
| **D** | Change to right lane |
| **H** | Eat hotdog 🌭 |
| **G** | Drink Big Gulp 🥤 |
| **B** | Road beer 🍺 |
| **Space** | Honk horn 📯 |
| **R** | Restart after game over |

## Technical Details

**Engine:** Unity 3D
**Platform:** PC (Windows)
**Language:** C#
**Version Control:** Git/GitHub

## Scripts Overview

- `TruckPlayerController.cs` - Main player movement and activities
- `TruckCollision.cs` - Collision detection and game over logic
- `ScoreTracker.cs` - Distance and time tracking
- `TrafficSystem.cs` - AI traffic spawning and management
- `ProceduralMapGenerator.cs` - Road network generation
- `TruckerUI.cs` - User interface management

## Installation & Setup

1. Clone this repository
2. Open project in Unity (version 2021.3 or newer recommended)
3. Open the main scene
4. Press Play to start the game

## Future Enhancements

- Multiple truck types to unlock
- Day/night cycle
- Weather effects (rain, fog)
- Additional traffic types (motorcycles, buses)
- Power-ups and bonuses
- Leaderboard system
- Mobile version support
