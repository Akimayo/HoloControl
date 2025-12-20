using CommunityToolkit.Mvvm.Input;
using HoloControl.Models;
using HoloControl.Models.Form;
using System.Diagnostics;
using System.Windows.Input;

namespace HoloControl.ViewModels
{
    internal class KioskViewModel : RootViewModel
    {
        private bool _isManualMode = true, _isRunning = false, _isPaused = false, _isOutputVisible = true;
        public bool IsManualMode { get => this._isManualMode; private set { this._isManualMode = value; this.Update(); } }
        public bool IsRunning { get => this._isRunning; private set { this._isRunning = value; this.Update(); } }
        public bool IsPaused { get => this._isPaused; private set { this._isPaused = value; this.Update(); } }
        public bool IsOutputVisible { get => this._isOutputVisible; set { this._isOutputVisible = value; this.Update(); } }
        private readonly IDispatcherTimer StatusCheckTimer = Dispatcher.GetForCurrentThread().CreateTimer();
        private readonly IDispatcherTimer ExpositionTimer = Dispatcher.GetForCurrentThread().CreateTimer();
        private readonly Stopwatch ExpositionStopwatch = new();

        private TimeSpan _totalTime, _remainingTime;
        public TimeSpan RemainingTime { get => this._remainingTime; set { this._remainingTime = value; this.Update(); this.Update(nameof(this.RemainingFraction)); }  }
        public double RemainingFraction => 1 - (this._remainingTime / this._totalTime);

        #region Commands
        public ICommand ToggleRedCommand { get; }
        public ICommand ToggleGreenCommand { get; }
        public ICommand ToggleBlueCommand { get; }
        public ICommand ToggleExternalCommand { get; }
        public ICommand ToggleFinishingCommand { get; }
        public ICommand StartCommand { get; }
        public ICommand PauseCommand { get; }
        public ICommand StopCommand { get; }
        #endregion

        public KioskViewModel() : base()
        {
            this.Connection.PropertyChanged += ConnectionChanged;
            this.Connection.SerialResponse += ParseResponse;
            this.Connection.SerialResponse -= AddToHistory; // Remove the default handler, writing to history to be handled by `ParseResponse` to avoid filling history with status checks
            this.StatusCheckTimer.Interval = TimeSpan.FromSeconds(1);
            this.StatusCheckTimer.Tick += CheckModeStatus;
            this.ExpositionTimer.IsRepeating = false;
            this.ExpositionTimer.Tick += (s, e) =>
            {
                this.IsRunning = false;
                this.ExpositionStopwatch.Stop();
                this.ExpositionStopwatch.Reset();
            };
            this.Timings.PropertyChanged += OnTimingsChanged;

            this.ToggleRedCommand = new RelayCommand(() => { this.Colors.Red = !this.Colors.Red; this.ExectuteToggleCommand(ColorKeeper.RGB); this.SendCommands(); }, this.CanSend);
            this.ToggleGreenCommand = new RelayCommand(() => { this.Colors.Green = !this.Colors.Green; this.ExectuteToggleCommand(ColorKeeper.RGB); this.SendCommands(); }, this.CanSend);
            this.ToggleBlueCommand = new RelayCommand(() => { this.Colors.Blue = !this.Colors.Blue; this.ExectuteToggleCommand(ColorKeeper.RGB); this.SendCommands(); }, this.CanSend);
            this.ToggleExternalCommand = new RelayCommand(() => { this.Colors.External = !this.Colors.External; this.ExectuteToggleCommand(ColorKeeper.EXTERNAL); this.SendCommands(); }, this.CanSend);
            this.ToggleFinishingCommand = new RelayCommand(() => { this.Colors.Finishing = !this.Colors.Finishing; this.ExectuteToggleCommand(ColorKeeper.FINISHING); this.SendCommands(); }, this.CanSend);
            this.StartCommand = new RelayCommand(() =>
            {
                if (this.IsPaused)
                    // If the exposition has been paused, subtract the already elapsed time from total and tack on waiting time
                    this.ExpositionTimer.Interval = this.ExpositionTimer.Interval - this.ExpositionStopwatch.Elapsed + TimeSpan.FromSeconds(this.Timings.WaitTime);
                else
                    // If this is a fresh exposure, set the total time needed for the exposure
                    this.ExpositionTimer.Interval = TimeSpan.FromSeconds(this.Timings.GetTotalTime());
                this.IsRunning = true;
                this.IsPaused = false;
                this.ExecuteSimpleCommand("18000000");
                this.ExpositionTimer.Start();
                this.ExpositionStopwatch.Restart();
            });
            this.PauseCommand = new RelayCommand(() =>
            {
                this.IsRunning = false;
                this.IsPaused = true;
                this.ExecuteSimpleCommand("19000000");
                this.ExpositionTimer.Stop();
                this.ExpositionStopwatch.Stop();
            });
            this.StopCommand = new RelayCommand(() =>
            {
                this.IsRunning = false;
                this.IsPaused = false;
                this.ExecuteSimpleCommand("1A000000");
                this.ExpositionTimer.Stop();
                this.ExpositionStopwatch.Stop();
                this.ExpositionStopwatch.Reset();
            });
        }

        protected override void ExecuteSimpleCommand(string parameter)
        {
            base.ExecuteSimpleCommand(parameter);
            this.SendCommands();
        }

