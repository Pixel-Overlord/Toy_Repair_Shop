# Toy Repair Shop

A portrait-mode 2D mobile casual game built in Unity (2022.3 LTS, Built-in Render Pipeline, C#). Players repair a stream of broken toys by selecting the correct tool for each repair step and dragging it across the toy.

## Gameplay Loop

1. From the Main Menu, tap **Play** to enter the Workshop.
2. A random broken toy is spawned with a name, a required sequence of repair steps, and a matching tool for each step.
3. Tap the toolbar button matching the current step's required tool, then drag across the toy to fill the progress bar.
4. Each completed step advances the toy's look (a new state sprite fades in) until the final step reveals the fully repaired toy.
5. A **Congratulations** popup celebrates the repair and awards coins, then the next toy spawns automatically.

## Project Structure

```
Assets/Scripts/
  Core/        - zero-dependency shared primitives
  Data/        - enums and ScriptableObject data definitions (ToyData, RepairStepData, ToyDatabase)
  Gameplay/    - RepairController, RepairStateMachine, RepairBehaviour strategies, spawning, drag interaction
  Managers/    - scene composition roots (WorkshopController, MainMenuUI's backing manager, SceneLoader, AudioManager, SaveManager, GameManager)
  UI/          - view components (ToyView, ToolbarView, RepairHUDView, RewardPopupView, ProgressBarView, ...)
  Editor/      - one-off and reusable batch-mode art/scene integration tools (not part of the shipped build)
```

Toy content is entirely data-driven: a `ToyData` ScriptableObject asset (`Assets/ScriptableObjects/Toys/`) defines a toy's name, category, reward, and its ordered list of `RepairStepData` steps (each step specifies the required `ToolType` and a `RepairType`). Adding a new toy to the game is a matter of creating a new `ToyData` asset and registering it in `ToyDatabase.asset` - no code changes required.

## Content Currently in the Game

- **10 toys**, each with a broken state, two mid-repair states, and a fully repaired state: Teddy, Robot, Car, Train, Plane, Bunny, Rocket, Truck, Drum, Boat.
- **7 reusable repair steps** (`Assets/ScriptableObjects/RepairSteps/`): Wash, Dry, Sew, Clean, Tighten, Polish, Paint - each toy uses a themed 3-step sequence built from these.
- **5 tools**: Sponge, Cloth, Needle, PaintRoller, Screwdriver.

## Architecture Notes

- `RepairController` (pure C#, no `MonoBehaviour`) orchestrates a repair session through a `RepairStateMachine` and reports progress via events; it never branches on `RepairType` directly - each `IRepairBehaviour` strategy owns what "progress" means for its type.
- `WorkshopController` is the Workshop scene's composition root: it wires the spawner, interaction, HUD, and popup views to `RepairController` and `ToolManager` via events, and owns the "spawn next toy" loop.
- Views (`ToyView`, `RepairHUDView`, `RewardPopupView`, `ProgressBarView`, `ToolbarView`) are purely presentational - they react to calls from `WorkshopController` and never read gameplay state directly.
- Toy repair-state art transitions use plain coroutine fades; per-tool and per-toy `Animator` hooks exist for anyone who wants to layer in real `AnimationClip`s later without touching gameplay code.

## Recent Changes

- Reworked the reward popup to use the `Congratulation_Banner` artwork, added a short delay before it appears so the fully-repaired toy is visible first, and personalized its message.
- Fixed the repair progress bar to track overall toy progress instead of resetting to 0% between steps.
- Simplified the in-repair HUD down to the toy name, current step, a plain-language "what to do next" hint, and the progress bar.
- Added a **Back to Menu** button to the Workshop scene.
- Re-sliced and re-imported toy art from `Toys_SpriteSheet_1`/`Toys_SpriteSheet_2`, expanding the toy roster from 3 to 10 and giving each toy a 4-state repair sequence with fade transitions between states.
- Removed unused/superseded source art (`Atlas_Toys_01.png`, `Progress bar.png`) once nothing referenced them anymore.

## Working with the Editor Tools

`Assets/Scripts/Editor/` contains batch-mode-runnable `[MenuItem]` tools used to slice atlases, wire scene references, and render scene previews without opening the Unity Editor UI (useful for headless verification). These are development tooling, not shipped game code - safe to delete individually once their one-time job is done, though a few (`AlphaBoundsSlicer`, `AtlasGridSlicer`, `ScenePreviewRenderer`) are kept as reusable utilities for future art passes.
