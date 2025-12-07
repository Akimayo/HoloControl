using HoloControl.ViewModels;

namespace HoloControl.Views.Kiosk
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            if (this.History.BindingContext is RootViewModel vm) vm.PropertyChanged += ScrollHistory;
        }

        private void ScrollHistory(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RootViewModel.History))
            {
                this.History.CursorPosition = this.History.Text.Length;
                this.History.Focus();
            }
        }

        private void Picker_Focused(object sender, FocusEventArgs e)
        {
            if (sender is Element elmt && elmt.BindingContext is RootViewModel context) context.Connection.ReloadPorts();
        }

        private void Picker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (sender is Picker picker && picker.BindingContext is RootViewModel context && picker.SelectedIndex != -1)
                context.Connect.Execute(null);
        }
    }
}