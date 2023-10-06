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
            (sender as Editor).CursorPosition = e.NewTextValue.Length;
        }
    }
}