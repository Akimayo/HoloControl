using System.ComponentModel;
using System.Runtime.CompilerServices;
using HoloControl.Models;
using System.Windows.Input;
using HoloControl.Models.Form;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.Input;

namespace HoloControl.ViewModels
{
    internal class RootViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string CurrentCommand { get => this._currentCommand; set { this._currentCommand = value; this.Update(); } }
        private string _currentCommand = "";

        public string History { get => this._history; set { if (value.Length > 5000) this._history = value.Substring(this._history.Length - 5000, 5000); else this._history = value; this.Update(); } }
        private string _history;

        public bool Sending { get => this._sending; set { this._sending = value; this.Update(); } }
        private bool _sending = false;

        public SerialConnectionModel Connection { get; } = new SerialConnectionModel();

        public ColorKeeper Colors { get; } = new();
        public TimeKeeper Timings { get; } = new();

        #region Commands
        public ICommand SimpleCommand { get; }
        public ICommand ToggleCommand { get; }
        public ICommand TimingCommand { get; }
        public ICommand TimeResetCommand { get; }
        public ICommand Connect { get; }
        public ICommand Send { get; }
        #endregion

        public RootViewModel()
        {
            this.Connection.SerialResponse += this.AddToHistory;
            this.Connection.SerialError += (r) => this.AddToHistory(r, '!');
            this.Connection.SerialStatus += (r) => this.AddToHistory(r, 'i');

            this.SimpleCommand = new RelayCommand<string>(this.ExecuteSimpleCommand);
            this.ToggleCommand = new RelayCommand<string>(this.ExectuteToggleCommand);
            this.TimingCommand = new RelayCommand<string>(this.ExecuteTimingCommand);
            this.Connect = new RelayCommand(this.Connection.Connect);
            this.Send = new RelayCommand(this.SendCommands, () => this.Connection.Status == ConnectionStatus.Connected && !this.Sending);
        }

        private void ExecuteSimpleCommand(string parameter)
        {
            this.CurrentCommand += parameter + " ";
        }
        private void ExectuteToggleCommand(string color)
        {
            this.ExecuteSimpleCommand(this.Colors[color]);
        }
        private void ExecuteTimingCommand(string time)
        {
            this.ExecuteSimpleCommand(this.Timings[time]);
        }

        private static readonly Regex InvisibleStripper = new(@"[^\x20-\x7e\x80\x82-\x8c\x8e\x91-\x9c\x9e-\xff]");
        private static string Replacer(Match s) => ((int)s.Value[0] & 1) > 0 ? "⬜" : "▫️";
        private void SendCommands()
        {
            this.Sending = true;
            string sent = this.Connection.SendString(this.CurrentCommand);
            if (!string.IsNullOrEmpty(sent))
            {
                this.AddToHistory(InvisibleStripper.Replace(sent, Replacer), '>');
                this.CurrentCommand = "";
            }
            this.Sending = false;
        }

        private void AddToHistory(string lines) => this.History += lines;
        private void AddToHistory(string lines, char indicator) => this.History += $"[{indicator}] {lines}\n";

        private void Update([CallerMemberName] string propertyName = null) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
