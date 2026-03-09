# IMPLEMENTATION.md

## 1. Project Context & Architecture

* **Goal:** Fix a bug where removing a path hangs on the first attempt due to `yabridgectl rm` pausing for interactive terminal input, and resolve UI desynchronization where the removed path persists in the list until reopening.
* **Tech Stack & Dependencies:**
* **Language:** C# (.NET 8.0)
* **Framework:** GtkSharp
* **CLI Tool:** `yabridgectl` and POSIX `yes` command.


* **File Structure:**
```text
├── Audio Tools.cs         # Target for command modification and UI update refactoring

```


* **Attention Points:** - `yabridgectl rm` detects leftover `.so` files and prompts: `Do you want to remove them? [y/N]`. Since the app uses standard shell execution without an interactive `stdin`, it hangs. Using the POSIX `yes` utility will automatically force the command to accept the prompt.
* GTK `ListStore` updates must be atomic to ensure the visual list precisely matches the parsed command output without flickering or holding stale data.



---

## 2. Execution Phases

#### Phase 1: Fix CLI Hang via Auto-Confirmation

* [x] **Step 1.1:** In `Audio Tools.cs`, locate the `RemovePathAsync` method.
* [x] **Step 1.2:** Find the line executing the `rm` command:
`int exitCode = await RunCommandAsync($"$HOME/.local/share/yabridge/yabridgectl rm \"{selectedPath}\"", progress);`
* [x] **Step 1.3:** Modify the command string to pipe `yes` into it, which automatically answers 'y' to the interactive prompt:
`$"yes | $HOME/.local/share/yabridge/yabridgectl rm \"{selectedPath}\""`
* [ ] **Verification:** Run the app, add a path, and click remove. Observe the output log to ensure it successfully bypassed the "leftover .so files" interactive prompt without hanging the UI.

#### Phase 2: Refactor Path Removal Workflow

* [x] **Step 2.1:** In `RemovePathAsync`, delete the nested local function `async Task<bool> RefreshPathListPassAsync(int passNumber)`.
* [x] **Step 2.2:** Remove the duplicate refresh passes (`refreshPass1` and `refreshPass2`).
* [x] **Step 2.3:** Replace the deleted logic with a single, direct call: `await RefreshPathListAsync(listStore);` placed immediately after the `await Task.Delay(500);` line.
* [x] **Step 2.4:** Remove the `if (!refreshSucceeded)` check, as the single pass will now accurately handle the updated state.
* [ ] **Verification:** Remove a path and verify that the sync dialog ("Run yabridgectl sync now to update registry?") appears promptly after the single refresh completes.

#### Phase 3: Atomic UI Updates

* [x] **Step 3.1:** In `Audio Tools.cs`, locate the `RefreshPathListAsync` method.
* [x] **Step 3.2:** Remove the early UI clearing line: `Application.Invoke(delegate { listStore.Clear(); });` from the top of the method.
* [x] **Step 3.3:** Below the `var lines = output.Split(...)` parsing logic, create a new `Application.Invoke(delegate { ... });` block.
* [x] **Step 3.4:** Inside this invoke delegate, execute `listStore.Clear();`, followed by a `foreach` loop that iterates over `lines` and calls `listStore.AppendValues(trimmedLine)` for any string starting with `/` or `~`.
* [ ] **Verification:** Add and remove a path. Verify that the visual list updates instantly without displaying the removed path, and the UI buttons re-enable properly.

---

## 3. Global Testing Strategy

* **Edge Case - No Leftover Files:** Test removing an empty directory to ensure the `yes |` pipe behaves correctly when `yabridgectl rm` does *not* prompt for confirmation (it should exit cleanly).
* **UI State Integrity:** Close the path manager window in the middle of a deletion or sync operation. Verify that the main window controls (`topControlsBox`) still correctly revert back to a sensitive/enabled state.