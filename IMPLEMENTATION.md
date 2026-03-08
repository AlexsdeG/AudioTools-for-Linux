# IMPLEMENTATION.md

## 1. Project Context & Architecture

* **Goal:** Add a path management GUI to AudioTools for Linux, allowing users to visually add, remove, and list plugin directories managed by `yabridgectl`.
* **Tech Stack & Dependencies:** - **Language:** C# (.NET 8.0)
* **UI Framework:** GtkSharp (GTK 3)
* **Tooling:** `yabridgectl` CLI


* **File Structure:**
```text
├── Audio Tools.cs         # Modify to add trigger button and window logic
├── AudioTools.csproj      # Existing project configuration

```


* **Attention Points:** - Commands must be executed via `/bin/bash` to ensure shell expansions (like `$HOME`) work correctly.
* Use `Gtk.TreeView` with a `ListStore` for the path list to provide a standard desktop list experience.



---

## 2. Execution Phases

#### Phase 1: Main Window Integration

* [x] **Step 1.1:** In `Audio Tools.cs`, locate the `VBox` setup and add a new `Button` labeled "Manage Paths".
* [x] **Step 1.2:** Attach a `Clicked` event handler to the "Manage Paths" button that initializes and shows the `PathManagerWindow`.
* [x] **Step 1.3:** Position the button within the `vbox` layout, ideally near the existing Yabridge controls.
* [x] **Verification:** Run the app and ensure the "Manage Paths" button appears and can be clicked without errors.

#### Phase 2: Path Management Window Scaffolding

* [x] **Step 2.1:** Create a method or helper class `PathManagerWindow` that inherits from `Gtk.Window`.
* [x] **Step 2.2:** Set window properties: Title ("Manage Plugin Paths"), Default Size (500x300), and Type (Toplevel).
* [x] **Step 2.3:** Implement a `VBox` container to hold the path list and a separate `HBox` for action buttons (Add/Remove/Close).
* [x] **Verification:** Click the "Manage Paths" button in the main app and verify a new empty window opens.

#### Phase 3: List Implementation & Data Retrieval

* [x] **Step 3.1:** Define a `Gtk.TreeView` and a `Gtk.ListStore` with a single string column for paths.
* [x] **Step 3.2:** Implement a `RefreshPathList()` function that runs `yabridgectl list` using the existing `RunCommandWithReturn` method.
* [x] **Step 3.3:** Add logic to parse the output of `yabridgectl list` (filtering out header/footer text) and populate the `ListStore`.
* [x] **Step 3.4:** Add the `TreeView` into a `ScrolledWindow` within the popup's `VBox`.
* [x] **Verification:** Open the Manage Paths window; verify it correctly lists your currently configured `yabridge` directories.

#### Phase 4: Add & Remove Logic

* [x] **Step 4.1:** Implement the "Add" button: Use `Gtk.FileChooserDialog` (Action: SelectFolder) to let the user pick a directory.
* [x] **Step 4.2:** On folder selection, execute `$HOME/.local/share/yabridge/yabridgectl add [selected_path]` via the existing command runner.
* [x] **Step 4.3:** Implement the "Remove" button: Get the selected path from the `TreeView` selection and execute `yabridgectl rm [selected_path]`.
* [x] **Step 4.4:** Ensure `RefreshPathList()` is called automatically after any Add or Remove operation to update the UI.
* [x] **Verification:** Add a new test folder and verify it appears in the list; then remove it and verify it disappears.

---

## 3. Global Testing Strategy

* **Permissions:** Ensure the app handles cases where the user selects a folder they don't have read/write access to.
* **Path Spaces:** Test adding and removing paths that contain spaces (e.g., `Documents/My VSTs`) to ensure shell commands are properly quoted.
* **Empty State:** Verify the window behaves correctly if `yabridgectl list` returns no paths.
* **Duplicate Prevention:** Observe how `yabridgectl` handles adding a path that already exists and ensure the UI reflects the result gracefully.