namespace HoloControl
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }
        protected override Window CreateWindow(IActivationState activationState)
        {
            string[] args = Environment.GetCommandLineArgs();
            return new Window(args.Contains("kiosk") ? new KioskShell() : new StandardShell());
        }
        public static void HandleAppActions(AppAction appAction)
        {
            Page page = appAction.Id switch
            {
                "mode_standard" => new StandardShell(),
                "mode_kiosk" => new KioskShell(),
                _ => default
            };

            if (page != null) Current.Windows[0].Page = page;
        }
    }
}