using CommunityToolkit.Mvvm.Input;
using HoloControl.Models;
using HoloControl.Models.Form;
using System.Windows.Input;

namespace HoloControl.ViewModels
{
    internal class KioskViewModel : RootViewModel
    {
        public bool IsManualMode { get; private set; } = true;
        public bool IsRunning { get; private set; } = false;
        private readonly IDispatcherTimer StatusCheckTimer = Dispatcher.GetForCurrentThread().CreateTimer();

        #region Commands
        public ICommand ToggleRedCommand { get; }
        public ICommand ToggleGreenCommand { get; }
        public ICommand ToggleBlueCommand { get; }
        public ICommand ToggleExternalCommand { get; }
        public ICommand ToggleFinishingCommand { get; }
        #endregion

        public KioskViewModel() : base()
        {
            this.Connection.PropertyChanged += ConnectionChanged;
            this.Connection.SerialResponse += ParseResponse;
            this.Connection.SerialResponse -= AddToHistory; // Remove the default handler, writing to history to be handled by `ParseResponse` to avoid filling history with status checks
            this.StatusCheckTimer.Interval = TimeSpan.FromSeconds(1);
            this.StatusCheckTimer.Tick += CheckModeStatus;

            this.ToggleRedCommand = new RelayCommand(() => { this.Colors.Red = !this.Colors.Red; this.ExectuteToggleCommand(ColorKeeper.RGB); this.SendCommands(); }, this.CanSend);
            this.ToggleGreenCommand = new RelayCommand(() => { this.Colors.Green = !this.Colors.Green; this.ExectuteToggleCommand(ColorKeeper.RGB); this.SendCommands(); }, this.CanSend);
            this.ToggleBlueCommand = new RelayCommand(() => { this.Colors.Blue = !this.Colors.Blue; this.ExectuteToggleCommand(ColorKeeper.RGB); this.SendCommands(); }, this.CanSend);
            this.ToggleExternalCommand = new RelayCommand(() => { this.Colors.External = !this.Colors.External; this.ExectuteToggleCommand(ColorKeeper.EXTERNAL); this.SendCommands(); }, this.CanSend);
            this.ToggleFinishingCommand = new RelayCommand(() => { this.Colors.Finishing = !this.Colors.Finishing; this.ExectuteToggleCommand(ColorKeeper.FINISHING); this.SendCommands(); }, this.CanSend);
        }

        private void ParseResponse(string response)
        {
            const string KWD_STATUS = "Status",
                         KWD_COLOR = "olor", // This is pulling double duty; `get_current_color` returns "Color" with upper-case C, but `set_manual_colors` returns lower-case
                         KWD_LED = "Finishing LED";
            foreach (string r in response.Split('\n'))
            {
                if (string.IsNullOrWhiteSpace(r)) continue;
                int c = r.IndexOf(':');
                if (r[(c - KWD_STATUS.Length)..c] == KWD_STATUS)
                {
                    this.IsManualMode = r[(c + 2)..^1] == "manual";
                    if (this.IsManualMode)
                    {
                        this.Connection.SendString("04000000"); // This does not return the state of the finishing LED,...
                        this.Colors.External = false;           // ...so we just set it to off as this should be the case anyways.
                    }
                }
                else if (r[(c - KWD_COLOR.Length)..c] == KWD_COLOR)
                {
                    this.Colors.Red = r[c + 2] == 'R';
                    this.Colors.Green = r[c + 3] == 'G';
                    this.Colors.Blue = r[c + 4] == 'B';
                    this.Colors.External = r[c + 5] == 'E';
                    this.AddToHistory(r);
                }
                else if (r[(c - KWD_LED.Length)..c] == KWD_LED)
                {
                    this.Colors.Finishing = r[c + 3] == 'N'; // `set_finishing_power` gives either "ON" or "OFF", so checking the second character is enough
                    this.AddToHistory(r);
                }
            }
        }

        private void CheckModeStatus(object sender, EventArgs e)
            // Send the "Get Mode" command, response handled by ParseResponse(...). This call does not add anything to history.
            => this.Connection.SendString("13000000");

        private void ConnectionChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SerialConnectionModel.Status))
            {
                // Start or stop the status check timer based on the connection status
                switch (this.Connection.Status)
                {
                    case ConnectionStatus.Connected:
                        this.StatusCheckTimer.Start();
                        break;
                    default:
                        this.StatusCheckTimer.Stop();
                        break;
                }
            }
        }
    }
}
