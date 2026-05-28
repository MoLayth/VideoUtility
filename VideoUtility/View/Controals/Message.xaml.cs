using System.Windows;
using System.Windows.Controls;

namespace VideoUtility.View.Controals {
    public partial class Message : UserControl {
        CancellationTokenSource cts = new CancellationTokenSource();
        public Message() {
            InitializeComponent();

            errorLabel.Visibility = Visibility.Hidden;

            this.Unloaded += Message_Unloaded;
        }

        private void Message_Unloaded(object sender, RoutedEventArgs e) {
            DisposeAndCancelCTS(ref cts);

            this.Unloaded -= Message_Unloaded;
        }

        public void ShowError(string message, float duration = 3) {
            System.Media.SystemSounds.Exclamation.Play();
            DisposeAndCancelCTS(ref cts);
            cts = new CancellationTokenSource();

            errorLabel.Content = message;
            _ = Animation(duration);
        }
        private async Task Animation(float duration) {
            errorLabel.Visibility = Visibility.Visible;

            try {
                await Task.Delay(TimeSpan.FromSeconds(duration), cts.Token);

                errorLabel.Visibility = Visibility.Hidden;
            }
            catch (OperationCanceledException) { return; }
        }

        private void DisposeAndCancelCTS(ref CancellationTokenSource cts) {
            if (cts != null) {
                try {
                    cts.Cancel();
                    cts.Dispose();
                }
                catch (Exception) { }
                finally {
                    cts.Dispose();
                    cts = null;
                }
            }
        }
    }
}
