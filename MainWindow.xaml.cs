using System.Diagnostics;
using System.Management;
using System.Windows;

namespace RestorePoint
{
    public partial class MainWindow : Window
    {
        private List<(string Description, DateTime CreationTime)> _points = new();

        public MainWindow()
        {
            InitializeComponent();
            LoadRestorePoints();
        }

        private void LoadRestorePoints()
        {
            _points = GetRestorePoints()
                .OrderByDescending(p => p.CreationTime)
                .ToList();

            RestorePointsList.Items.Clear();

            if (_points.Count == 0)
            {
                RestorePointsList.Items.Add("  No restore points found.");
                SetStatus("No restore points found.");
                return;
            }

            foreach (var point in _points)
            {
                RestorePointsList.Items.Add($"  {point.Description}   —   {point.CreationTime:yyyy-MM-dd HH:mm:ss}");
            }

            SetStatus($"{_points.Count} restore point(s) found.");
        }

        private void CreateClick(object sender, RoutedEventArgs e)
        {
            var dialog = new CreateDialog { Owner = this };
            if (dialog.ShowDialog() != true)
            {
                return;
            }

            var name = dialog.RestorePointName;
            SetStatus($"Creating restore point \"{name}\"...");

            try
            {
                SetRegistryBypass(true);

                var scope = new ManagementScope(@"\\.\root\default");
                scope.Connect();

                using var restoreClass = new ManagementClass(scope, new ManagementPath("SystemRestore"), null);
                using var inParams = restoreClass.GetMethodParameters("CreateRestorePoint");

                inParams["Description"] = name;
                inParams["RestorePointType"] = 12;
                inParams["EventType"] = 100;

                var outParams = restoreClass.InvokeMethod("CreateRestorePoint", inParams, null);
                var result = Convert.ToInt32(outParams?["ReturnValue"] ?? 0);

                if (result != 0)
                {
                    throw new InvalidOperationException($"CreateRestorePoint failed with code {result}.");
                }

                SetStatus($"✔ Restore point \"{name}\" created at {DateTime.Now:HH:mm:ss}");
                LoadRestorePoints();
            }
            catch (Exception ex)
            {
                SetStatus($"✘ Error: {ex.Message}");
                MessageBox.Show($"Failed to create restore point:\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SetRegistryBypass(false);
            }
        }

        private void DeleteClick(object sender, RoutedEventArgs e)
        {
            if (RestorePointsList.SelectedIndex < 0 || _points.Count == 0)
            {
                SetStatus("Select a restore point to delete.");
                return;
            }

            var selected = _points[RestorePointsList.SelectedIndex];
            var result = MessageBox.Show(
                $"Delete restore point:\n\"{selected.Description}\"?\n\nThis cannot be undone.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            SetStatus($"Deleting \"{selected.Description}\"...");

            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ShadowCopy");
                ManagementObject? match = null;

                foreach (ManagementObject shadow in searcher.Get())
                {
                    using (shadow)
                    {
                        var dateStr = shadow["InstallDate"]?.ToString();
                        if (string.IsNullOrWhiteSpace(dateStr))
                        {
                            continue;
                        }

                        var shadowDate = ManagementDateTimeConverter.ToDateTime(dateStr);
                        if (Math.Abs((shadowDate - selected.CreationTime).TotalMinutes) < 5)
                        {
                            match = (ManagementObject)shadow.Clone();
                            break;
                        }
                    }
                }

                if (match is null)
                {
                    SetStatus("✘ Could not match restore point to a shadow copy.");
                    MessageBox.Show(
                        "Could not find a matching shadow copy. The restore point may be managed by Windows.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                using (match)
                {
                    var id = match["ID"]?.ToString();
                    if (string.IsNullOrWhiteSpace(id))
                    {
                        throw new InvalidOperationException("Matched shadow copy has no ID.");
                    }

                    RunCommand("vssadmin", $"delete shadows /shadow=\"{id}\" /quiet");
                }

                SetStatus($"✔ \"{selected.Description}\" deleted.");
                LoadRestorePoints();
            }
            catch (Exception ex)
            {
                SetStatus($"✘ Error: {ex.Message}");
                MessageBox.Show($"Deletion failed:\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteAllClick(object sender, RoutedEventArgs e)
        {
            if (_points.Count == 0)
            {
                SetStatus("No restore points to delete.");
                return;
            }

            var result = MessageBox.Show(
                $"Permanently delete ALL {_points.Count} restore point(s)?\n\nThis cannot be undone.",
                "Confirm Delete All",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            SetStatus("Deleting all restore points...");

            try
            {
                RunCommand("vssadmin", "delete shadows /for=C: /all /quiet");
                SetStatus("✔ All restore points deleted.");
                LoadRestorePoints();
            }
            catch (Exception ex)
            {
                SetStatus($"✘ Error: {ex.Message}");
                MessageBox.Show($"Deletion failed:\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshClick(object sender, RoutedEventArgs e)
        {
            LoadRestorePoints();
            SetStatus("List refreshed.");
        }

        private void SetStatus(string message)
        {
            StatusText.Text = message;
        }

        private static List<(string Description, DateTime CreationTime)> GetRestorePoints()
        {
            var list = new List<(string Description, DateTime CreationTime)>();

            try
            {
                var scope = new ManagementScope(@"\\.\root\default");
                scope.Connect();

                using var searcher = new ManagementObjectSearcher(scope, new ObjectQuery("SELECT * FROM SystemRestore"));
                foreach (ManagementObject obj in searcher.Get())
                {
                    using (obj)
                    {
                        var description = obj["Description"]?.ToString() ?? "Unnamed";
                        var dateStr = obj["CreationTime"]?.ToString();
                        var creationTime = dateStr is not null
                            ? ManagementDateTimeConverter.ToDateTime(dateStr)
                            : DateTime.MinValue;

                        list.Add((description, creationTime));
                    }
                }
            }
            catch
            {
                // Keep UI responsive; the caller will show an empty list state.
            }

            return list;
        }

        private static void SetRegistryBypass(bool enable)
        {
            try
            {
                using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                    @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\SystemRestore",
                    writable: true);

                if (enable)
                {
                    key?.SetValue("SystemRestorePointCreationFrequency", 0, Microsoft.Win32.RegistryValueKind.DWord);
                }
                else
                {
                    key?.DeleteValue("SystemRestorePointCreationFrequency", throwOnMissingValue: false);
                }
            }
            catch
            {
                // Ignore registry errors and let Windows decide whether creation is allowed.
            }
        }

        private static void RunCommand(string fileName, string arguments)
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo(fileName, arguments)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                }
            };

            process.Start();
            var standardOutput = process.StandardOutput.ReadToEnd();
            var standardError = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                var details = string.IsNullOrWhiteSpace(standardError) ? standardOutput : standardError;
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(details)
                    ? $"Command '{fileName} {arguments}' failed with exit code {process.ExitCode}."
                    : details.Trim());
            }
        }
    }
}
