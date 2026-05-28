using System.Windows;
using System.Windows.Controls;

namespace VideoUtility.View.Controals {
    public partial class StringInputWindow : UserControl {
        public StringInputWindow() {
            InitializeComponent();
        }

        public static string? ShowWindow(string windowTitle, string placeHolder = "") {
            StringInputWindow presetName = new StringInputWindow();

            var modalWindow = new Window {
                Title = windowTitle,
                Content = presetName,
                Owner = Application.Current.MainWindow,
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.ToolWindow,
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
            };

            presetName.presetNameTextBox.Text = placeHolder;

            string? result = null;

            presetName.okButton.Click += (s, e) => {
                if (string.IsNullOrWhiteSpace(presetName.presetNameTextBox.Text)) {
                    System.Media.SystemSounds.Exclamation.Play();
                }
                else {
                    modalWindow.Close();
                    result = presetName.presetNameTextBox.Text;
                }
            };
            presetName.cancelButton.Click += (s, e) => {
                modalWindow.Close();
                result = null;
            };

            presetName.presetNameTextBox.KeyDown += (s, e) => {
                if (e.Key == System.Windows.Input.Key.Enter) {
                    if (string.IsNullOrWhiteSpace(presetName.presetNameTextBox.Text)) {
                        System.Media.SystemSounds.Exclamation.Play();
                    }
                    else {
                        modalWindow.Close();
                        result = presetName.presetNameTextBox.Text;
                    }
                }
            };

            modalWindow.ShowDialog();

            return result;
        }
    }
}
