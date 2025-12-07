using System.ComponentModel;
using System.Runtime.CompilerServices;
using HoloControl.Models;
using System.Windows.Input;
using HoloControl.Models.Form;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace HoloControl.ViewModels
{
    internal class RootViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string CurrentCommand { get => this._currentCommand; set { this._currentCommand = value; this.Update(); } }
        private string _currentCommand = "";

        public string History { get => this._history; set { if (value.Length > 5000) this._history = value.Substring(this._history.Length - 5000, 5000); else this._history = value; this.Update(); } }
        private string _history;

        public ObservableCollection<HistoryItem> HistoryList { get; }

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
        public ICommand Clear { get; }
        #endregion

        protected bool CanSend(string _) => this.Connection.Status == ConnectionStatus.Connected && !this.Sending;
        protected bool CanSend() => this.Connection.Status == ConnectionStatus.Connected && !this.Sending;

        public RootViewModel()
        {
            this.Connection.SerialResponse += this.AddToHistory;
            this.Connection.SerialError += (r) => this.AddToHistory(r, '!');
            this.Connection.SerialStatus += (r) => this.AddToHistory(r, 'i');

            this.HistoryList = new();

            this.SimpleCommand = new RelayCommand<string>(this.ExecuteSimpleCommand, this.CanSend);
            this.ToggleCommand = new RelayCommand<string>(this.ExectuteToggleCommand, this.CanSend);
            this.TimingCommand = new RelayCommand<string>(this.ExecuteTimingCommand, this.CanSend);
            this.Connect = new RelayCommand(this.Connection.Connect);
            this.Send = new RelayCommand(this.SendCommands, this.CanSend);
            this.Clear = new RelayCommand(() => { this.HistoryList.Clear(); this.History = ""; }, this.CanSend);
        }

        protected void ExecuteSimpleCommand(string parameter)
        {
            this.CurrentCommand += parameter + " ";
            /* this.Sending = true;
            byte[] sent = this.Connection.SendString(parameter);
            if (sent.Length > 0) this.AddToHistory(sent);
            this.Sending = false; */
        }
        protected void ExectuteToggleCommand(string color)
        {
            this.ExecuteSimpleCommand(this.Colors[color]);
        }
        protected void ExecuteTimingCommand(string time)
        {
            this.ExecuteSimpleCommand(this.Timings[time]);
        }


        protected void SendCommands()
        {
            this.Sending = true;
            byte[] sent = this.Connection.SendString(this.CurrentCommand.Trim());
            if (sent.Length > 0)
            {
                this.AddToHistory(sent);
                this.CurrentCommand = "";
            }
            this.Sending = false;
        }

        protected void AddToHistory(string lines) // For serial replies
        {
            this.History += lines;
            foreach (string l in lines.Split('\n')) if (!string.IsNullOrWhiteSpace(l)) this.HistoryList.Add(new(l));
            //while (this.HistoryList.Count > 8) this.HistoryList.RemoveAt(0);
        }
        private void AddToHistory(byte[] command)
        {
            HistoryItem entry = new(command);
            this.History += $"[>] {entry.Message}\n";
            this.HistoryList.Add(entry);
        }
        private void AddToHistory(string lines, char indicator)
        {
            HistoryItem entry = new(indicator switch { '>' => HistoryItem.ItemType.Command, 'i' => HistoryItem.ItemType.Info, _ => HistoryItem.ItemType.Error }, lines);
            this.History += $"[{indicator}] {entry.Message}\n";
            this.HistoryList.Add(entry);
        }

        protected void Update([CallerMemberName] string propertyName = null) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
