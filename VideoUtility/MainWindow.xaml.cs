using System.Windows;
using System.Windows.Controls;
using VideoUtility.Classes;
using VideoUtility.View.Controals;
namespace VideoUtility {

    public partial class MainWindow : Window {        
        List<PresetData> presets = new List<PresetData>();
        public static MainWindow Instance { get; private set; }
        string selectedFilePath = "";

        List<FrameworkElement> nonCommandUI = new(); // this list contain all the element that need to be hidden when the user select (use command)
        List<FrameworkElement> commandUI = new();

        // i use this (one use) bool to check if i sett all element values to the preset 0
        // if that so then i can check for any change and show the save button
        bool isAllTreeValueSetup = false;
        public MainWindow() {
            InitializeComponent();
            Instance = this;

            // setting the lists
            foreach (UIElement element in editPresetGrid.Children) {
                if (element is FrameworkElement fe) {
                    if (fe.Tag != null && fe.Tag.ToString() == "UI_Command") {
                        commandUI.Add(fe);
                        continue;
                    }

                    int row = Grid.GetRow(fe);
                    if (row == 0 || row == 1 || row == 6) continue;

                    nonCommandUI.Add(fe);
                }
            }

            Functionality.InitializeFFmpeg();
            Load();

            CutsInput.MaxValue = 15;
            CutsInput.MinValue = 1;
            CRFInput.MaxValue = 51;
            CRFInput.MinValue = 0;

            Binding();
            OnPresetChange(0);


            // capture all events
            editPresetGrid.AddHandler(TextBox.TextChangedEvent,new RoutedEventHandler(EditPresetGrid_ValueChanged));
            editPresetGrid.AddHandler(ComboBox.SelectionChangedEvent, new RoutedEventHandler(EditPresetGrid_ValueChanged));
            editPresetGrid.AddHandler(CustomCheckbox.ValueChangedEvent, new RoutedEventHandler(EditPresetGrid_ValueChanged));
            editPresetGrid.AddHandler(IntInputControl.ValueChangedEvent, new RoutedEventHandler(EditPresetGrid_ValueChanged));
            

            renameButton.Click += (s, e) => {
                if(presets.Count == 0) return;
                string? res = StringInputWindow.ShowWindow("Renaming", presets[presetComboBox.SelectedIndex].Name);
                RenamingPreset(res);
            };
            deleteButton.Click += (s, e) => {
                MessageBoxResult result = MessageBox.Show("Are You Sure About That!", "warning message", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                switch (result) {
                    case MessageBoxResult.Yes:
                        int selectedIndex = presetComboBox.SelectedIndex;
                        presets.RemoveAt(selectedIndex);
                        UpdatePresets();
                        presetComboBox.SelectedIndex = Math.Clamp(selectedIndex - 1, 0, presets.Count - 1);
                        break;
                    default:
                        break;
                }
            };

            saveButton.Click += (s, e) => SaveChangedPreset();
            selectVideoButton.Click += (s, e) => { 
                string path = Functionality.SelectVideo();
                if(string.IsNullOrEmpty(path)) return;

                selectedFilePath = path;
                videoPathLabel.Content = selectedFilePath; 
            };
            presetComboBox.SelectionChanged += (s, e) => {
                if(presetComboBox.SelectedIndex < 0) return;

                OnPresetChange(presetComboBox.SelectedIndex);
            };
            encoderComboBox.SelectionChanged += (s,e) => {
                if (encoderComboBox.SelectedIndex == 4) {
                    gpuEncoderComboBox.Visibility = Visibility.Visible;
                }
                else {
                    gpuEncoderComboBox.Visibility = Visibility.Hidden;
                }
            };

            addButton.Click += AddButton_Click;
            applyButton.Click += (s, e) => {
                if(string.IsNullOrEmpty(selectedFilePath)) {
                    ShowMessageInApplyPresetTap("Please select a video first.");
                    return;
                }

                _ = Functionality.ApplayPreset(selectedFilePath, presets[applyPresetComboBox.SelectedIndex]);
            };

            // handling showing and hiding of the ui element when use command box checked
            commandCheckbox.OnValueChange += (v) => {
                commandCheckbox_HandleUIVisibility(v);
            };

            this.Closing += (s, e) => {
                SaverAndLoader.SavePreset(presets);
                ContextMenuManager.SyncPresetsToRegistry(presets.Select(p => p.Name).ToArray());
            };

            // when all tree element are build then update the ui
            this.Loaded += (s, e) => {
                commandCheckbox_HandleUIVisibility(presets[0].useCommand);
            };
        }

        private void commandCheckbox_HandleUIVisibility(bool v) {
            if (v) {
                foreach (var element in nonCommandUI) {
                    element.Visibility = Visibility.Hidden;
                }
                foreach (var element in commandUI) {
                    element.Visibility = Visibility.Visible;
                }
            }
            else {
                foreach (var element in nonCommandUI) {
                    element.Visibility = Visibility.Visible;
                }
                foreach (var element in commandUI) {
                    element.Visibility = Visibility.Hidden;
                }
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e) {
            string? res = StringInputWindow.ShowWindow("Adding");
            AddPreset(res);
        }

        // this function will be called when the user clicks the ok or cancel button in the preset name input window,
        // if the user clicks ok, it will create a new preset with the name entered by the user and add it to the presets list
        private void RenamingPreset(string? presetName) {
            if (presetName == null) return;
            presets[presetComboBox.SelectedIndex].Name = presetName;
            int currentIndex = presetComboBox.SelectedIndex;
            UpdatePresets();
            presetComboBox.SelectedIndex = currentIndex;            
        }
        private void AddPreset(string? presetName) {
            if (presetName == null) return;

            presets.Add(new PresetData() {
                Name = presetName,
                Suffix = "",
                Cuts = 1,
                CRF = 23,
                Encoder = 0,
                ToFormat = 0,
                AudioState = 0,
                EncoderPreset = 2,
                useCommand = false,
                RawCommand = "",
            });
            UpdatePresets();
            presetComboBox.SelectedIndex = presets.Count - 1;
            OnPresetChange(presetComboBox.SelectedIndex);
        }

        // this function will save the changes made to the selected preset to the presets list
        private void SaveChangedPreset() {
            PresetData selectedPreset = presets[presetComboBox.SelectedIndex];
            selectedPreset.Suffix = suffixTextBox.Text;
            selectedPreset.Cuts = CutsInput.Value;
            selectedPreset.CRF = CRFInput.Value;
            selectedPreset.Encoder = encoderComboBox.SelectedIndex;
            selectedPreset.ToFormat = formatComboBox.SelectedIndex;
            selectedPreset.AudioState = audioComboBox.SelectedIndex;
            selectedPreset.EncoderPreset = encoderPresetComboBox.SelectedIndex;
            selectedPreset.gpuEncoder = gpuEncoderComboBox.SelectedIndex;
            selectedPreset.useCommand = commandCheckbox.Value;
            selectedPreset.RawCommand = commandTextBox.Text;
            saveButton.Visibility = Visibility.Hidden;
        }

        // check if any of the controls values have if that so then show the save button
        private void EditPresetGrid_ValueChanged(object sender, RoutedEventArgs e) {
            if (!isAllTreeValueSetup) return;

            DependencyObject sourceControl = e.OriginalSource as DependencyObject;

            // safety checks  
            if (sourceControl == null || saveButton == null || presetComboBox == null || gpuEncoderComboBox == null || gpuEncoderComboBox == null || commandCheckbox == null || commandTextBox == null || presets.Count == 0 ) return;

            // ignore changes made in the preset name input window, because those are not "editing" the preset, but rather creating a new one
            if (sourceControl == presetComboBox || encoderPresetComboBox == null) {
                return;
            }
            
            
            if(presetComboBox.SelectedIndex < 0) return;
            // check if the current values of the controls are different from the values of the selected preset,
            // if they are different, show the save button otherwise keep it hidden
            PresetData currentPreset = presets[presetComboBox.SelectedIndex];
            if(currentPreset.Suffix != suffixTextBox.Text || currentPreset.Cuts != CutsInput.Value ||
                currentPreset.Encoder != encoderComboBox.SelectedIndex || currentPreset.CRF != CRFInput.Value ||
                currentPreset.ToFormat != formatComboBox.SelectedIndex || currentPreset.AudioState != audioComboBox.SelectedIndex ||
                currentPreset.EncoderPreset != encoderPresetComboBox.SelectedIndex || currentPreset.gpuEncoder != gpuEncoderComboBox.SelectedIndex ||
                currentPreset.useCommand != commandCheckbox.Value || currentPreset.RawCommand != commandTextBox.Text) {

                saveButton.Visibility = Visibility.Visible;
            }
            else saveButton.Visibility = Visibility.Hidden;
        }
        
        // loading the save file if any is exist
        private void Load() {
            presets = SaverAndLoader.LoadData();
            // if this is the first time the user opens the app, there will be no presets, so we need to create a default preset
            if (presets.Count == 0) {
                PresetData defaultPreset = new PresetData();
                defaultPreset.Name = "Optimize Video";
                defaultPreset.Suffix = "_Optimized";
                defaultPreset.Cuts = 1;
                defaultPreset.CRF = 23;
                defaultPreset.Encoder = 4;
                defaultPreset.ToFormat = 0;
                defaultPreset.AudioState = 0;
                defaultPreset.EncoderPreset = 2;
                defaultPreset.RawCommand = "";

                presets.Add(defaultPreset);
            }
        }

        private void Binding() {
            encoderComboBox.ItemsSource = MediaSettings.encoders;
            formatComboBox.ItemsSource = MediaSettings.videoFormats;
            audioComboBox.ItemsSource = MediaSettings.audioActions;
            UpdatePresets();
            encoderPresetComboBox.ItemsSource = MediaSettings.encoderPresets;

            MediaSettings.InitializeSupportedGPUEncoders();
            gpuEncoderComboBox.ItemsSource = MediaSettings.supportedGPUEncoders;
        }
        // this function should be call when i change the presets list
        private void UpdatePresets() {
            presetComboBox.ItemsSource = new List<string>(presets.Select(p => p.Name));
            applyPresetComboBox.ItemsSource = new List<string>(presets.Select(p => p.Name));

            if(presets.Count > 1) {
                deleteButton.Visibility = Visibility.Visible;
            }else deleteButton.Visibility = Visibility.Hidden;
        }

        // this function will set the values of the controls to the values of the selected preset
        private void OnPresetChange(int presetIndex) {
            PresetData selectedPreset = presets[presetIndex];
            suffixTextBox.Text = selectedPreset.Suffix;
            CutsInput.Value = selectedPreset.Cuts;
            CRFInput.Value = selectedPreset.CRF;
            encoderComboBox.SelectedIndex = selectedPreset.Encoder;
            encoderPresetComboBox.SelectedIndex = selectedPreset.EncoderPreset;
            formatComboBox.SelectedIndex = selectedPreset.ToFormat;
            audioComboBox.SelectedIndex = selectedPreset.AudioState;
            gpuEncoderComboBox.SelectedIndex = selectedPreset.gpuEncoder;

            commandTextBox.Text = selectedPreset.RawCommand;
            commandCheckbox.Value = selectedPreset.useCommand;

            if(selectedPreset.Encoder == 4) {
                gpuEncoderComboBox.Visibility = Visibility.Visible;
            }else gpuEncoderComboBox.Visibility = Visibility.Hidden;

            isAllTreeValueSetup = true;
        }
        public void ShowMessageInApplyPresetTap(string message) {
            messageControl.ShowError(message, 5);
        }        

        // allow only numbers in the cuts input and with maximum 10
        private void OnlyInt(object Sender, int max, int min) {
            TextBox sender = (TextBox) Sender;

            if (sender == null || sender.Text.Length == 0) return;
            int caretIndex = sender.CaretIndex; // capture the caret index before modifying the text

            int value;
            if (!int.TryParse(sender.Text,out value)){
                if (sender.Text.Length == 1) {
                    sender.Text = "";
                    return;
                }
                sender.Text = sender.Text.Remove(sender.CaretIndex - 1, 1);
                caretIndex--;
            }
            else {
                sender.Text = Math.Clamp(value, min, max).ToString();
            }

            sender.CaretIndex = caretIndex; // restore the caret index after modifying the text
        }
    }
}