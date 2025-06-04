using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SilkySouls.Utilities
{
    public static class VersionChecker
    {
        public static async Task<(bool hasUpdate, Version currentVersion, Version webVersion)> CheckForUpdate()
        {
            try
            {
                var currentVersion = Assembly.GetEntryAssembly()?.GetName().Version;
                if (currentVersion == null) return (false, null, null);

                var client = new HttpClient();
                client.DefaultRequestHeaders.UserAgent.Add(
                    new ProductInfoHeaderValue("SilkySouls", currentVersion.ToString()));

                var response = await client.GetStringAsync(
                    "https://api.github.com/repos/borgCode/SilkySouls/releases/latest");

                int tagIndex = response.IndexOf("\"tag_name\":", StringComparison.OrdinalIgnoreCase);
                if (tagIndex == -1) return (false, currentVersion, null);

                int quoteStart = response.IndexOf('"', tagIndex + "\"tag_name\":".Length) + 1;
                int quoteEnd = response.IndexOf('"', quoteStart);

                if (quoteStart == -1 || quoteEnd == -1) return (false, currentVersion, null);

                var webVersion = new Version(response.Substring(quoteStart, quoteEnd - quoteStart).TrimStart('v'));

                return (webVersion > currentVersion, currentVersion, webVersion);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Update check failed: {ex.Message}");
                return (false, null, null);
            }
        }

        public static async void CheckForUpdates(Window parentWindow, bool showNoUpdateMessage = false)
        {
            var (hasUpdate, currentVersion, webVersion) = await CheckForUpdate();

            if (!hasUpdate || webVersion == null || currentVersion == null)
            {
                if (showNoUpdateMessage)
                {
                    MessageBox.Show(
                        "Your application is up to date.",
                        "Update Check",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }

                return;
            }


            var updateWindow = new Window
            {
                Title = "Update Available",
                Width = 300,
                Height = 200,
                WindowStyle = WindowStyle.None,
                ResizeMode = ResizeMode.NoResize,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = parentWindow,
                Background = (SolidColorBrush)Application.Current.Resources["BackgroundBrush"],
                BorderBrush = (SolidColorBrush)Application.Current.Resources["BorderBrush"],
                BorderThickness = new Thickness(1)
            };

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(30) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });

            var titleBar = new Border
            {
                Background = (SolidColorBrush)Application.Current.Resources["TitleBarBrush"],
                Child = new TextBlock
                {
                    Text = "Update Available",
                    Foreground = (SolidColorBrush)Application.Current.Resources["TextBrush"],
                    Margin = new Thickness(10, 5, 0, 0)
                }
            };
            Grid.SetRow(titleBar, 0);
            grid.Children.Add(titleBar);

            var message = new TextBlock
            {
                Text =
                    $"A new version (v{webVersion.Major}.{webVersion.Minor}.{webVersion.Build}) is available!\nCurrent version: v{currentVersion.Major}.{currentVersion.Minor}.{currentVersion.Build}",
                Foreground = (SolidColorBrush)Application.Current.Resources["TextBrush"],
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(20, 10, 20, 10),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(message, 1);
            grid.Children.Add(message);
            
            var dontShowCheckbox = new CheckBox
            {
                Content = "Don't show on app launch",
                Foreground = (SolidColorBrush)Application.Current.Resources["TextBrush"],
                Margin = new Thickness(20, 5, 20, 0),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
                IsChecked = !SettingsManager.Default.EnableUpdateChecks
            };
            Grid.SetRow(dontShowCheckbox, 1);
            grid.Children.Add(dontShowCheckbox);

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 10, 0, 0)
            };

            var downloadButton = new Button
            {
                Content = "Download",
                Width = 80,
                Height = 25,
                Margin = new Thickness(5)
            };
            downloadButton.Click += (s, e) =>
            {
                SettingsManager.Default.EnableUpdateChecks = dontShowCheckbox.IsChecked != true;
                SettingsManager.Default.Save();
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "https://github.com/borgCode/SilkySouls/releases/latest",
                    UseShellExecute = true
                });
                updateWindow.Close();
            };

            var laterButton = new Button
            {
                Content = "Later",
                Width = 80,
                Height = 25,
                Margin = new Thickness(5)
            };
            laterButton.Click += (s, e) => 
            {
                SettingsManager.Default.EnableUpdateChecks = dontShowCheckbox.IsChecked != true;
                SettingsManager.Default.Save();
    
                updateWindow.Close();
            };

            buttonPanel.Children.Add(downloadButton);
            buttonPanel.Children.Add(laterButton);
            grid.Children.Add(buttonPanel);
            Grid.SetRow(buttonPanel, 2);

            updateWindow.Content = grid;

            titleBar.MouseLeftButtonDown += (s, e) => updateWindow.DragMove();

            updateWindow.ShowDialog();
        }
        
        public static void UpdateVersionText(TextBlock appVersion)
        {
            var currentVersion = Assembly.GetEntryAssembly()?.GetName().Version;
            if (currentVersion != null)
            {
                appVersion.Text = $"v{currentVersion.Major}.{currentVersion.Minor}.{currentVersion.Build}";
            }
        }
    }
}