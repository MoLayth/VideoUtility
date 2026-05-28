namespace VideoUtility.Classes {
    using FFMpegCore;
    using Microsoft.Win32;
    using System.IO;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Media;
    using VideoUtility.View.Dialog;

    public static class Functionality {
        public static void InitializeFFmpeg() {
            string ffmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Dependencies");
            GlobalFFOptions.Configure(options => options.BinaryFolder = ffmpegPath);
        }

        public static string SelectVideo() {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Video Files|*.mp4;*.mkv;*.avi;*.mov;*.webm|All Files|*.*";
            if (openFileDialog.ShowDialog() == true) {
                return openFileDialog.FileName;
            }

            return "";
        }

        public static async Task ApplayPreset(string inputFile, PresetData preset) {
            string directory = Path.GetDirectoryName(inputFile);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(inputFile);
            string videoFormat = MediaSettings.VideoFormatToFFmpegStringIdentifier(MediaSettings.videoFormats[preset.ToFormat], inputFile);
            IMediaAnalysis inputVideoInfo = FFProbe.Analyse(inputFile);


            string audioArgument = preset.AudioState == 1 ? "-an" : "";
            double cutDuration = inputVideoInfo.Duration.TotalSeconds / (double)preset.Cuts;

            // create directory
            if (preset.Cuts > 1) {
                string cutDirectory = Path.Join(Path.GetDirectoryName(inputFile), fileNameWithoutExtension + "_Cuts");
                Directory.CreateDirectory(cutDirectory);
                directory = cutDirectory;
            }

            ProgressBar progressBar = new ProgressBar();
            progressBar.Show();

            // use this var for collecting correctly the progress
            double totalProgressForAllCuts = 0;
            double previousProgress = 0;

            for (int i = 0; i < preset.Cuts; i++) {
                string cut = preset.Cuts == 1 ? "" : $"_cut_{i + 1}";
                string outputPath = Path.Join(directory, fileNameWithoutExtension + preset.Suffix + cut + $".{videoFormat}");
                TimeSpan currentCutDuration = TimeSpan.FromSeconds(cutDuration);
                previousProgress = 0; // reset for every cut

                if (MediaSettings.encoders[preset.Encoder] == "GPU Acceleration") {
                    progressBar.bar.Fill = Brushes.LightBlue;
                    string videoCodec = MediaSettings.supportedGPUEncoders[preset.gpuEncoder];

                    string gpuSpeedPreset = MediaSettings.GetGPUSpeedPreset(preset);

                    string capturedError = "";
                    try {
                        await FFMpegArguments.FromFileInput(inputFile, true, options => {
                            options.Seek(TimeSpan.FromSeconds(i * cutDuration)).WithDuration(TimeSpan.FromSeconds(cutDuration)).WithCustomArgument(audioArgument);
                        })
                            .OutputToFile(outputPath, true, options => {
                                options.WithVideoCodec(videoCodec)
                                .WithCustomArgument($"-cq {preset.CRF}")
                                .WithCustomArgument($"-preset {gpuSpeedPreset}");
                            }).NotifyOnError(errorMessage => { capturedError = errorMessage; }).NotifyOnProgress(progress => {
                            Application.Current.Dispatcher.Invoke(() => {
                                double delta = progress - previousProgress;
                                totalProgressForAllCuts += delta / preset.Cuts;
                                progressBar.SetProgress(totalProgressForAllCuts);
                                previousProgress = progress;
                            });
                            }, totalTimeSpan: currentCutDuration).ProcessAsynchronously();
                    }
                    catch (Exception ex) {
                        string finalErrorMessage = !string.IsNullOrEmpty(capturedError) ? capturedError : ex.Message;

                        // then i run this method via right click context menu
                        if(MainWindow.Instance == null) MessageBox.Show(finalErrorMessage,"Error",MessageBoxButton.OK,MessageBoxImage.Error);
                        else MainWindow.Instance.ShowMessageInApplyPresetTap(finalErrorMessage);

                        if (File.Exists(outputPath)) File.Delete(outputPath);

                        progressBar.FinishEncoding = true;
                        progressBar.Close();

                        return;
                    }
                    finally { 
                        if(i == preset.Cuts - 1) {
                            progressBar.FinishEncoding = true; 
                            progressBar.Close();
                        }
                    }
                }
                else {
                    progressBar.bar.Fill = Brushes.Red;
                    string capturedError = "";
                    try {
                        string videoCodec = MediaSettings.HardwareEncoderToFFmpegStringIdentifier(MediaSettings.encoders[preset.Encoder]);
                        await FFMpegArguments.FromFileInput(inputFile, true, options => {
                            options.Seek(TimeSpan.FromSeconds(i * cutDuration)).WithDuration(TimeSpan.FromSeconds(cutDuration)).WithCustomArgument(audioArgument);
                        })
                            .OutputToFile(outputPath, true, options => {
                                options.WithVideoCodec(videoCodec).WithConstantRateFactor(preset.CRF)
                                .WithSpeedPreset(MediaSettings.SpeedPresetToFFmpegSpeed(MediaSettings.encoderPresets[preset.EncoderPreset]));
                            }).NotifyOnError(errorMessage => { capturedError = errorMessage; }).NotifyOnProgress(progress => {
                                Application.Current.Dispatcher.Invoke(() => {
                                    double delta = progress - previousProgress;
                                    totalProgressForAllCuts += delta / preset.Cuts;
                                    progressBar.SetProgress(totalProgressForAllCuts);
                                    previousProgress = progress;
                                });
                            }, totalTimeSpan: currentCutDuration).ProcessAsynchronously();
                    }
                    catch (Exception ex) {
                        string finalErrorMessage = !string.IsNullOrEmpty(capturedError) ? capturedError : ex.Message;

                        if (MainWindow.Instance == null) MessageBox.Show(finalErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        else MainWindow.Instance.ShowMessageInApplyPresetTap(finalErrorMessage);

                        if (File.Exists(outputPath)) File.Delete(outputPath);

                        progressBar.FinishEncoding = true;
                        progressBar.Close();

                        return;
                    }
                    finally {
                        if (i == preset.Cuts - 1) {
                            progressBar.FinishEncoding = true;
                            progressBar.Close();
                        }
                    }
                }

            }
        }

        private static void oldMethod(string inputFile, PresetData preset) {
            string directory = Path.GetDirectoryName(inputFile);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(inputFile);
            string videoFormat = MediaSettings.VideoFormatToFFmpegStringIdentifier(MediaSettings.videoFormats[preset.ToFormat], inputFile);

            string outputPath = Path.Join(directory, fileNameWithoutExtension + preset.Suffix + $".{videoFormat}");

            #region Handel Video
            if (MediaSettings.encoders[preset.Encoder] == "GPU Acceleration") {
                string videoCodec = MediaSettings.supportedGPUEncoders[preset.gpuEncoder];

                string gpuSpeedPreset = MediaSettings.GetGPUSpeedPreset(preset);
                //string audioArgument = preset.AudioState == 2 ? "-an" : ""; // this use onlay inside the encode video

                string capturedError = "";
                try {
                    FFMpegArguments.FromFileInput(inputFile)
                        .OutputToFile(outputPath, true, options => {
                            options.WithVideoCodec(videoCodec)
                            .WithCustomArgument($"-cq {preset.CRF}")
                            .WithCustomArgument($"-preset {gpuSpeedPreset}");
                        }).NotifyOnError(errorMessage => { capturedError = errorMessage; }).ProcessSynchronously();
                }
                catch (Exception ex) {
                    string finalErrorMessage = !string.IsNullOrEmpty(capturedError) ? capturedError : ex.Message;
                    MainWindow.Instance.ShowMessageInApplyPresetTap(finalErrorMessage);
                }
            }
            else {
                string capturedError = "";
                try {
                    string videoCodec = MediaSettings.HardwareEncoderToFFmpegStringIdentifier(MediaSettings.encoders[preset.Encoder]);
                    FFMpegArguments.FromFileInput(inputFile)
                        .OutputToFile(outputPath, true, options => {
                            options.WithVideoCodec(videoCodec).WithConstantRateFactor(preset.CRF)
                            .WithSpeedPreset(MediaSettings.SpeedPresetToFFmpegSpeed(MediaSettings.encoderPresets[preset.EncoderPreset]));
                        }).NotifyOnError(errorMessage => { capturedError = errorMessage; }).ProcessSynchronously();
                }
                catch (Exception ex) {
                    string finalErrorMessage = !string.IsNullOrEmpty(capturedError) ? capturedError : ex.Message;
                    MainWindow.Instance.ShowMessageInApplyPresetTap(finalErrorMessage);
                }
            }
            #endregion

            #region handel audio

            if (preset.AudioState != 0) {
                switch (preset.AudioState) {
                    // extract audio to separate file
                    case 1:
                        string audioOutputPath = Path.Join(directory, fileNameWithoutExtension + preset.Suffix + ".mp3");

                        FFMpeg.ExtractAudio(outputPath, audioOutputPath);
                        break;

                    // remove audio from video
                    case 2:
                        string tempAudioPath = Path.Join(directory, fileNameWithoutExtension + "_temp_without_audio" + $".{videoFormat}");
                        FFMpeg.Mute(outputPath, tempAudioPath);

                        File.Delete(outputPath);
                        File.Move(tempAudioPath, outputPath);
                        break;
                }
            }
            #endregion

            #region Handle Cuts
            if (preset.Cuts > 1) {
                IMediaAnalysis inputVideoInfo = FFProbe.Analyse(inputFile);
                string cutDirectory = Path.Join(Path.GetDirectoryName(outputPath), fileNameWithoutExtension + "_Cuts");
                Directory.CreateDirectory(cutDirectory);


                double cutDuration = inputVideoInfo.Duration.TotalSeconds / (double)preset.Cuts;
                for (int i = 0; i < preset.Cuts; i++) {
                    string cutOutputPath = Path.Join(cutDirectory, $"{fileNameWithoutExtension}_cut_{i + 1}.{videoFormat}");
                    FFMpegArguments.FromFileInput(outputPath, true, options => {
                        options.Seek(TimeSpan.FromSeconds(i * cutDuration)).WithDuration(TimeSpan.FromSeconds(cutDuration));
                    }
                    ).OutputToFile(cutOutputPath, true, options => {
                        options.WithVideoCodec("copy").WithAudioCodec("copy");
                    }).ProcessSynchronously();
                }
                File.Delete(outputPath);
            }
            #endregion
        }

        public static bool IsEncoderSupported(string encoder) {
            try {
                // $"-v error -f lavfi -i color=c=black:s=128x128 -vframes 1 -c:v {encoderName} -f null -"
                // here we just generate a 1 frame video with the specified encoder and check if it processes successfully
                bool isSuccessful = FFMpegArguments.FromFileInput("color=c=black:s=128x128", verifyExists: false, options => {
                    options.WithCustomArgument("-v error -f lavfi");
                }).OutputToFile("-", overwrite: false, options => {
                    options.WithCustomArgument($"-vframes 1 -pix_fmt nv12 -c:v {encoder} -f null");
                }).ProcessSynchronously();

                return isSuccessful;
                        
            } catch (Exception ex) {
                return false;
            }
        }
    }
}
