using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace HoloControl.Models
{
    internal class SerialConnectionModel : INotifyPropertyChanged
    {
        private static readonly Regex CommandFormat = new(@"([0-9a-fA-F]{1,8})\s?"), InitReply = new(@"^HoloControl;b:([0-9A-Fa-f]*);v:([A-Za-z 0-9:]+)");

        public event PropertyChangedEventHandler PropertyChanged;
        public delegate void SerialResponseEventHandler(string response);
        public event SerialResponseEventHandler SerialResponse;
        public event SerialResponseEventHandler SerialError;
        public event SerialResponseEventHandler SerialStatus;

        public IList<string> AvailablePorts { get => this._availablePorts; private set { this._availablePorts = value; this.Update(); } }
        private IList<string> _availablePorts = PlatformConnectionManager.GetPortNames();
        public ConnectionStatus Status { get => this._status; set { this._status = value; this.Update(); this.ReloadPorts(); } }
        private ConnectionStatus _status = ConnectionStatus.Disconnected;

        public DateTime BoardBuildTime { get => this._boardBuildTime; private set { this._boardBuildTime = value; this.Update(); this.Update(nameof(this.Board)); } }
        private DateTime _boardBuildTime;

        public string BoardUid { get => this._boardUid; private set { this._boardUid = value; this.Update(); this.Update(nameof(this.Board)); } }
        private string _boardUid;

        public string Board { get => $"UID: {this.BoardUid}\r\nBuild Date: {this.BoardBuildTime}"; }

        public int SelectedPort { get => this._selectedPort; set { this._selectedPort = value; this.Update(); } }
        private int _selectedPort = -1;

        private PlatformConnectionManager OpenPort;
        private Task SerialReader;
        private CancellationTokenSource ReaderCancellation;
        private readonly IDispatcherTimer AliveTimer = Dispatcher.GetForCurrentThread().CreateTimer();

        private void KeepReadingSerial()
        {
            while (!this.ReaderCancellation.Token.IsCancellationRequested)
            {
                if (this.OpenPort.HasBytesToRead())
                {
                    string incoming = this.OpenPort.ReadExisting();
                    MainThread.BeginInvokeOnMainThread(() => this.SerialResponse?.Invoke(incoming));
                }
            }
        }

        public byte[] SendString(string command)
        {
            try
            {
                List<byte> sent = new();
                if (CommandFormat.IsMatch(command))
                {
                    int index, maxIndex, i = 0;
                    byte[] send = new byte[4];
                    foreach (ValueMatch cmd in CommandFormat.EnumerateMatches(command))
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            index = cmd.Index + 2 * j;
                            maxIndex = cmd.Index + cmd.Length;
                            if (char.IsWhiteSpace(command[maxIndex - 1])) maxIndex--;
                            if (index + 1 < maxIndex) send[j] = Convert.ToByte(command.Substring(index, 2), 16);
                            else if (index < maxIndex) send[j] = Convert.ToByte(command.Substring(index, 1) + "0", 16);
                            else send[j] = 0;
                        }
                        if (send[0] < 64) send[0] += 64; // Make it a letter for easier reading
                        Task.Delay(1000);
                        this.OpenPort.Write(send, 0, 4);
                        sent.AddRange(send);
                        i++;
                    }
                    return sent.ToArray();
                }
                else return Array.Empty<byte>();
            }
            catch (System.Exception ex)
            {
                if (this.OpenPort.IsOpen()) this.OpenPort.Close();
                this.ReaderCancellation.Cancel();
                this.Status = ConnectionStatus.Error;
                this.SerialError?.Invoke(ex.Message);
                return Array.Empty<byte>();
            }
        }
        public void Connect()
        {
            this.AliveTimer.Stop();
            int portIndex = this.SelectedPort;
            if (this.SerialReader != null)
            {
                this.ReaderCancellation.Cancel();
                if (!this.SerialReader.IsCanceled) this.SerialReader.Wait();
                if (this.SerialReader.IsCompleted) this.SerialReader.Dispose();
            }
            if (this.OpenPort != null)
            {
                string portName = this.OpenPort.GetPortName();
                this.OpenPort.Close();
                this.Status = ConnectionStatus.Disconnected;
                this.SerialStatus?.Invoke("Disconnected from " + portName);
            }
            this.BoardBuildTime = DateTime.MinValue;
            this.BoardUid = null;
            this.ReaderCancellation = new();
            this.SerialReader = new Task(this.KeepReadingSerial, this.ReaderCancellation.Token);
            this.AliveTimer.Interval = TimeSpan.FromSeconds(5);
            this.AliveTimer.Tick += this.CheckPortAlive;
            if (portIndex >= 0)
            {
                this.Status = ConnectionStatus.Connecting;
                PlatformConnectionManager port = PlatformConnectionManager.Create(this.AvailablePorts[portIndex], 115200);
                this.SerialStatus?.Invoke("Connecting to " + port.GetPortName() + "...");
                try
                {
                    port.Open();
                    this.OpenPort = port;
                    this.OpenPort.ErrorReceived += (s, e) => this.SerialError?.Invoke(e.ToString());
                    port.Write(new byte[] { 0, 0, 0, 0 }, 0, 4);
                    string comm = port.ReadExisting();
                    if (!string.IsNullOrEmpty(comm))
                    {
                        if (InitReply.IsMatch(comm))
                        {
                            Match matches = InitReply.Match(comm);
                            this.BoardBuildTime = DateTime.ParseExact(matches.Groups[2].Value, "ddd MMM dd HH:mm:ss yyyy", System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat);
                            this.BoardUid = matches.Groups[1].Value;
                            this.SerialReader.Start();
                            this.AliveTimer.Start();
                            this.Status = ConnectionStatus.Connected;
                            this.SerialStatus?.Invoke("Connected to a HoloControl board on port " + port.GetPortName());
                            this.SelectedPort = portIndex;
                        }
                        else throw new Exception("Device is not a HoloControl board");
                    }
#if TRACE
                    else
                    {
                        this.SerialReader.Start();
                        this.Status = ConnectionStatus.Connected;
                        this.SerialStatus?.Invoke("Connected to port " + port.GetPortName() + ", possibly a null emulator");
                        this.SelectedPort = portIndex;
                    }
#else
                    else throw new Exception("Device did not respond");
#endif
                }
                catch (Exception ex)
                {
                    this.Status = ConnectionStatus.Error;
                    this.SerialError?.Invoke(ex.Message);
                }
            }
        }
        private void CheckPortAlive(object _, EventArgs __) => this.CheckPortAlive();
        private void CheckPortAlive()
        {
            if (this.OpenPort != null && !this.OpenPort.IsOpen())
            {
                this.ReaderCancellation.Cancel();
                this.Status = ConnectionStatus.Disconnected;
                this.SerialStatus?.Invoke("Port " + this.AvailablePorts[this.SelectedPort] + " disconnected");
                this.SelectedPort = -1;
                this.AliveTimer.Stop();
            }
        }
        public void ReloadPorts()
        {
            this.AvailablePorts = PlatformConnectionManager.GetPortNames();
            this.SelectedPort = this.OpenPort == null ? -1 : this.AvailablePorts.IndexOf(this.OpenPort.GetPortName());
        }
        private void Update([CallerMemberName] string propertyName = null) { this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    }
}
