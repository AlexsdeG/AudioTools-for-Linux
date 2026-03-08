# Architecture Blueprint

## Repository Map
- `AudioTools.csproj`: Build/runtime configuration for the app (`net8.0`, GtkSharp dependency, Linux self-contained publish settings).
- `Audio Tools.cs`: Main application implementation.
- `setup/*.sh`: Environment bootstrap and dependency validation scripts.
- `Run This To Build.sh`: Convenience wrapper for production publish.

## Business Logic Location
- Business and orchestration logic currently lives in `Audio Tools.cs`:
  - Command selection and action routing from UI controls.
  - Shell command execution wrappers (`RunCommand`, `RunCommandWithReturn`).
  - Initial environment status aggregation and display.

## Route Layer
- This repository has no HTTP/API route layer.
- User interactions are event-driven Gtk handlers (button/menu callbacks) in `Audio Tools.cs`.

## UI Component Layer
- GTK UI composition is in `Audio Tools.cs` constructor:
  - Window, menus, labels, combo boxes, buttons, output view.
  - Event handlers trigger domain actions through command execution methods.

## Data and Control Flow
1. User clicks a GTK menu item or button.
2. Handler builds command string and dispatches through `RunCommand` or `RunCommandWithReturn`.
3. `/bin/bash -c` executes command.
4. Output/error streams are captured and rendered in `TextView`.
5. UI remains the operator console for system-level audio tooling.
