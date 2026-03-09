# Changelog

All notable changes to AudioTools for Linux will be documented in this file.

## [0.98.0] - 2026-03-09

### Fixed
- Automatically confirms `yabridgectl rm` prompts by piping `yes`, preventing first-attempt remove hangs.
- Simplified path removal refresh flow to a single `RefreshPathListAsync` pass after removal delay.
- Made path list refresh atomic by clearing and repopulating `ListStore` inside one GTK invoke block.

## [0.97.0] - 2026-03-09

### Fixed
- **Path management UI freeze on first removal**: Fixed critical threading bug where removing/adding paths would freeze UI buttons on the first attempt. Root cause was GTK dialog operations (MessageDialog, ProgressDialog) being called from async continuations on non-GTK threads, blocking the GTK event loop.
  - Added `InvokeOnGtkThreadAsync<T>()` helper to safely marshal GTK operations back to main thread
  - Refactored `RemovePathAsync()` to create sync confirmation dialog on GTK thread
  - Refactored `AddPathAsync()` to create sync confirmation dialog on GTK thread
  - Both methods now properly close ProgressDialog on GTK thread via `InvokeOnGtkThreadAsync`
  - Buttons and UI elements now correctly re-enable after path operations
  - List refresh now displays correct path count after first attempt

## [0.96.0] - 2026-03-08

### Added
- **Plugin Browser window** with a new **View Plugins** button in the main UI.
- **Scrollable plugin list** showing plugin **Name**, **Type**, **Location**, and an **Action** column.
- **Refresh support** to reload plugin status directly from `yabridgectl status`.
- **Folder opening workflow**: double-clicking a plugin row opens its folder using `xdg-open`.

### Changed
- Reused and exposed `RunCommandWithReturn` for plugin status retrieval in the plugin browser.
- Added robust parsing logic for section-based `yabridgectl status` output with empty-state handling.

## [0.95.0] - 2026-03-08

### Added
- **Path Management GUI**: Added comprehensive visual interface for managing yabridge plugin directories
  - New "Manage Paths" button in the main window near yabridge controls
  - Dedicated "Manage Plugin Paths" window with scrollable list view
  - **Add** functionality: Browse and select directories to add as plugin paths
  - **Remove** functionality: Select and remove existing paths with confirmation dialog
  - Real-time list refresh after add/remove operations
  - Automatic parsing and filtering of `yabridgectl list` output
  - Proper handling of paths with spaces through shell command quoting

### Technical Details
- Implemented using `Gtk.TreeView` and `Gtk.ListStore` for native desktop list experience
- Integrated with existing `yabridgectl` CLI commands
- Commands properly executed via `/bin/bash` to support shell expansions
- Added `ShowPathManagerWindow()`, `RefreshPathList()`, `AddPath()`, and `RemovePath()` methods

## [0.94.0] - Previous Release
- Initial version with basic yabridge controls and audio configuration
