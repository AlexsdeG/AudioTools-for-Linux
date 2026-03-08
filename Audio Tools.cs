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
	
        
        // Create the main window
        var appVersion = "0.96";
        var window = new Window("AudioTools v" + appVersion);
        window.SetDefaultSize(400, 500);
        window.SetPosition(WindowPosition.Center);
        window.DeleteEvent += (o, args) => Application.Quit();

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
			RunCommand("winetricks");
		};
		optionsMenu.Append(winetricksItem);
		
		// Launch winecfg
		var winecfgItem = new MenuItem("Open WINE config");
		winecfgItem.Activated += (sender, e) => {
			RunCommand("winecfg");
		};
		optionsMenu.Append(winecfgItem);
		
		// Launch wine uninstaller
		var wineuninstItem = new MenuItem("Open WINE programs");
		wineuninstItem.Activated += (sender, e) => {
			RunCommand("wine uninstaller");
		};
		optionsMenu.Append(wineuninstItem);
		
		// Check for updates
		var updateItem = new MenuItem("Check for Updates");
		updateItem.Activated += (sender, e) => 
        {
        	var yabridgeVersion = "";
	        var checkyabridgeVersion = "";
	        var checkAudioToolsVersion = "";
	        yabridgeVersion = RunCommandWithReturn("$HOME/.local/share/yabridge/yabridgectl --version");
        
	        checkyabridgeVersion = RunCommandWithReturn("wget -qO- https://api.github.com/repos/robbert-vdh/yabridge/releases/latest | jq -r '.tag_name'");
      		checkAudioToolsVersion = RunCommandWithReturn("wget -qO- https://api.github.com/repos/Heroin-Bob/AudioTools-for-Linux/releases/latest | jq -r '.tag_name'");
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
            RunCommand(concatCommand);
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
            RunCommand(concatCommand);
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
              	RunCommand(@"echo Below is a list of commands that can be executed from AudioTools. For advanced steps visit https://github.com/robbert-vdh/yabridge.
              	echo -------------------------------
              	echo - sync: performs a sync between the paths for installed plugins and yabridge. 
              	echo - status: view the currently synced plugins and review their install locations.
              	echo - prune: performs a sync but also checks for plugins that are no longer installed and removes them from yabridge.
              	echo - list: views the directories that yabridge looks for plugins within.
              	echo - verbose: forces resync with all plugins. CAUTION: you may need to re-register/re-activate your plugins after running this command. Use only for last-resort debugging.");
              break;
              default:
                  outCommand = "";
              break;
          }

                if (string.IsNullOrWhiteSpace(outCommand)) return;

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
                        await RunCommandAsync(outCommand, progress, cts.Token);
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
                        await RunCommandAsync(outCommand, progress, CancellationToken.None);
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
        RunCommand(initCommand);


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


    private void RunCommand(string command)
    {
    	var startIter = outputTextView.Buffer.StartIter;
        outputTextView.ScrollToIter(startIter, 0, true, 0, 0);
        // Clear previous output
        outputTextView.Buffer.Text = "";
	
	
        // Set up the process start info
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "/bin/bash",
            Arguments = $"-c \"{command}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        // Start the process
        using (var process = new Process { StartInfo = processStartInfo })
        {
            process.OutputDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {

                    AppendOutput(args.Data);

                }
            };

            process.ErrorDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    AppendOutput("Error: " + args.Data);


                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
        }
    }

    public static async Task<string> RunCommandWithReturnAsync(string command, CancellationToken ct = default)
    {
        var output = new List<string>();

        var processStartInfo = new ProcessStartInfo
        {
            FileName = "/bin/bash",
            Arguments = $"-c \"{command}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var process = new Process { StartInfo = processStartInfo, EnableRaisingEvents = true })
        {
            var outputTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            process.OutputDataReceived += (sender, args) =>
            {
                if (args.Data == null)
                {
                    return;
                }
                output.Add(args.Data);
            };

            process.ErrorDataReceived += (sender, args) =>
            {
                if (args.Data == null)
                {
                    return;
                }
                output.Add("Error: " + args.Data);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            using (ct.Register(() =>
            {
                try { if (!process.HasExited) process.Kill(true); } catch { }
            }))
            {
                await process.WaitForExitAsync(ct);
            }
        }

        return string.Join("\n", output);
    }

    // Compatibility synchronous wrapper (keeps existing callsites working during refactor)
    public static string RunCommandWithReturn(string command)
    {
        return RunCommandWithReturnAsync(command).GetAwaiter().GetResult();
    }

    public static async Task RunCommandAsync(string command, IProgress<string> progress, CancellationToken ct = default)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "/bin/bash",
            Arguments = $"-c \"{command}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var process = new Process { StartInfo = processStartInfo, EnableRaisingEvents = true })
        {
            process.OutputDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    progress?.Report(args.Data);
                }
            };

            process.ErrorDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    progress?.Report("Error: " + args.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            using (ct.Register(() =>
            {
                try { if (!process.HasExited) process.Kill(true); } catch { }
            }))
            {
                await process.WaitForExitAsync(ct);
            }
        }
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
            if (text.Contains("TERM environment variable not set"))
            {
                outputTextView.Buffer.Text += text.Replace("Error: TERM environment variable not set.", "");
            }
            else
            {
                outputTextView.Buffer.Text += text + "\n";
            }

            outputTextView.ScrollToIter(outputTextView.Buffer.EndIter, 0, false, 0, 0);
        });
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
        addButton.Clicked += async (sender, e) => await AddPathAsync(listStore);
        buttonBox.PackStart(addButton, true, true, 5);

        // Remove button
        var removeButton = new Button("Remove");
        removeButton.Clicked += async (sender, e) => await RemovePathAsync(treeView, listStore);
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
            var statusOutput = await RunCommandWithReturnAsync("$HOME/.local/share/yabridge/yabridgectl status", ct);
            var plugins = ParsePluginStatus(statusOutput);

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

    private List<(string Name, string Type, string Location)> ParsePluginStatus(string output)
    {
        var plugins = new List<(string Name, string Type, string Location)>();
        var lines = output.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        string currentDirectory = string.Empty;

        foreach (var rawLine in lines)
        {
            var line = rawLine.TrimEnd();
            var trimmed = line.Trim();

            if (string.IsNullOrWhiteSpace(trimmed)) continue;

            if (trimmed.StartsWith("/") && !trimmed.Contains("::"))
            {
                currentDirectory = trimmed.TrimEnd('/');
                continue;
            }

            if (!line.StartsWith("  ") || !trimmed.Contains("::")) continue;

            var pluginParts = trimmed.Split(new[] { "::" }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (pluginParts.Length < 2) continue;

            var relativePluginPath = pluginParts[0].Trim();
            var metadataTokens = pluginParts[1].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var pluginType = metadataTokens.Length > 0 ? metadataTokens[0].Trim() : "Unknown";

            var relativeDirectory = System.IO.Path.GetDirectoryName(relativePluginPath);
            var location = string.IsNullOrEmpty(relativeDirectory)
                ? currentDirectory
                : System.IO.Path.Combine(currentDirectory, relativeDirectory).Replace("\\", "/");

            var pluginName = System.IO.Path.GetFileNameWithoutExtension(relativePluginPath);
            if (string.IsNullOrEmpty(pluginName)) pluginName = relativePluginPath;

            plugins.Add((pluginName, pluginType, location));
        }

        return plugins;
    }

    private async Task RefreshPathListAsync(ListStore listStore, CancellationToken ct = default)
    {
        Application.Invoke(delegate { listStore.Clear(); });

        // Run yabridgectl list command
        string output = await RunCommandWithReturnAsync("$HOME/.local/share/yabridge/yabridgectl list", ct);

        // Parse output and add paths to list
        var lines = output.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            string trimmedLine = line.Trim();
            // Filter out header/footer text - only add lines that look like paths
            if (trimmedLine.StartsWith("/") || trimmedLine.StartsWith("~"))
            {
                Application.Invoke(delegate { listStore.AppendValues(trimmedLine); });
            }
        }
    }

    private async Task AddPathAsync(ListStore listStore)
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
                topControlsBox.Sensitive = false;
                var progress = new Progress<string>(s => AppendOutput(s));
                try
                {
                    await RunCommandAsync($"$HOME/.local/share/yabridge/yabridgectl add \"{selectedPath}\"", progress);
                    // Refresh the list
                    await RefreshPathListAsync(listStore);
                    // Ask user whether to run sync now
                    var syncConfirm = new MessageDialog(null,
                        DialogFlags.Modal,
                        MessageType.Question,
                        ButtonsType.YesNo,
                        "Run yabridgectl sync now to update registry?");

                    if (syncConfirm.Run() == (int)ResponseType.Yes)
                    {
                        syncConfirm.Destroy();
                        var ctsSync = new CancellationTokenSource();
                        var syncDlg = new ProgressDialog(null, "yabridgectl sync");
                        syncDlg.OnCancel += () => { AppendOutput("Cancellation requested..."); try { ctsSync.Cancel(); } catch { } };
                        try
                        {
                            var progressSync = new Progress<string>(s => { AppendOutput(s); syncDlg.AppendLog(s); });
                            await RunCommandAsync("$HOME/.local/share/yabridge/yabridgectl sync", progressSync, ctsSync.Token);
                        }
                        catch (OperationCanceledException)
                        {
                            AppendOutput("Sync cancelled.");
                        }
                        finally
                        {
                            syncDlg.Close();
                        }
                    }
                    else
                    {
                        syncConfirm.Destroy();
                    }
                }
                finally
                {
                    topControlsBox.Sensitive = true;
                }
            }
        }

        fileChooser.Destroy();
    }

    private async Task RemovePathAsync(TreeView treeView, ListStore listStore)
    {
        TreeIter iter;
        ITreeModel model;

        if (treeView.Selection.GetSelected(out model, out iter))
        {
            string selectedPath = (string)model.GetValue(iter, 0);

            // Confirm removal
            var dialog = new MessageDialog(
                null,
                DialogFlags.Modal,
                MessageType.Question,
                ButtonsType.YesNo,
                $"Remove path:\n{selectedPath}?");

            if (dialog.Run() == (int)ResponseType.Yes)
            {
                // Remove path using yabridgectl asynchronously
                topControlsBox.Sensitive = false;
                var progress = new Progress<string>(s => AppendOutput(s));
                try
                {
                    await RunCommandAsync($"$HOME/.local/share/yabridge/yabridgectl rm \"{selectedPath}\"", progress);
                    // Refresh the list
                    await RefreshPathListAsync(listStore);
                        // Ask user whether to run sync now
                        var syncConfirm = new MessageDialog(null,
                            DialogFlags.Modal,
                            MessageType.Question,
                            ButtonsType.YesNo,
                            "Run yabridgectl sync now to update registry?");

                        if (syncConfirm.Run() == (int)ResponseType.Yes)
                        {
                            syncConfirm.Destroy();
                            var ctsSync = new CancellationTokenSource();
                            var syncDlg = new ProgressDialog(null, "yabridgectl sync");
                            syncDlg.OnCancel += () => { AppendOutput("Cancellation requested..."); try { ctsSync.Cancel(); } catch { } };
                            try
                            {
                                var progressSync = new Progress<string>(s => { AppendOutput(s); syncDlg.AppendLog(s); });
                                await RunCommandAsync("$HOME/.local/share/yabridge/yabridgectl sync", progressSync, ctsSync.Token);
                            }
                            catch (OperationCanceledException)
                            {
                                AppendOutput("Sync cancelled.");
                            }
                            finally
                            {
                                syncDlg.Close();
                            }
                        }
                        else
                        {
                            syncConfirm.Destroy();
                        }
                }
                finally
                {
                    topControlsBox.Sensitive = true;
                }
            }

            dialog.Destroy();
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
