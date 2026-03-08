# Changelog

All notable changes to AudioTools for Linux will be documented in this file.

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