        private void OnTimingsChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this._totalTime = this.RemainingTime = TimeSpan.FromSeconds(this.Timings.GetTotalTime());
            if (this.CanSend() && e.PropertyName.EndsWith("String"))
                this.ExecuteTimingCommand(e.PropertyName[..^6]);
        }

        private bool colorsRequested = false, timingsRequested = false;
        private void ParseResponse(string response)
        {
            const string KWD_STATUS = "Status",
                         KWD_COLOR = "olor", // This is pulling double duty; `get_current_color` returns "Color" with upper-case C, but `set_manual_colors` returns lower-case
                         KWD_LED = "Finishing LED",
                         KWD_TIME = "time [ms]";
            bool processedRequestedTimings = false;
            foreach (string r in response.Split('\n'))
            {
                if (string.IsNullOrWhiteSpace(r)) continue;
                int c = r.IndexOf(':');
                if (c < 0)
                {
                    // An error message was received, open the output to show to user
                    this.IsOutputVisible = true;
                    this.AddToHistory(r);
                    this.IsRunning = false;
                    this.IsPaused = false;
                    this.ExpositionTimer.Stop();
                    this.ExpositionStopwatch.Stop();
                    this.ExpositionStopwatch.Reset();
                }
                else if (r[(c - KWD_STATUS.Length)..c] == KWD_STATUS)
                {
                    if (c + 2 >= r.Length) continue; // Skip this block if the status is unset

                    bool isManualMode = r[(c + 2)..^1] == "manual";

                    // When the board is switched to a new mode and we are learning of the change now, `isManualMode != this.IsManualMode` in the first pass.
                    // Therefore we get the relevant control information from the board itself once.
                    if (isManualMode && !this.IsManualMode)
                    {
                        // Get state of colors
                        this.colorsRequested = true;
                        this.Connection.SendString("04000000"); // This does not return the state of the finishing LED,...
                        this.Colors.External = false;           // ...so we just set it to off as this should be the case anyways.
                    }
                    else if (!isManualMode && this.IsManualMode)
                    {
                        // Get timings
                        this.timingsRequested = true;
                        this.Connection.SendString("14000000");
                    }
                    // Now we store the new mode and propagate it to the rest of the app
                    this.IsManualMode = isManualMode;
                }
                else if (r[(c - KWD_COLOR.Length)..c] == KWD_COLOR)
                {
                    this.Colors.Red = r[c + 2] == 'R';
                    this.Colors.Green = r[c + 3] == 'G';
                    this.Colors.Blue = r[c + 4] == 'B';
                    this.Colors.External = r[c + 5] == 'E';

                    // Unless the check was requested automatically during mode change, add the response to history
                    if (!this.colorsRequested) this.AddToHistory(r);
                    else this.colorsRequested = false;
                }
                else if (r[(c - KWD_LED.Length)..c] == KWD_LED)
                {
                    this.Colors.Finishing = r[c + 3] == 'N'; // `set_finishing_power` gives either "ON" or "OFF", so checking the second character is enough
                    this.AddToHistory(r);
                }
                else if (r[(c - KWD_TIME.Length)..c] == KWD_TIME && uint.TryParse(r[(c + 2)..], out uint timeMs))
                {
                    // Convert time from milliseconds to seconds
                    float time = timeMs / 1e3f;
                    // Exposition, waiting and finishing time have the same format and should be distinguishable by the seventh character in front of 'time'. For exposition
                    // times, this is the last character of the color name, and for finishing and waiting it's just somewhere in the middle of the word, but still enough to
                    // distinguish.
                    switch (r[c - KWD_TIME.Length - 7])
                    {
                        case 'd': // Red
                            this.Timings.RedTime = time;
                            break;
                        case 'n': // Green
                            this.Timings.GreenTime = time;
                            break;
                        case 'e': // Blue
                            this.Timings.BlueTime = time;
                            break;
                        case 'l': // External
                            this.Timings.ExternalTime = time;
                            break;
                        case 'i': // Finishing
                            this.Timings.FinishingTime = time;
                            break;
                        case 'a': // Waiting
                            this.Timings.WaitTime = time;
                            break;
                    }
                    // Unless the check was requested automatically during mode change, add  the response to history
                    if (!this.timingsRequested) this.AddToHistory(r);
                    else processedRequestedTimings = true;
                }
                else this.AddToHistory(r + " (parsing failed)"); // Fallback when parsing fails
            }

            // Reset `this.timingsRequested` if they were processed in this batch
            if (processedRequestedTimings) this.timingsRequested = false;
        }

        private void CheckModeStatus(object sender, EventArgs e)
        {
            // Send the "Get Mode" command, response handled by ParseResponse(...). This call does not add anything to history.
            this.Connection.SendString("13000000");
            // If the exposition is running, update the displayed remaining time
            if (this.IsRunning) this.RemainingTime = TimeSpan.FromSeconds(this.Timings.GetTotalTime()) - this.ExpositionStopwatch.Elapsed;
        }

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
                        this.IsRunning = false;
                        this.IsPaused = false;
                        this.ExpositionTimer.Stop();
                        this.ExpositionStopwatch.Stop();
                        this.ExpositionStopwatch.Reset();
                        break;
                }
            }
        }
    }
}
