## Technical Objects
GameObjects that need to exist in the scene, depending on which (e.g. main menu objects don’t exist in gameplay scenes and vice versa)

### Common
GameDataHandler
- Game data management
GameManager
- Game-wide stuff
SceneSwitchHandler
- Scene changing with transition animation
ResourceSystem
- Handles resource (ScriptableObject) loading and accessing
UIManager
- Handles Common UI for all scenes
TelemetryHandler
- Handles Firebase authentication and database access
- Also handles action telemetry timing
InputDeviceManager
- Connection to controller, sending braille data, reading IMU data
BraillePatternPlayer
- Handles playing of braille pattern sequences
BrailleTester
- Testing platform for BraillePatternPlayer
MainThreadDispatcher
- Something something unity thread

### Main Menu
MainMenuUIController
- Handles UI specific to the Main Menu

### Gameplay
LevelManager
- Level state transitions, game pausing, fish score tracking, other level info
GameplayUIController
- Handles UI specific to gameplay
PauseUIController
- Handles visibility for any pause screen elements
FishingManager
- Fishing state transitions & main fishing logic
FishTargeting
- Fish spawning/pool & targeting
FishLootTable
- Weighted loot table for fishing

## How Pattern Sequences Are Played
1. Pattern Sequence Selection

When you call PlayPatternSequence, you specify the name of the pattern sequence, which finger(s) (thumb, index, or both) should play it, and whether it should loop.

2. Pattern Lookup and Encoding

The class looks up the requested sequence from preloaded and encoded pattern lists for the thumb and index. Each sequence consists of a list of encoded Braille patterns, which are converted from the raw data at startup.

3. Coroutine Playback

If a valid sequence is found, a coroutine (RunSequence) is started (if not already running). This coroutine is responsible for sending the encoded Braille patterns to the hardware at regular intervals, defined by PatternDelay.

4. Pattern Advancement

On each interval:
4.1. The next pattern in the sequence is selected for each active finger.
4.2. The encoded values are sent to the Braille device
4.3. If looping is enabled, the sequence wraps around; otherwise, it stops when the end is reached.

5. Stopping and Events

You can stop playback for a specific finger or both. When a sequence finishes (and is not looping), an event (PatternEnded) is triggered for that finger. When no sequence is active for either the thumb or index finger, the coroutine is stopped.

## Pausing
Pausing is only available during the fishing gameplay

## State Machines

### MainMenuView
(More of a view that) represents each view of the main menu scene
- Main Menu
- Encyclopedia
- Level Select
- Settings

### LevelState
Represents each stage of the entire gameplay:
1. Getting ready to fish
2. Fishing proper
3. Ending the fishing cycle
4. Showing the score and end menu selection

### FishingState
Represents each stage of the fishing cycle:
1. Bait Preparation
2. Line Casting
3. Waiting for Bite
4. Reeling
5. Fish Inspection

## Resources (ScriptableObjects)

### BraillePinPatternSequence
A sequence of braille pin patterns to play.
- SequenceName: Name of this sequence (and the value to search for by the player)
- Sequence: The sequence to play. Each pattern in the inspector is represented by 4 strings on top of each other, and maps to each row of a P20

### Fish
Information specific to each fish. Currently only used by the loot table
- FishName: The name of this fish
- FishID: The id of this fish (and the value searched for)
- Sprite: the sprite to show when this fish is caught

### InputPrompt
A specific input prompt to be played
- PromptName: The name of this input prompt (and the value searched for)
- Message: The accompanying text message
- Video: the accompanying video to play

## Debug Hotkeys
Keyboard hotkeys for input or state changes

- [R] - Delete game data ([GameManager](Assets/Scripts/GameManager.cs))
- [Y] - Joystick Press ([LevelManager](Assets/Scripts/LevelManager.cs))
- [L.Alt] + [1] - Unlock milkfish ([LevelManager](Assets/Scripts/LevelManager.cs))
- [L.Alt] + [2] - Unlock seabass ([LevelManager](Assets/Scripts/LevelManager.cs))
- [L.Alt] + [3] - Unlock tilapia ([LevelManager](Assets/Scripts/LevelManager.cs))
- [E] - Add fish ([LevelState/PlayingLevelState](Assets/Scripts/LevelState/PlayingLevelState.cs))
- [A] - Button 0 Press ([Input/ButtonInput](Assets/Scripts/Input/ButtonInput.cs))
- [B] - Button 1 Press / Pause ([Input/ButtonInput](Assets/Scripts/Input/ButtonInput.cs))

# Telemetry

## ActionTelemetryHandler
Handles Action timing. Call `StartActionTimer(string actionName)` to start timing an action by name, then call `EndAndRecordActionTimer(string actionName)` to end and store the action time.
- If the action is already being timed when calling `StartActionTimer()`, the start time will be updated to when thee method was called
- If no action was being timed when calling `EndAndRecordActionTimer()`, nothing happens.

Time taken by actions is stored in a Dictionary (`Dictionary<string, List<float>>`) where the key value is a list of the time taken by each instance of an action to execute. Call `GetAverageTimeTaken()` to get a Dictionary of the average time taken by actions, where the key value is the integer average.

### Actions
- BaitPreparationRight
- BaitPreparationLeft
- FishSelection
- CastBack
- CastForward
- ReelBack
- ReelForward
- ReelClockwise
- ReelCounterClockwise
- InspectPrepare
- InspectFish
- ReleasePrepare
- ReleaseFish