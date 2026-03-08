# IMPLEMENTATION.md

## 1. Project Context & Architecture

* **Goal:** Create a "Plugin Browser" window that lists all synced plugins found by `yabridge`, displaying their name, type (VST2/VST3/CLAP), and directory location, with the ability to open their containing folder in the file explorer.
* **Tech Stack & Dependencies:**
* **Language:** C# (.NET 8.0).
* **UI Framework:** GtkSharp (GTK 3).
* **External CLI:** `yabridgectl` for data retrieval.
* **System Tool:** `xdg-open` for opening file explorer windows.


# IMPLEMENTATION.md

## Overview
This document describes the design and prioritized workplan for improving AudioTools, with a focus on robust `yabridgectl` integration, responsive UI, and maintainable code structure.

Goals
- Make `yabridgectl` operations reliable and user-friendly.
- Prevent UI freezes and double-click/duplicate-command issues.
- Improve "open folder" reliability across run modes (Debug vs published single-file).
- Split UI and execution logic to enable testing and easier maintenance.
- Add logging and simple persistence for user settings.

## Prioritized Roadmap
- High (do first)
	- Robust OpenFolder behavior and clear failure reporting.
	- Convert blocking process calls to async with streamed output.
	- Disable UI controls while long operations run (prevent double clicks).
	- Surface stderr and non-zero exit codes to the UI.
- Medium
	- Progress / cancellable dialogs for long `yabridgectl` runs.
	- Auto-sync after changing plugin paths (with opt-in confirmation).
	- Split CLI logic into `YabridgeService` (separation of concerns).
	- Basic logging to `~/.config/AudioTools/AudioTools.log`.
- Low
	- Settings persistence (JSON under `~/.config/AudioTools/`).
	- Unit tests for `ParsePluginStatus()` and other parsers.
	- UI polish (centered Clear button, search/filter in plugin browser, context menu).

## Design Notes & Rationale
- Published single-file apps can run with a different PATH and environment; launching external tools must explicitly check or adapt to environment differences (e.g., `DISPLAY`, `DBUS_SESSION_BUS_ADDRESS`).
- Long-running CLI calls must not run on GTK thread; use `Task.Run` + `Process.WaitForExitAsync()` and marshal UI updates using `Application.Invoke`.
- For opening folders prefer `UseShellExecute = true` with `FileName = location` (let OS handle it). If not available, fall back to `xdg-open` and capture exit code + stderr.
- Always disable the UI control that triggered an operation and show an animated spinner or change the label to a loading state, re-enable after completion or failure.

## Implementation Plan (detailed, ordered)

1) [x] Robust OpenFolderLocation (Small)
	 - Files: `Audio Tools.cs` (method `OpenFolderLocation`).
	 - Tasks:
		 - Try `ProcessStartInfo.UseShellExecute = true` with `FileName = location` first and start it without arguments; on success return.
		 - If that throws or returns null, try canonical `xdg-open` paths (`/usr/bin/xdg-open`, `/bin/xdg-open`, `xdg-open`).
		 - Capture `ExitCode`, `StandardError` and wait for exit (use async).
		 - Show a MessageDialog containing actionable stderr and a suggestion (e.g., "install xdg-open or ensure PATH includes it").
	 - Why: Published binaries often miss session env vars; this fallback mosaic increases reliability.

2) [ ] Async process helpers + streaming (Medium)
	 - Files: Replace `RunCommand` / `RunCommandWithReturn` in `Audio Tools.cs` with:
		 - `Task RunCommandAsync(string command, IProgress<string> progress, CancellationToken ct)`
		 - `Task<string> RunCommandWithReturnAsync(string command, CancellationToken ct)`
	 - Implementation details:
		 - Use `ProcessStartInfo` with Redirected streams, `UseShellExecute = false`.
		 - Use `process.BeginOutputReadLine()` + event handlers; prefer `await process.WaitForExitAsync(ct)`.
		 - Report output via `progress.Report(line)` and marshal updates to the UI using `Application.Invoke`.
	 - Why: Prevents UI freezes and supports streaming live output into the TextView.

3) [ ] Disable controls / loading state pattern (Small)
	 - Files: `Audio Tools.cs` — all button click handlers that run long operations (`yabridgectl sync`, `list`, `status`, `AddPath`, `RemovePath`, plugin refresh).
	 - Tasks:
		 - On click, set `button.Sensitive = false` (or set a `Loading` label/icon).
		 - Optionally add a small `Spinner` or change the button label to `Working...`.
		 - Re-enable button at operation completion or on exception in `finally`.
		 - Prevent nested clicks via a simple per-action `bool` guard or by disabling the full `topControlsBox` while operations run.
	 - Why: Avoids duplicate operations and confusing state.

4) [ ] Progress / cancellable dialog for long `yabridgectl` runs (Medium)
	 - Files: New small helper class `ProgressDialog` (GTK dialog with spinner, text area, Cancel button).
	 - Tasks:
		 - Show dialog when running `sync` or `sync -v`.
		 - Pipe process output to dialog log and to main output area.
		 - Support cancellation via `CancellationTokenSource` that kills process.
	 - Why: Better UX for long sync operations and safe cancellation.

