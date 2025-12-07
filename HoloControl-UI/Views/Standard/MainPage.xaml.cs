using HoloControl.ViewModels;

namespace HoloControl.Views.Standard
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }
        private void Editor_TextChanged(object sender, TextChangedEventArgs e)
        {
            Task.Delay(1000);
            (sender as Editor).CursorPosition = e.NewTextValue.Length;
        }
        private void PortPicker_Focused(object sender, FocusEventArgs e)
        {
            if (this.BindingContext is RootViewModel context) context.Connection.ReloadPorts();
        }
    }
}