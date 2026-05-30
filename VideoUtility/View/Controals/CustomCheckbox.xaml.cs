using System.Windows;
using System.Windows.Controls;
namespace VideoUtility.View.Controals {
    public partial class CustomCheckbox : UserControl {
        public event Action<bool> OnValueChange;
        public bool Value {
            get { return valueRectangel.Visibility == Visibility.Visible; }
            set {
                if (value) valueRectangel.Visibility = Visibility.Visible;
                else valueRectangel.Visibility = Visibility.Hidden;

                OnValueChange?.Invoke(value);
                RaiseEvent(new RoutedEventArgs(ValueChangedEvent, this));
            }
        }

        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged",
            RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CustomCheckbox));
        public event RoutedEventHandler ValueChanged {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }

        public CustomCheckbox() {
            InitializeComponent();
            body.MouseDown += Body_MouseDown;
        }

        private void Body_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            Value = !Value;            
        }
    }
}
