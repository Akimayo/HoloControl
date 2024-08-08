using HoloControl.ViewModels;

namespace HoloControl.Views.Standard
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            if (this.BindingContext is RootViewModel context) context.HistoryList.CollectionChanged += HistoryList_CollectionChanged;
        }

        private void HistoryList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Task.Delay(1000);
            this.History.ScrollTo((sender as ICollection<object>).Count - 1, position: ScrollToPosition.End);
        }
        private void Editor_TextChanged(object sender, TextChangedEventArgs e)
        {
            Task.Delay(1000);
            (sender as Editor).CursorPosition = e.NewTextValue.Length;
        }
    }
}