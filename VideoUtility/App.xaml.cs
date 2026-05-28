using System.Windows;
using VideoUtility.Classes;

namespace VideoUtility {
    public partial class App : Application {
        protected override async void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);

            // if the app was launch using right click menu
            if (e.Args.Length > 0) {
                string videoPath = e.Args[0];

                string? presetName = e.Args.Length > 1 ? e.Args[1] : null;

                if (!string.IsNullOrEmpty(presetName)) {
                    // i need to run the function ApplyPreset
                    List<PresetData> presets = SaverAndLoader.LoadData();
                    PresetData selectedPreset = new();

                    foreach (var preset in presets) {
                        if (preset.Name == presetName) selectedPreset = preset;
                    }

                    Functionality.InitializeFFmpeg();
                    MediaSettings.InitializeSupportedGPUEncoders();

                    try {
                        await Functionality.ApplayPreset(videoPath, selectedPreset);
                    }
                    catch (Exception ex) {
                        MessageBox.Show($"A critical error occurred before encoding started:\n{ex.Message}", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    finally {
                        this.Shutdown();
                    }
                }
                else {
                    // i have only the video path no preset selected just set the path to the video path and show the main window
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                }
            }
            else {
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
            }
        }
    }
}
