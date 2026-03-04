# Utilities

Utilities in this folder:
- `StaminaScript`
  - Configurable stamina max, regen rate, and regen delay
  - Safe stamina spending via `UseStamina(float amount)`
  - Recovery helpers (`RestoreStamina`, `ResetToFull`)
  - Read-only public properties for UI and gameplay systems
- `PauseMenu`
  - Centralized pause/resume state and timescale control
  - Canvas toggle support via `pauseMenuCanvas`
  - Static read-only pause flag: `PauseMenu.gameIsPaused`

Main files:
- `Scripts/Utilities/StaminaScript.cs`
- `Scripts/Utilities/PauseMenu.cs`
