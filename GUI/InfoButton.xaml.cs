using System.Windows;
using System.Windows.Controls;

namespace GUI
{
    /// <summary>
    /// Interaction logic for InfoButton.xaml
    /// </summary>
    public partial class InfoButton : UserControl
    {
        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(InfoButton));

        public event RoutedEventHandler Click
        {
            add { AddHandler(ClickEvent, value); }
            remove { RemoveHandler(ClickEvent, value); }
        }

        void ClickButton(object sender, RoutedEventArgs e) => RaiseEvent(new RoutedEventArgs(ClickEvent));

        public InfoButton()
        {
            InitializeComponent();

        }
    }
}
