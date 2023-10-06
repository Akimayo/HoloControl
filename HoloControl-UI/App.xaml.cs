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
    }
}