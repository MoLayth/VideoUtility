using System.Diagnostics;
using Microsoft.Win32;

namespace VideoUtility.Classes {
    public static class ContextMenuManager {
        private static readonly string[] SupportedExtensions = { ".mp4", ".mkv", ".avi", ".mov" };

        public static void SyncPresetsToRegistry(string[] currentPresetNames) {
            string appPath = Process.GetCurrentProcess().MainModule.FileName;

            // Loop through every file extension and build the menu for each one
            foreach (string ext in SupportedExtensions) {
                string baseExtensionPath = $@"Software\Classes\SystemFileAssociations\{ext}\shell";

                // 1. Wipe the old menu completely for this extension
                using (RegistryKey classesKey = Registry.CurrentUser.OpenSubKey(baseExtensionPath, true)) {
                    if (classesKey != null) {
                        classesKey.DeleteSubKeyTree("VideoUtility", false);
                    }
                }

                // If no presets, we just leave it deleted and move to the next extension
                if (currentPresetNames == null || currentPresetNames.Length == 0) continue;

                // 2. Create the Main "Video Utility" right-click folder for this extension
                using (RegistryKey mainKey = Registry.CurrentUser.CreateSubKey($@"{baseExtensionPath}\VideoUtility")) {
                    mainKey.SetValue("MUIVerb", "Video Utility");
                    mainKey.SetValue("Icon", $"\"{appPath}\",0");

                    // The magic line that tells Windows this is a cascading drop-down menu
                    mainKey.SetValue("SubCommands", "");

                    // 3. Create the nested 'shell' folder
                    using (RegistryKey shellKey = mainKey.CreateSubKey("shell")) {
                        foreach (string preset in currentPresetNames) {
                            string safeKeyName = preset.Replace(@"\", "").Replace("/", "");

                            using (RegistryKey presetKey = shellKey.CreateSubKey(safeKeyName)) {
                                presetKey.SetValue("MUIVerb", preset);

                                using (RegistryKey commandKey = presetKey.CreateSubKey("command")) {
                                    string commandString = $"\"{appPath}\" \"%1\" \"{preset}\"";
                                    commandKey.SetValue("", commandString);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
