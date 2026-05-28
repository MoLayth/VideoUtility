using FFMpegCore;
using FFMpegCore.Enums;
namespace VideoUtility.Classes {
    public static class MediaSettings {
        public static readonly List<string> encoders = new List<string>() { "H264", "H265", "VP9", "AV1", "GPU Acceleration" };
        public static readonly List<string> allGPUEncoders = new List<string>() { 
            // NVIDIA
            "h264_nvenc", "hevc_nvenc" , "av1_nvenc" ,
            // AMD
            "h264_amf", "hevc_amf" , "av1_amf" ,
            // Intel
            "h264_qsv", "hevc_qsv" , "av1_qsv"
        };

        public static readonly List<string> videoFormats = new List<string>() { "DEFAULT", "MP4", "MKV", "MOV", "WEBM" };
        public static readonly List<string> audioActions = new List<string>() { "Keep", "Remove" };
        public static readonly List<string> encoderPresets = new List<string>() { "Superfast" , "Veryfast", "Fast", "Medium", "Slow", "Slower" , "VerySlow" };

        public static List<string> supportedGPUEncoders = new List<string>();
        public static void InitializeSupportedGPUEncoders() {
            foreach (string encoder in allGPUEncoders) {
                if (Functionality.IsEncoderSupported(encoder)) {
                    supportedGPUEncoders.Add(encoder);
                }
            }
            if (supportedGPUEncoders.Count == 0) supportedGPUEncoders.Add("No GPU Encoders Supported");
        }

        public static string HardwareEncoderToFFmpegStringIdentifier(string encoder) {
            switch (encoder) {
                case "H264": return "libx264";
                case "H265": return "libx265";
                case "VP9": return "libvpx-vp9";
                case "AV1": return "libaom-av1";
                default: return encoder;
            }
        }
        public static FFMpegCore.Enums.Speed SpeedPresetToFFmpegSpeed(string speedPreset) {
            switch (speedPreset) {
                case "Superfast": return FFMpegCore.Enums.Speed.SuperFast;
                case "Veryfast": return FFMpegCore.Enums.Speed.VeryFast;
                case "Fast": return FFMpegCore.Enums.Speed.Fast;
                case "Medium": return FFMpegCore.Enums.Speed.Medium;
                case "Slow": return FFMpegCore.Enums.Speed.Slow;
                case "Slower": return FFMpegCore.Enums.Speed.Slower;
                case "VerySlow": return FFMpegCore.Enums.Speed.VerySlow;
            }
            return FFMpegCore.Enums.Speed.Medium;
        }

        public static string VideoFormatToFFmpegStringIdentifier(string format, string inputFilePath) {
            switch (format) {
                case "MP4": return "mp4";
                case "MKV": return "mkv";
                case "MOV": return "mov";
                case "WEBM": return "webm";

                // in the cas of "DEFAULT"
                default: return System.IO.Path.GetExtension(inputFilePath).TrimStart('.').ToLower();
            }
        }

        public static string GetGPUSpeedPreset(PresetData preset) {
            string videoCodec = supportedGPUEncoders[preset.gpuEncoder];
            string gpuSpeedPreset = "";

            // NVIDIA
            if (videoCodec.Contains("nvenc")) {
                gpuSpeedPreset = $"p{preset.EncoderPreset + 1}";
            }
            // Intel Quick Sync (QSV)
            else if (videoCodec.Contains("qsv")) {
                // You will need to map your UI index (e.g., 0-6) to Intel's string names
                string[] intelPresets = { "veryfast", "faster", "fast", "medium", "slow", "slower", "veryslow" };

                // Ensure the index doesn't go out of bounds
                int index = Math.Min(preset.EncoderPreset, intelPresets.Length - 1);
                gpuSpeedPreset = intelPresets[index];
            }
            // AMD
            else if (videoCodec.Contains("amf")) {
                string[] amdPresets = { "speed", "balanced", "quality" };
                int index = 0;

                if (preset.EncoderPreset > 4) index = 2;
                else if (preset.EncoderPreset > 1) index = 1;
                else index = 0;

                gpuSpeedPreset = amdPresets[index];
            }

            return gpuSpeedPreset;
        }
    }
}
