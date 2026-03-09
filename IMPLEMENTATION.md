# IMPLEMENTATION.md

## 1. Project Context & Architecture

* **Goal:** Add a "Setup" window to AudioTools for Linux to verify system requirements (Wget, Pipewire, Wine < 9.21, Yabridge, Fonts) and provide one-click installations for missing components using commands from `setup reqs.sh`.
* **Tech Stack & Dependencies:** - **Language:** C# (.NET 8.0)
* **Framework:** GtkSharp (GTK 3)
* **Tools:** `/bin/bash`, `which`, `x-terminal-emulator` (for sudo password prompts).


* **File Structure:**
```text
├── Audio Tools.cs         # Add menu item and the new SetupWindow UI/logic
├── RequirementChecker.cs  # (New) Logic to run `which` and `--version` checks

```


* **Attention Points:** - Commands requiring `sudo` (like `apt-get install`) cannot be run silently via standard output redirection because they require an interactive password prompt. They must be wrapped in `x-terminal-emulator -e "bash -c '...'"` to allow user input.
* Wine version parsing must specifically check if the version is < 9.21.
* Multi-line bash scripts (like the Wine installation) should be written to a temporary `.sh` file and executed to avoid complex string escaping issues in C#.



---

## 2. Execution Phases

#### Phase 1: Menu Bar Integration

* [ ] **Step 1.1:** In `Audio Tools.cs`, locate the `MenuBar` setup section.
* [ ] **Step 1.2:** Create a new `MenuItem` named `setupMenuItem` with the label "Setup".
* [ ] **Step 1.3:** Add `setupMenuItem` to the `menuBar` alongside the existing "Options" menu.
* [ ] **Step 1.4:** Create a submenu for `setupMenuItem` and add a `MenuItem` called "Check Requirements".
* [ ] **Step 1.5:** Attach an `Activated` event to the "Check Requirements" item that calls a new method: `ShowSetupWindow()`.
* [ ] **Verification:** Build and run the app. Verify that the "Setup" menu exists at the top and clicking "Check Requirements" does not throw an exception.

#### Phase 2: Requirement Checking Logic

* [ ] **Step 2.1:** Create a new file `RequirementChecker.cs` (or add a static class at the bottom of `Audio Tools.cs`).
* [ ] **Step 2.2:** Implement a method `Task<(bool IsInstalled, string Version)> CheckToolAsync(string toolName, string versionFlag = "--version")`. Use `ProcessUtils.RunCommandWithReturnAsync($"which {toolName}")` to check existence. If it exists, run `$"{toolName} {versionFlag}"` to get the version string.
* [ ] **Step 2.3:** Implement a specific Wine check method that parses the version string (e.g., `wine-9.0`) and returns a boolean indicating if it is strictly `< 9.21`, along with the version string.
* [ ] **Step 2.4:** Implement a check for Microsoft Fonts by running `fc-list | grep -i mscorefonts`. If the output length is > 0, it's installed.
* [ ] **Verification:** Temporarily log the output of `CheckToolAsync("wget")` and `CheckToolAsync("wine")` to the console to ensure parsing works correctly on your Linux system.

#### Phase 3: Setup Window UI Scaffolding

* [ ] **Step 3.1:** In `Audio Tools.cs`, implement the `ShowSetupWindow()` method. Instantiate a new `Window("System Setup & Requirements")` with a default size of 600x400.
* [ ] **Step 3.2:** Create a main `VBox` and add a "Refresh Checks" `Button` at the top.
* [ ] **Step 3.3:** Below the button, create a `ScrolledWindow` containing a `VBox` (let's call it `requirementsList`).
* [ ] **Step 3.4:** Create a helper method `AddRequirementRow(Box parentBox, string name, bool isMet, string statusText, Action onInstall)` that creates an `HBox` containing:
* A `Label` for the component name.
* A `Label` with Pango markup for status (e.g., `<span foreground='green'>✅ {statusText}</span>` or `<span foreground='red'>❌ Not Installed</span>`).
* An "Install" `Button` (set `Sensitive = !isMet` so it's disabled if already installed).


* [ ] **Verification:** Open the "Setup" window via the menu. Verify the layout renders correctly with placeholder rows.

#### Phase 4: Wiring Install Actions & Scripts

* [ ] **Step 4.1:** In `ShowSetupWindow()`, define the installation scripts derived from `setup reqs.sh`.
* [ ] **Step 4.2:** For **Wget, Pipewire/Pactl, and Fonts**, wire the Install buttons to launch a terminal:
`Process.Start("x-terminal-emulator", "-e \"bash -c 'sudo apt-get install -y wget; echo Done; read -p \\\"Press Enter to close\\\"'\"");`
* [ ] **Step 4.3:** For **Wine**, wire the Install button to:
1. Write the multi-line Wine 9.21 installation script (from `setup reqs.sh` lines 36-66) to `/tmp/install_wine.sh`.
2. Execute `Process.Start("x-terminal-emulator", "-e \"bash -c 'sudo bash /tmp/install_wine.sh; read -p \\\"Press Enter...\\\"'\"");`


* [ ] **Step 4.4:** For **Yabridge**, wire the Install button to use the existing `YabridgeService.RunCommandAsync()` with a `ProgressDialog` since it doesn't require `sudo`, using the `wget`/`tar` extraction script from `setup reqs.sh`.
* [ ] **Step 4.5:** Implement the actual population logic inside `ShowSetupWindow()`: call the checker methods from Phase 2, then clear and populate the `requirementsList` VBox using `AddRequirementRow` for each tool. Tie this to the "Refresh Checks" button as well.
* [ ] **Verification:** Uninstall `wget` (if safe) or temporarily change the check for a dummy package. Click the "Install" button in the Setup UI. Verify a terminal pops up, asks for your `sudo` password, installs the package, and waits for Enter.

---

## 3. Global Testing Strategy

* **Wine Version Edge Cases:** Manually install a Wine version > 9.21 (if possible on a test machine) to ensure the checker correctly flags it with a Red X and warns the user.
* **Terminal Fallback:** If `x-terminal-emulator` is not linked to the user's default terminal (e.g., they only have `gnome-terminal` or `konsole`), the `Process.Start` might fail. Catch exceptions on `Process.Start` and display a `MessageDialog` instructing the user to run the `setup reqs.sh` script manually if the terminal fails to launch.
* **Asynchronous UI:** Ensure the window does not freeze while the `CheckToolAsync` methods are querying versions in the background upon opening the window.