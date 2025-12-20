using HoloControl.ViewModels;

namespace HoloControl.Views.Kiosk
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            if (this.History.BindingContext is RootViewModel vm) vm.PropertyChanged += ScrollHistory;
            this.FocusableElements = [ewt, ert, egt, ebt, eet, eft];
        }

        private readonly IList<VisualElement> FocusableElements;
        private async void ScrollHistory(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RootViewModel.History) && this.History.IsVisible)
            {
                // Scroll the History Editor to end by setting the cursor position and focusing that element.
                // This causes focus to be removed from Entries mid-edit, so if an entry was focused, remember
                // the focused one and move focus back to it right after.
                var prevFocusedElement = this.FocusableElements.FirstOrDefault(e => e.IsFocused);
                this.History.CursorPosition = this.History.Text.Length;
                this.History.Focus();
                if (prevFocusedElement != null)
                {
                    await Task.Delay(50);
                    prevFocusedElement.Focus();
                }
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