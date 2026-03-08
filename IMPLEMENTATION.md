# IMPLEMENTATION.md

## 1. Project Context & Architecture

* **Goal:** Create a "Plugin Browser" window that lists all synced plugins found by `yabridge`, displaying their name, type (VST2/VST3/CLAP), and directory location, with the ability to open their containing folder in the file explorer.
* **Tech Stack & Dependencies:**
* **Language:** C# (.NET 8.0).
* **UI Framework:** GtkSharp (GTK 3).
* **External CLI:** `yabridgectl` for data retrieval.
* **System Tool:** `xdg-open` for opening file explorer windows.


* **File Structure:**
* `Audio Tools.cs`: Primary file for adding the trigger button, the new window class, and parsing logic.


* **Attention Points:**
* `yabridgectl status` output is tabular text; parsing must account for variable spacing.
* File paths in Linux should be handled carefully to avoid shell escaping issues when passed to `xdg-open`.



---

## 2. Execution Phases

#### Phase 1: Main Window Update

* [x] **Step 1.1:** In `Audio Tools.cs`, add a new `Button` titled "View Plugins" to the main `vbox`.
* [x] **Step 1.2:** Implement a click handler that instantiates and shows the `PluginBrowserWindow`.
* [ ] **Verification:** Launch AudioTools and confirm the "View Plugins" button exists and opens a new window.

#### Phase 2: Plugin Browser Window & UI Layout

* [x] **Step 2.1:** Create the `PluginBrowserWindow` class inheriting from `Gtk.Window`.
* [x] **Step 2.2:** Add a `TreeView` inside a `ScrolledWindow` to handle many plugins.
* [x] **Step 2.3:** Define four columns in the `TreeView`: **Name**, **Type**, **Location**, and an **Action** (Open Folder).
* [x] **Step 2.4:** Add a "Refresh" button to re-run the scan command.
* [ ] **Verification:** Open the window and verify the headers for Name, Type, and Location are visible.

#### Phase 3: Data Retrieval & Parsing

* [x] **Step 3.1:** Execute `$HOME/.local/share/yabridge/yabridgectl status` using the existing `RunCommandWithReturn` helper.
* [x] **Step 3.2:** Implement a parser to loop through the string lines. Use `StringSplitOptions.RemoveEmptyEntries` to extract columns from the `yabridgectl` table output.
* [x] **Step 3.3:** Map the parsed data into a `Gtk.ListStore` (string, string, string).
* [ ] **Verification:** Verify the console output of the parser shows correctly identified plugin names and paths.

#### Phase 4: File Explorer Integration

* [x] **Step 4.1:** Attach a `RowActivated` event (double-click) or a specific "Open Folder" button for each row.
* [x] **Step 4.2:** Use `Process.Start("xdg-open", directoryPath)` to trigger the system's default file manager at the plugin's location.
* [x] **Step 4.3:** Ensure the path is wrapped in quotes or properly escaped for the shell command.
* [ ] **Verification:** Click a plugin in the list and confirm your Linux file manager (Nautilus, Dolphin, etc.) opens the correct folder.

---

## 3. Global Testing Strategy

* **Large Libraries:** Test the window's performance and scrolling with 100+ plugins.
* **Path Variants:** Ensure plugins located in hidden folders (e.g., `.wine/drive_c/...`) open correctly in the file explorer.
* **Empty State:** If `yabridgectl` hasn't synced anything yet, display a "No plugins found" message or an empty list gracefully.