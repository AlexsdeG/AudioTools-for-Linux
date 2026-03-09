using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Gtk;

public class AudioTools
{
    private TextView outputTextView;
    private VBox topControlsBox;

    public AudioTools()
    {    	
        Application.Init();
        // Initialize logging and load persisted settings
        Logger.Init();
        SettingsManager.Load();
        
        // Create the main window
        var appVersion = "0.98";
        var window = new Window("AudioTools v" + appVersion);
        // Restore saved size if present
        if (SettingsManager.Instance != null && SettingsManager.Instance.WindowWidth > 0 && SettingsManager.Instance.WindowHeight > 0)
        {
            window.SetDefaultSize(SettingsManager.Instance.WindowWidth, SettingsManager.Instance.WindowHeight);
        }
        else
        {
            window.SetDefaultSize(400, 500);
        }
        window.SetPosition(WindowPosition.Center);
        window.DeleteEvent += (o, args) =>
        {
            try
            {
                // Persist window position/size and last action
                try { window.GetSize(out int w, out int h); SettingsManager.Instance.WindowWidth = w; SettingsManager.Instance.WindowHeight = h; } catch { }
                try { window.GetPosition(out int x, out int y); SettingsManager.Instance.WindowX = x; SettingsManager.Instance.WindowY = y; } catch { }
                SettingsManager.Save();
                Logger.LogInfo("Application exiting, settings saved.");
            }
            catch { }

            Application.Quit();
        };

        // Create a vertical box to hold all sections
        var vbox = new VBox();

        // Keep the controls area visually grouped with padding.
        topControlsBox = new VBox(false, 5)
        {
            BorderWidth = 10
        };


        // Create an HBox to hold the two buttons side by side
        var hbox = new HBox();

        var menuBar = new MenuBar();
        vbox.PackStart(menuBar, false, false, 0);
		
		var optionsMenuItem = new MenuItem("Options");
        menuBar.Add(optionsMenuItem);
        
        // Create Options submenu
        var optionsMenu = new Menu();
        optionsMenuItem.Submenu = optionsMenu;
        
        // Launch winetricks
        var winetricksItem = new MenuItem("Open Winetricks");
        winetricksItem.Activated += (sender, e) => {
            ProcessUtils.RunCommand("winetricks", s => AppendOutput(s));
        };
		optionsMenu.Append(winetricksItem);
		
		// Launch winecfg
        var winecfgItem = new MenuItem("Open WINE config");
        winecfgItem.Activated += (sender, e) => {
            ProcessUtils.RunCommand("winecfg", s => AppendOutput(s));
        };
		optionsMenu.Append(winecfgItem);
		
		// Launch wine uninstaller
        var wineuninstItem = new MenuItem("Open WINE programs");
        wineuninstItem.Activated += (sender, e) => {
            ProcessUtils.RunCommand("wine uninstaller", s => AppendOutput(s));
        };
		optionsMenu.Append(wineuninstItem);
		
		// Check for updates
		var updateItem = new MenuItem("Check for Updates");
		updateItem.Activated += (sender, e) => 
        {
        	var yabridgeVersion = "";
                var checkyabridgeVersion = "";
                var checkAudioToolsVersion = "";
                yabridgeVersion = YabridgeService.RunCommandWithReturn("$HOME/.local/share/yabridge/yabridgectl --version");
        
                checkyabridgeVersion = ProcessUtils.RunCommandWithReturn("wget -qO- https://api.github.com/repos/robbert-vdh/yabridge/releases/latest | jq -r '.tag_name'");
        		checkAudioToolsVersion = ProcessUtils.RunCommandWithReturn("wget -qO- https://api.github.com/repos/Heroin-Bob/AudioTools-for-Linux/releases/latest | jq -r '.tag_name'");
	        using (MessageDialog md = new MessageDialog(null,
	        DialogFlags.Modal,
	        MessageType.Info,
	        ButtonsType.Ok,
	        "yabridge version: " + yabridgeVersion.Replace("yabridgectl ","") + "\n" + "latest yabridge version: " + checkyabridgeVersion + "\n" + "AudioTools version: " + appVersion + "\n" + "Latest version: " + checkAudioToolsVersion.Replace("v","")))
	        {
	            md.Title = "Message";
	            md.Run();
	            md.Destroy();
	        }
        };
        
		optionsMenu.Append(updateItem);
		
		
        // Create a label
        var sammpleRateLabel = new Label("Sample Rate:");
        hbox.PackStart(sammpleRateLabel, false, false, 5);

        // Create a dropdown (ComboBox)
        var sampleComboBox = new ComboBoxText();
        sampleComboBox.AppendText("44100");
        sampleComboBox.AppendText("48000");
        hbox.PackStart(sampleComboBox, false, false, 5);

        topControlsBox.PackStart(hbox, false, true, 5);

        // Create a button
        var sampleButton = new Button("Set Sample Rate");
        sampleButton.Clicked += (sender, e) =>
        {
            string selectedValue = sampleComboBox.ActiveText;
            string concatCommand = "pw-metadata -n settings 0 clock.force-rate " + selectedValue;
            ProcessUtils.RunCommand(concatCommand, s => AppendOutput(s));
        };
        hbox.PackStart(sampleButton, false, false, 5);

        hbox = new HBox();

        // Create a label
        var bufferSizeLabel = new Label("Buffer Size:");
        hbox.PackStart(bufferSizeLabel, false, false, 5);

        // Create a dropdown (ComboBox)
        var bufferComboBox = new ComboBoxText();
        bufferComboBox.AppendText("16");
        bufferComboBox.AppendText("32");
        bufferComboBox.AppendText("64");
        bufferComboBox.AppendText("128");
        bufferComboBox.AppendText("256");
        bufferComboBox.AppendText("512");
        bufferComboBox.AppendText("1024");
        hbox.PackStart(bufferComboBox, false, false, 5);

        topControlsBox.PackStart(hbox, false, true, 5);

        // Create a button
        var bufferButton = new Button("Set Buffer Size");
        bufferButton.Clicked += (sender, e) =>
        {
            string selectedValue = bufferComboBox.ActiveText;
            string concatCommand = "pw-metadata -n settings 0 clock.force-quantum " + selectedValue;
            ProcessUtils.RunCommand(concatCommand, s => AppendOutput(s));
        };
        hbox.PackStart(bufferButton, false, false, 5);


	hbox = new HBox();
    var managePathsLabel = new Label("Plugin Paths:");
    hbox.PackStart(managePathsLabel, false, false, 5);

    // Create Manage Paths button
    var managePathsButton = new Button("Manage Paths");
    managePathsButton.Clicked += (sender, e) => ShowPathManagerWindow();
    hbox.PackStart(managePathsButton, false, false, 5);
    topControlsBox.PackStart(hbox, false, false, 5);

    hbox = new HBox();
    var viewPluginsLabel = new Label("Plugin Browser:");
    hbox.PackStart(viewPluginsLabel, false, false, 5);

    // Create View Plugins button
        var viewPluginsButton = new Button("View Plugins");
        viewPluginsButton.Clicked += (sender, e) => ShowPluginBrowserWindow();
    hbox.PackStart(viewPluginsButton, false, false, 5);
    topControlsBox.PackStart(hbox, false, false, 5);

        // Add spacing before the Yabridge controls section.
        hbox = new HBox();
        topControlsBox.PackStart(hbox, false, false, 2);

        hbox = new HBox();

        // Create a label
        var yabridgeLabel = new Label("Yabridge:");
        hbox.PackStart(yabridgeLabel, false, false, 5);

        // Create a dropdown (ComboBox)
        var yabridgeComboBox = new ComboBoxText();
        yabridgeComboBox.AppendText("sync");
        yabridgeComboBox.AppendText("status");
        yabridgeComboBox.AppendText("prune");
        yabridgeComboBox.AppendText("list");
        yabridgeComboBox.AppendText("verbose");
        yabridgeComboBox.AppendText("help");
        hbox.PackStart(yabridgeComboBox, false, false, 5);

        topControlsBox.PackStart(hbox, false, true, 5);

        // Create a button
          var yabridgeButton = new Button("Run");
          yabridgeButton.Clicked += async (sender, e) =>
          {
                string selectedValue = yabridgeComboBox.ActiveText;
                string outCommand = "";
                switch (selectedValue)
          {
              case "sync":
                  outCommand = "$HOME/.local/share/yabridge/yabridgectl sync";
              break;
              case "status":
                  outCommand = "$HOME/.local/share/yabridge/yabridgectl status";
              break;
              case "prune":
              	outCommand = "$HOME/.local/share/yabridge/yabridgectl sync -p";
              break;
              case "list":
              	outCommand = "$HOME/.local/share/yabridge/yabridgectl list";
              break;
              case "verbose":
              	outCommand = "$HOME/.local/share/yabridge/yabridgectl sync -v";
              break;
              case "help":
                   ProcessUtils.RunCommand(@"echo Below is a list of commands that can be executed from AudioTools. For advanced steps visit https://github.com/robbert-vdh/yabridge.
                   	echo -------------------------------
                   	echo - sync: performs a sync between the paths for installed plugins and yabridge. 
                   	echo - status: view the currently synced plugins and review their install locations.
                   	echo - prune: performs a sync but also checks for plugins that are no longer installed and removes them from yabridge.
                   	echo - list: views the directories that yabridge looks for plugins within.
                   	echo - verbose: forces resync with all plugins. CAUTION: you may need to re-register/re-activate your plugins after running this command. Use only for last-resort debugging.", s => AppendOutput(s));
              break;
              default:
                  outCommand = "";
              break;
          }

                if (string.IsNullOrWhiteSpace(outCommand)) return;

                // Log the command and environment for diagnostics
                try { Logger.LogCommand(outCommand, selectedValue); } catch { }

                // Long-running operations (sync/verbose) get a cancellable progress dialog
                bool isLong = selectedValue == "sync" || selectedValue == "verbose";
                if (isLong)
                {
                    var cts = new CancellationTokenSource();
                    var dlg = new ProgressDialog(window, "yabridgectl");
                    dlg.OnCancel += () =>
                    {
                        AppendOutput("Cancellation requested...");
                        try { cts.Cancel(); } catch { }
                    };

                    yabridgeButton.Sensitive = false;
                    topControlsBox.Sensitive = false;
                    var progress = new Progress<string>(s => { AppendOutput(s); dlg.AppendLog(s); });
                    try
                    {
                        await YabridgeService.RunCommandAsync(outCommand, progress, cts.Token);
                        // persist last action
                        SettingsManager.Instance.LastYabridgeAction = selectedValue;
                        SettingsManager.Save();
                    }
                    catch (OperationCanceledException)
                    {
                        AppendOutput("Operation cancelled.");
                    }
                    finally
                    {
                        dlg.Close();
                        yabridgeButton.Sensitive = true;
                        topControlsBox.Sensitive = true;
                    }
                }
                else
                {
                    yabridgeButton.Sensitive = false;
                    topControlsBox.Sensitive = false;
                    var progress = new Progress<string>(s => AppendOutput(s));
                    try
                    {
                        await YabridgeService.RunCommandAsync(outCommand, progress, CancellationToken.None);
                        // persist last action
                        SettingsManager.Instance.LastYabridgeAction = selectedValue;
                        SettingsManager.Save();
                    }
                    finally
                    {
                        yabridgeButton.Sensitive = true;
                        topControlsBox.Sensitive = true;
                    }
                }
          };
        hbox.PackStart(yabridgeButton, false, false, 5);

/*
        hbox = new HBox();

        // Create the buttons with customizable properties
        var syncButton = CreateButton("yabridgectl sync", 150, 50);
        syncButton.Clicked += (sender, e) => RunCommand("$HOME/.local/share/yabridge/yabridgectl sync");

        var statusButton = CreateButton("yabridgectl status", 150, 50);
        statusButton.Clicked += (sender, e) => RunCommand("$HOME/.local/share/yabridge/yabridgectl status");

        var pruneButton = CreateButton("yabridgectl prune", 150, 50);
        pruneButton.Clicked += (sender, e) => RunCommand("$HOME/.local/share/yabridge/yabridgectl sync --prune");

        hbox.PackStart(syncButton, true, true, 5);
        hbox.PackStart(statusButton, true, true, 5);
        hbox.PackStart(pruneButton, true, true, 5);

        vbox.PackStart(hbox, false, false, 5);
*/


        // Create the Clear button
        var clearButton = CreateButton("Clear", 140, 36);
        clearButton.Clicked += (sender, e) => ClearOutput();

        // Create the text view for output
        outputTextView = new TextView
        {
            Editable = false,
            WrapMode = WrapMode.Word
        };

        // Create a ScrolledWindow and add the TextView to it
        var scrolledWindow = new ScrolledWindow();
        scrolledWindow.Add(outputTextView);

        // Add margins around output text area and clear button.
        var outputSectionBox = new VBox(false, 5)
        {
            BorderWidth = 10
        };

        // Add the controls section and output area.
        vbox.PackStart(topControlsBox, false, false, 0);
        outputSectionBox.PackStart(scrolledWindow, true, true, 0);
        outputSectionBox.PackStart(clearButton, false, false, 0);
        vbox.PackStart(outputSectionBox, true, true, 0);

        // Create the EventBox container which will handle clicks
        EventBox eventBox = new EventBox();
        eventBox.Events |= Gdk.EventMask.ButtonPressMask;

        // Create styled label (looks like a hyperlink)
        Label linkLabel = new Label();
        linkLabel.Markup = "<span foreground=\"aqua\" underline=\"single\">Click here to visit the Wiki!</span>";
        linkLabel.Selectable = false;

        // Handle click to open URL
        eventBox.ButtonPressEvent += (sender, args) =>
        {
            if (args.Event.Button == 1) // Left mouse button only
            {
                try
                {
                    Process.Start("xdg-open", "https://github.com/Heroin-Bob/AudioTools-for-Linux/wiki");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error opening link: {ex.Message}");
                }
            }
        };

        // Add label to event box, event box to vbox
        eventBox.Add(linkLabel);

        vbox.PackStart(eventBox, false, false, 0);

        // Set the policy for the scrollbars
        scrolledWindow.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);

        // Add the vertical box to the window
        window.Add(vbox);
        window.ShowAll();


	string initCommand = @"#!/bin/bash        	
			sampleRate=$(pw-metadata -n settings 0 clock.force-rate | grep -oP '\d+' | tail -n 1)
			if [ $sampleRate -eq 0 ]; then
				sampleRate=$(pw-metadata -n settings 0 clock.rate | grep -oP '\d+' | tail -n 2 | head -n 1)
			fi

			bufferSize=$(pw-metadata -n settings 0 clock.force-quantum | grep -oP '\d+' | tail -n 1)
			if [ $bufferSize -eq 0 ]; then
				bufferSize=$(pw-metadata -n settings 0 clock.rate | grep -oP '\d+' | tail -n 2 | head -n 1)
			fi

			num_plugins=$(echo $($HOME/.local/share/yabridge/yabridgectl sync) | grep -oP '\d+' | tail -n 3 | head -n 1)
			num_new_plugins=$(echo $($HOME/.local/share/yabridge/yabridgectl sync) | grep -oP '\d+' | tail -n 2 | head -n 1)

			playbackDevices=$(pactl list sinks | grep -A1 Name:\ $(pactl get-default-sink) | grep Description | cut -d : -f2 | xargs)
			recordingDevices=$(pactl list sources | grep -A1 Name:\ $(pactl get-default-source) | grep Description | cut -d : -f2 | xargs)

			# Print the results
			clear
			echo Playback Device: $playbackDevices
			echo Recording Device: $recordingDevices
			echo ' '
			echo Number of plugins: $num_plugins
			echo Number of new plugins: $num_new_plugins
			echo Sample Rate: $sampleRate
			echo Buffer Size: $bufferSize";
        ProcessUtils.RunCommand(initCommand, s => AppendOutput(s));


        Application.Run();
    }


