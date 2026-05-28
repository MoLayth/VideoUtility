using System.Windows;
using System.Windows.Interop;

namespace VideoUtility.View.Dialog {
    // i must set the size of this window ProgressBar to be fixed so the user cant change the width
    public partial class ProgressBar : Window {

        // this bool must be set to true so it can closed
        // i do that so the user he cant close the window and only closed when i tell it to be.
        public bool FinishEncoding;
        public ProgressBar() {
            InitializeComponent();
            FinishEncoding = false;

            Closing += ProgressBar_Closing;
        }

        private void ProgressBar_Closing(object? sender, System.ComponentModel.CancelEventArgs e) {
            if(!FinishEncoding) {
                e.Cancel = true;
                System.Media.SystemSounds.Asterisk.Play();
            }
            else {
                FinishEncoding = false;
            }
        }

        // the p must be between 0 and 100 to ProgressBar work correctly
        public void SetProgress(double p) {
            p = Math.Clamp(p, 0, 100);
            bar.Width = Map(p, 0, 100, 0, this.ActualWidth);
        }

        private double Map(double value, double fromSource, double toSource, double fromTarget, double toTarget) {
            return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
        }
    }
}
