using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VideoUtility.View.Controals {
    public partial class IntInputControl : UserControl {
        public int MaxValue;
        public int MinValue;

        public int Value {
            get {
                 return int.Parse(valueLabel.Content.ToString());
            }
            set {
                int oldValue = Value;
                int newValue = Math.Clamp(value, MinValue, MaxValue);

                if (oldValue == newValue) return; // No change, so don't raise the event

                valueLabel.Content = newValue.ToString();
                OnValueChanged?.Invoke(newValue);
                RaiseEvent(new RoutedEventArgs(ValueChangedEvent, this));
            }
        }

        public event Action<int> OnValueChanged;

        // Register the Routed Event to bubble up the tree this well becoum useful later
        // when i want to get nofified when the Value changes in the preset editor of any controal
        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged",
            RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(IntInputControl));
        public event RoutedEventHandler ValueChanged {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }

        public IntInputControl() {
            InitializeComponent();

            increaseButton.Click += (s, e) => ChangeValue(1);
            decreaseButton.Click += (s, e) => ChangeValue(-1);

            valueLabel.MouseWheel += ValueLabel_MouseWheel;
        }

        private void ValueLabel_MouseWheel(object sender, MouseWheelEventArgs e) {
            int step = e.Delta > 0 ? 1 : -1;
            ChangeValue(step);
        }

        private void ChangeValue(int delta) {
            Value += delta;
        }
    }
}
