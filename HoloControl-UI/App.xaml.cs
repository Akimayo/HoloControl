namespace HoloControl
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            string[] args = Environment.GetCommandLineArgs();

            MainPage = args.Contains("kiosk") ? new KioskShell() : new StandardShell();
        }
        public static void HandleAppActions(AppAction appAction)
        {
            App.Current.Dispatcher.Dispatch(() =>
            {
                Page page = appAction.Id switch
                {
                    // "mode_standard" => new Views.Standard.MainPage(),
                    "mode_kiosk" => new Views.Kiosk.MainPage(),
                    _ => default(Page)
                };

                if (page != null) Application.Current.Windows[0].Page = page;
            });
        }
    }
}