    private Button CreateButton(string label, int width, int height)
    {
        var button = new Button(label)
        {
            WidthRequest = width,
            HeightRequest = height
        };
        return button;
    }


    

    private void ClearOutput()
    {
        // Clear the text view
        outputTextView.Buffer.Text = "";
    }

    private void AppendOutput(string text)
    {
        // Update the text view in the UI thread
        Application.Invoke(delegate
        {
            try
            {
                var buffer = outputTextView.Buffer;
                var appendText = text.Contains("TERM environment variable not set")
                    ? text.Replace("Error: TERM environment variable not set.", "")
                    : text + "\n";

                var end = buffer.EndIter;
                buffer.Insert(ref end, appendText);

                // Create temporary mark at the iterator returned from the insert (ensures mark is at new end)
                var mark = buffer.CreateMark(null, end, false);
                try
                {
                    // Use alignment when scrolling so yalign=1.0 forces the view to the very bottom.
                    outputTextView.ScrollToMark(mark, 0, true, 0, 1.0);
                }
                finally
                {
                    try { buffer.DeleteMark(mark); } catch { }
                }
            }
            catch { }
        });
    }

    private Task<T> InvokeOnGtkThreadAsync<T>(Func<T> func)
    {
        var tcs = new TaskCompletionSource<T>();
        Application.Invoke(delegate
        {
            try
            {
                tcs.SetResult(func());
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });
        return tcs.Task;
    }
    
    static bool IsPath(string line)
    {
        // Check for common path patterns (you can customize this)
        return line.StartsWith("/") || line.Contains("\\") || line.Contains(":");
    }

    private void ShowPathManagerWindow()
    {
        // Create the Path Manager window
        var pathWindow = new Window("Manage Plugin Paths");
        pathWindow.SetDefaultSize(500, 300);
        pathWindow.SetPosition(WindowPosition.Center);
        pathWindow.DeleteEvent += (o, args) => pathWindow.Destroy();

        // Create main vertical box
        var vbox = new VBox(false, 5);

        // Create TreeView and ListStore for paths
        var listStore = new ListStore(typeof(string));
        var treeView = new TreeView(listStore);
        treeView.HeadersVisible = true;
        
        var column = new TreeViewColumn();
        column.Title = "Plugin Paths";
        var cell = new CellRendererText();
        column.PackStart(cell, true);
        column.AddAttribute(cell, "text", 0);
        treeView.AppendColumn(column);

        // Add TreeView to ScrolledWindow
        var scrolledWindow = new ScrolledWindow();
        scrolledWindow.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
        scrolledWindow.Add(treeView);
        vbox.PackStart(scrolledWindow, true, true, 5);

        // Create HBox for buttons
        var buttonBox = new HBox(false, 5);

        // Add button
        var addButton = new Button("Add");
        addButton.Clicked += (sender, e) => 
        { 
            _ = AddPathAsync(listStore);
        };
        buttonBox.PackStart(addButton, true, true, 5);

        // Remove button
        var removeButton = new Button("Remove");
        removeButton.Clicked += (sender, e) => 
        { 
            _ = RemovePathAsync(treeView, listStore);
        };
        buttonBox.PackStart(removeButton, true, true, 5);

        // Close button
        var closeButton = new Button("Close");
        closeButton.Clicked += (sender, e) => pathWindow.Destroy();
        buttonBox.PackStart(closeButton, true, true, 5);

        vbox.PackStart(buttonBox, false, false, 5);

        // Add vbox to window
        pathWindow.Add(vbox);
        
        // Load initial paths
        _ = RefreshPathListAsync(listStore);
        
        pathWindow.ShowAll();
    }

    private void ShowPluginBrowserWindow()
    {
        try
        {
            // 1. Create window inline, exactly like PathManager
            var pluginWindow = new Window("Plugin Browser");
            pluginWindow.SetDefaultSize(900, 550);
            pluginWindow.SetPosition(WindowPosition.Center);
            pluginWindow.DeleteEvent += (o, args) => pluginWindow.Destroy();

            var vbox = new VBox(false, 6);

            // 2. Setup TreeView and Store
            var pluginStore = new ListStore(typeof(string), typeof(string), typeof(string), typeof(string));
            var treeView = new TreeView(pluginStore);
            treeView.HeadersVisible = true;

            string[] columns = { "Name", "Type", "Location", "Action" };
            for (int i = 0; i < columns.Length; i++)
            {
                var col = new TreeViewColumn { Title = columns[i] };
                var cell = new CellRendererText();
                col.PackStart(cell, true);
                col.AddAttribute(cell, "text", i);
                treeView.AppendColumn(col);
            }

            // 3. Handle double-click / Row activation
            treeView.RowActivated += (sender, args) => 
            {
                if (pluginStore.GetIter(out TreeIter iter, args.Path))
                {
                    var actionValue = pluginStore.GetValue(iter, 3)?.ToString();
                    if (actionValue == "Open Folder")
                    {
                        var location = pluginStore.GetValue(iter, 2)?.ToString();
                        if (!string.IsNullOrWhiteSpace(location) && location != "-")
                        {
                            OpenFolderLocation(pluginWindow, location);
                        }
                    }
                }
            };

            var scrolledWindow = new ScrolledWindow();
            scrolledWindow.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
            scrolledWindow.Add(treeView);
            vbox.PackStart(scrolledWindow, true, true, 5);

            var refreshButton = new Button("Refresh");
            refreshButton.Clicked += async (sender, e) =>
            {
                refreshButton.Sensitive = false;
                var cts = new CancellationTokenSource();
                try
                {
                    await RefreshPluginList(pluginStore, cts.Token);
                }
                finally
                {
                    refreshButton.Sensitive = true;
                }
            };
            vbox.PackStart(refreshButton, false, false, 5);

            pluginWindow.Add(vbox);
            
            // 4. Load plugins immediately before showing the window (removes the 'Shown' event trap)
            _ = RefreshPluginList(pluginStore);
            
            pluginWindow.ShowAll();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error opening plugin browser: " + ex.Message);
        }
    }

    private void OpenFolderLocation(Window parent, string location)
    {
        // Try letting the OS open the folder first (works for desktop environments and published single-file apps)
        try
        {
            var shellStart = new ProcessStartInfo
            {
                FileName = location,
                UseShellExecute = true
            };

            try
            {
                var started = Process.Start(shellStart);
                if (started != null)
                {
                    return;
                }
            }
            catch
            {
                // Continue to fallbacks below
            }

            // Fallback to xdg-open variants and capture stderr for diagnostics
            string[] candidates = { "/usr/bin/xdg-open", "/bin/xdg-open", "xdg-open" };
            var errors = new System.Collections.Generic.List<string>();

            foreach (var candidate in candidates)
            {
                if (candidate.StartsWith("/") && !File.Exists(candidate))
                {
                    continue;
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = candidate,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                startInfo.ArgumentList.Add(location);

                try
                {
                    using var proc = Process.Start(startInfo);
                    if (proc == null)
                    {
                        errors.Add($"{candidate}: failed to start process.");
                        continue;
                    }

                    var stdErr = proc.StandardError.ReadToEnd();
                    var stdOut = proc.StandardOutput.ReadToEnd();
                    proc.WaitForExit();

                    if (proc.ExitCode == 0)
                    {
                        return;
                    }

                    var combined = stdErr;
                    if (string.IsNullOrWhiteSpace(combined)) combined = stdOut;
                    if (string.IsNullOrWhiteSpace(combined)) combined = $"{candidate} exited with code {proc.ExitCode}";
                    errors.Add(combined.Trim());
                }
                catch (Exception ex)
                {
                    errors.Add($"{candidate}: {ex.Message}");
                }
            }

            throw new InvalidOperationException(string.Join("\n", errors));
        }
        catch (Exception ex)
        {
            using var dialog = new MessageDialog(
                parent,
                DialogFlags.Modal,
                MessageType.Error,
                ButtonsType.Ok,
                $"Failed to open plugin folder.\n\nPath: {location}\n\n{ex.Message}\n\nTip: Ensure xdg-open is installed and available in this app's runtime PATH.");
            dialog.Run();
            dialog.Destroy();
        }
    }

    private async Task RefreshPluginList(ListStore pluginStore, CancellationToken ct = default)
    {
        pluginStore.Clear();
        try
        {
            var statusOutput = await YabridgeService.RunCommandWithReturnAsync("$HOME/.local/share/yabridge/yabridgectl status", ct);
            var plugins = PluginParser.ParsePluginStatus(statusOutput);

            if (plugins.Count == 0)
            {
                Application.Invoke(delegate { pluginStore.AppendValues("No plugins found", "-", "-", "-"); });
                return;
            }

            foreach (var plugin in plugins)
            {
                if (ct.IsCancellationRequested) break;
                var name = plugin.Name;
                var type = plugin.Type;
                var location = plugin.Location;
                Application.Invoke(delegate { pluginStore.AppendValues(name, type, location, "Open Folder"); });
            }
        }
        catch (OperationCanceledException)
        {
            // ignore cancellation
        }
        catch (Exception ex)
        {
            Application.Invoke(delegate { pluginStore.AppendValues("Error loading plugins", "-", ex.Message, "-"); });
        }
    }

    

    private async Task RefreshPathListAsync(ListStore listStore, CancellationToken ct = default)
    {
        try
        {
            AppendOutput("[RefreshPathListAsync] Starting...");

            // Run yabridgectl list command
            AppendOutput("[RefreshPathListAsync] Calling yabridgectl list...");
            string output = await YabridgeService.RunCommandWithReturnAsync("$HOME/.local/share/yabridge/yabridgectl list", ct);
            AppendOutput($"[RefreshPathListAsync] Got output, length={output.Length}");

            // Parse output and replace the full list in one GTK invoke so the UI stays in sync.
            var lines = output.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            AppendOutput($"[RefreshPathListAsync] Parsed {lines.Length} lines");

            int addedCount = 0;
            Application.Invoke(delegate
            {
                listStore.Clear();
                foreach (var line in lines)
                {
                    string trimmedLine = line.Trim();
                    if (trimmedLine.StartsWith("/") || trimmedLine.StartsWith("~"))
                    {
                        listStore.AppendValues(trimmedLine);
                        addedCount++;
                    }
                }
            });

            AppendOutput($"[RefreshPathListAsync] Added {addedCount} paths to list");
            AppendOutput("[RefreshPathListAsync] Complete");
        }
        catch (OperationCanceledException)
        {
            AppendOutput("[RefreshPathListAsync] Cancelled");
        }
        catch (Exception ex)
        {
            AppendOutput($"[RefreshPathListAsync] EXCEPTION: {ex.GetType().Name} - {ex.Message}");
            throw;
        }
    }

    private async Task AddPathAsync(ListStore listStore)
    {
        try
        {
            var fileChooser = new FileChooserDialog(
                "Select Plugin Directory",
                null,
                FileChooserAction.SelectFolder,
                "Cancel", ResponseType.Cancel,
                "Select", ResponseType.Accept);

            if (fileChooser.Run() == (int)ResponseType.Accept)
            {
                string selectedPath = fileChooser.Filename;
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    // Add path using yabridgectl asynchronously
                    Application.Invoke(delegate { topControlsBox.Sensitive = false; });
                    var progress = new Progress<string>(s => AppendOutput(s));
                    try
                    {
                        AppendOutput("[AddPathAsync] Running yabridgectl add...");
                        int exitCode = await YabridgeService.RunCommandAsync($"$HOME/.local/share/yabridge/yabridgectl add \"{selectedPath}\"", progress);
                        if (exitCode != 0)
                        {
                            AppendOutput($"[AddPathAsync] Command failed with exit code {exitCode}. Aborting.");
                        }
                        else
                        {
                            AppendOutput("[AddPathAsync] add command completed. Waiting for yabridge to process...");
                            // Give yabridge a moment to actually process the addition
                            await Task.Delay(500);
                            // Refresh the list
                            try
                            {
                                AppendOutput("Refreshing path list...");
                                await RefreshPathListAsync(listStore);
                                AppendOutput("Path list refreshed.");
                            }
                            catch (Exception ex)
                            {
                                AppendOutput($"ERROR refreshing path list: {ex.Message}");
                            }
                            // Ask user whether to run sync now (marshalled to GTK thread)
                            bool userConfirmedSync = await InvokeOnGtkThreadAsync(() =>
                            {
                                var syncConfirm = new MessageDialog(null,
                                    DialogFlags.Modal,
                                    MessageType.Question,
                                    ButtonsType.YesNo,
                                    "Run yabridgectl sync now to update registry?");

                                bool result = syncConfirm.Run() == (int)ResponseType.Yes;
                                syncConfirm.Destroy();
                                return result;
                            });

                            if (userConfirmedSync)
                            {
                                var ctsSync = new CancellationTokenSource();
                                var syncDlg = await InvokeOnGtkThreadAsync(() =>
                                    new ProgressDialog(null, "yabridgectl sync"));

                                syncDlg.OnCancel += () => { AppendOutput("Cancellation requested..."); try { ctsSync.Cancel(); } catch { } };
                                
                                try
                                {
                                    var progressSync = new Progress<string>(s => { AppendOutput(s); syncDlg.AppendLog(s); });
                                    await YabridgeService.RunCommandAsync("$HOME/.local/share/yabridge/yabridgectl sync", progressSync, ctsSync.Token);
                                }
                                catch (OperationCanceledException)
                                {
                                    AppendOutput("Sync cancelled.");
                                }
                                finally
                                {
                                    await InvokeOnGtkThreadAsync(() => { syncDlg.Close(); return (object)null; });
                                }
                            }
                        }
                    }
                    finally
                    {
                        Application.Invoke(delegate { topControlsBox.Sensitive = true; });
                    }
                }
            }

            fileChooser.Destroy();
        }
        catch (Exception ex)
        {
            AppendOutput($"Error adding path: {ex.Message}");
            Application.Invoke(delegate { topControlsBox.Sensitive = true; });
        }
    }

    private async Task RemovePathAsync(TreeView treeView, ListStore listStore)
    {
        TreeIter iter;
        ITreeModel model;

        try
        {
            if (treeView.Selection.GetSelected(out model, out iter))
            {
                string selectedPath = (string)model.GetValue(iter, 0);

                AppendOutput($"[RemovePathAsync] Workaround enabled: auto-accepting removal for '{selectedPath}'.");

                // Remove path using yabridgectl asynchronously
                Application.Invoke(delegate { topControlsBox.Sensitive = false; });
                var progress = new Progress<string>(s => AppendOutput(s));
                try
                {
                    AppendOutput($"[RemovePathAsync] Running yabridgectl rm (thread={Environment.CurrentManagedThreadId})...");
                    int exitCode = await YabridgeService.RunCommandAsync($"yes | $HOME/.local/share/yabridge/yabridgectl rm \"{selectedPath}\"", progress);
                    AppendOutput($"[RemovePathAsync] rm finished with exit code {exitCode}.");

                    // Give yabridge a moment to process filesystem/index updates.
                    await Task.Delay(500);
                    await RefreshPathListAsync(listStore);

                    // Ask user whether to run sync now (marshalled to GTK thread)
                    bool userConfirmedSync = await InvokeOnGtkThreadAsync(() =>
                    {
                        var syncConfirm = new MessageDialog(null,
                            DialogFlags.Modal,
                            MessageType.Question,
                            ButtonsType.YesNo,
                            "Run yabridgectl sync now to update registry?");

                        bool result = syncConfirm.Run() == (int)ResponseType.Yes;
                        syncConfirm.Destroy();
                        return result;
                    });

                    if (!userConfirmedSync)
                    {
                        AppendOutput("[RemovePathAsync] Sync prompt declined by user.");
                        return;
                    }

                    var ctsSync = new CancellationTokenSource();
                    var syncDlg = await InvokeOnGtkThreadAsync(() =>
                        new ProgressDialog(null, "yabridgectl sync"));

                    syncDlg.OnCancel += () => { AppendOutput("Cancellation requested..."); try { ctsSync.Cancel(); } catch { } };

                    try
                    {
                        var progressSync = new Progress<string>(s => { AppendOutput(s); syncDlg.AppendLog(s); });
                        await YabridgeService.RunCommandAsync("$HOME/.local/share/yabridge/yabridgectl sync", progressSync, ctsSync.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        AppendOutput("Sync cancelled.");
                    }
                    finally
                    {
                        await InvokeOnGtkThreadAsync(() => { syncDlg.Close(); return (object)null; });
                    }
                }
                catch (Exception ex)
                {
                    AppendOutput($"[RemovePathAsync] ERROR: {ex.Message}");
                    AppendOutput($"[RemovePathAsync] STACK: {ex.StackTrace}");
                }
                finally
                {
                    Application.Invoke(delegate { topControlsBox.Sensitive = true; });
                    AppendOutput("[RemovePathAsync] UI controls re-enabled.");
                }
            }
            else
            {
                var dialog = new MessageDialog(
                    null,
                    DialogFlags.Modal,
                    MessageType.Info,
                    ButtonsType.Ok,
                    "Please select a path to remove.");
                dialog.Run();
                dialog.Destroy();
            }
        }
        catch (Exception ex)
        {
            AppendOutput($"Error removing path: {ex.Message}");
            Application.Invoke(delegate { topControlsBox.Sensitive = true; });
        }
    }

    public static void Main()
    {
        new AudioTools();
    }
}

// Small progress dialog used for long-running yabridgectl operations.
public class ProgressDialog
{
    private Dialog dialog;
    private TextView textView;
    private Spinner spinner;
    private Button cancelButton;
    public System.Action OnCancel;

    public ProgressDialog(Window parent, string title = "Progress")
    {
        dialog = new Dialog(title, parent, DialogFlags.Modal);
        dialog.SetDefaultSize(700, 360);

        var content = dialog.ContentArea;

        var hbox = new HBox(false, 6);
        spinner = new Spinner();
        spinner.Start();
        hbox.PackStart(spinner, false, false, 6);

        textView = new TextView { Editable = false, WrapMode = WrapMode.Word };
        var sw = new ScrolledWindow();
        sw.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
        sw.Add(textView);
        hbox.PackStart(sw, true, true, 6);

        content.PackStart(hbox, true, true, 6);

        cancelButton = new Button("Cancel");
        cancelButton.Clicked += (s, e) => OnCancel?.Invoke();
        dialog.AddActionWidget(cancelButton, ResponseType.Cancel);

        dialog.ShowAll();
    }

    public void AppendLog(string line)
    {
        Application.Invoke(delegate
        {
            textView.Buffer.Text += line + "\n";
            textView.ScrollToIter(textView.Buffer.EndIter, 0, false, 0, 0);
        });
    }

    public void Close()
    {
        Application.Invoke(delegate { try { dialog.Destroy(); } catch { } });
    }
}