5) [ ] Auto-resync and improved path management (Small → Medium)
	 - Files: `AddPath`, `RemovePath`, `RefreshPathList` in `Audio Tools.cs`.
	 - Tasks:
		 - After adding/removing a path, prompt user: "Run yabridgectl sync now?" (checkbox to auto-run).
		 - If yes, run `yabridgectl sync` asynchronously, show progress dialog.
	 - Why: Keeps yabridge registry consistent without manual steps.

6) [ ] Split logic into `YabridgeService` (Medium)
	 - Files: New file `YabridgeService.cs`; refactor `RunCommand*` usage to the service.
	 - Responsibilities:
		 - Provide `Task<string> GetStatusAsync()`, `Task<List<(Name,Type,Location)>> GetPluginsAsync()`, `Task<List<string>> GetPathsAsync()`, `Task SyncAsync(...)`.
		 - Centralized logging and environment checks.
	 - Why: Easier testing, reuse, and clearer UI code.

7) [ ] Parser hardening & unit tests (Small → Medium)
	 - Files: Extract `ParsePluginStatus` into `PluginParser.cs`.
	 - Tasks:
		 - Add unit tests (new test project) for multiple `yabridgectl status` sample outputs (edge cases, different formats).
		 - Normalize paths and handle empty/unknown entries.
	 - Why: Prevent regressions and increase robustness.

8) [ ] Settings & logging (Small)
	 - Files: New `Settings.cs`, `Logger.cs`; create `~/.config/AudioTools/` directory.
	 - Tasks:
		 - Save last window position, last yabridge action, list of user-added plugin paths (optional).
		 - Log commands, environment vars, and stderr to `AudioTools.log`.
	 - Why: Easier diagnosis for support and persist useful preferences.

9) [ ] UI polish & deprecated-ctor replacement (Small)
	 - Files: `Audio Tools.cs`.
	 - Tasks:
		 - Replace obsolete `new VBox()`/`new HBox()` constructors with `new Box(Orientation.Vertical, spacing)` where practical to remove deprecation warnings.
		 - Center Clear button using an `HBox` with `Center` alignment.
		 - Add search/filter bar in plugin browser (optional).
	 - Why: Cleaner code and fewer warnings.

## Implementation Details & Code Pointers
- `OpenFolderLocation`: see current implementation in `Audio Tools.cs` (search for `OpenFolderLocation`) — replace with the robust fallback + diagnostics behavior.
- `RunCommand` / `RunCommandWithReturn`: appear in `Audio Tools.cs` — convert to async helpers; use `Task.Run` + `process.WaitForExitAsync`.
- UI handlers to update: the yabridge buttons and `Manage Paths`/`View Plugins` flows in `Audio Tools.cs`.
- Parser to extract: `ParsePluginStatus(string output)` in `Audio Tools.cs` — extract and add tests.

## Example: Button disabled pattern (pseudo)
- On click:
	- `button.Sensitive = false;`
	- `try { await YabridgeService.SyncAsync(progress, ct); } finally { button.Sensitive = true; }`

## Testing & Validation
- Manual:
	- Run published binary from a terminal; validate `OpenFolderLocation` works when launched from a desktop `.desktop` file and when launched from terminal.
	- Run `yabridgectl sync` with many plugins to test streaming/progress UI.
- Automated:
	- Unit tests for plugin parsing (various outputs).
	- Integration tests that mock `YabridgeService` (if feasible).

## Backwards Compatibility & Safety
- All changes are additive; default behavior remains unless toggles are introduced.
- Long-running commands should be cancellable and should not change application state until operation completes.

## Suggested timeline & effort estimate
- Day 1 (1–3 hours): Implement robust `OpenFolderLocation` + small tests.
- Day 2 (4–8 hours): Implement async process helpers and convert `RefreshPluginList` and `RefreshPathList` to async + disable buttons while running.
- Day 3 (4–8 hours): Add progress dialog and cancellation for `sync`.
- Day 4 (4–8 hours): Refactor `YabridgeService`, extract parser, and add unit tests.
- Follow-up (2–6 hours): Settings, logging, and UI polish.

## Acceptance criteria
- Published binary opens folder reliably on typical Ubuntu desktop.
- Long `yabridgectl` commands run without blocking the UI and show live output.
- Buttons are disabled during operations and re-enabled on completion or failure.
- Parser passes unit tests for representative `yabridgectl` output variations.

---

Quick next steps I can implement for you now (pick one):
- A. Add the robust `OpenFolderLocation()` implementation + user-facing error dialog.
- B. Convert `RunCommandWithReturn()` to `RunCommandWithReturnAsync()` and convert `RefreshPluginList()` to use it (includes button disable pattern).
- C. Implement progress dialog + cancellation for `yabridgectl sync`.

Tell me which to implement first and I’ll produce the exact code changes and run a build locally.