using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
namespace VideoUtility.Classes {
    public static class SaverAndLoader {
        public static void SavePreset(List<PresetData> data) {
            string path = GetPresetPath();

            string directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory)) {
                Directory.CreateDirectory(directory);
            }

            var options = new System.Text.Json.JsonSerializerOptions {
                IncludeFields = true,
            };
            string json = System.Text.Json.JsonSerializer.Serialize(data, options);
            File.WriteAllText(path, json);
        }
        public static List<PresetData> LoadData() {
            if (!File.Exists(GetPresetPath())) {
                return new List<PresetData>();
            }

            string jasonFile = File.ReadAllText(GetPresetPath());

            var options = new System.Text.Json.JsonSerializerOptions {
                IncludeFields = true,
            };
            return System.Text.Json.JsonSerializer.Deserialize<List<PresetData>>(jasonFile, options);
        }

        private static string GetPresetPath() {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\VideoUtility\\" + "Data.preset";
        }
    }
}
