﻿using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace HoloControl.Models
{
    internal class SerialConnectionModel : INotifyPropertyChanged
    {
        private static readonly Regex CommandFormat = new(@"([0-9a-fA-F]{8})\s?");

        public event PropertyChangedEventHandler PropertyChanged;
        public delegate void SerialResponseEventHandler(string response);
        public event SerialResponseEventHandler SerialResponse;
        public event SerialResponseEventHandler SerialError;
        public event SerialResponseEventHandler SerialStatus;

        public IList<string> AvailablePorts { get => this._availablePorts; private set { this._availablePorts = value; this.Update(); } }
        private IList<string> _availablePorts = PlatformConnectionManager.GetPortNames();
        public ConnectionStatus Status { get => this._status; set { this._status = value; this.Update(); this.ReloadPorts(); } }
        private ConnectionStatus _status = ConnectionStatus.Disconnected;

        public int SelectedPort { get => this._selectedPort; set { this._selectedPort = value; this.Update(); } }
        private int _selectedPort = -1;

        private PlatformConnectionManager OpenPort;
        private Task SerialReader;
        private CancellationTokenSource ReaderCancellation;

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

        public string SendString(string command)
        {
            try
            {
                StringBuilder sent = new();
                if (CommandFormat.IsMatch(command))
                {
                    byte[] send = new byte[4];
                    foreach (ValueMatch cmd in CommandFormat.EnumerateMatches(command))
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            send[j] = Convert.ToByte(command.Substring(cmd.Index + 2 * j, 2), 16);
                        }
                        send[0] += 64;
                        this.OpenPort.Write(send, 0, 4);
                        sent.Append(Encoding.ASCII.GetString(send, 0, 4));
                    }
                    return sent.ToString();
                }
                else return null;
            }
            catch (System.Exception ex)
            {
                this.OpenPort.Close();
                this.Status = ConnectionStatus.Error;
                this.SerialError?.Invoke(ex.Message);
                return null;
            }
        }
        public void Connect()
        {
            if (this.SerialReader != null)
            {
                this.ReaderCancellation.Cancel();
                this.SerialReader.Wait();
                this.SerialReader.Dispose();
            }
            if (this.OpenPort != null)
            {
                string portName = this.OpenPort.GetPortName();
                this.OpenPort.Close();
                this.Status = ConnectionStatus.Disconnected;
                this.SerialStatus?.Invoke("Disconnected from " + portName);
            }
            int portIndex = this.SelectedPort;
            this.ReaderCancellation = new();
            this.SerialReader = new Task(this.KeepReadingSerial, this.ReaderCancellation.Token);
            if (portIndex >= 0)
            {
                this.Status = ConnectionStatus.Connecting;
                PlatformConnectionManager port = PlatformConnectionManager.Create(this.AvailablePorts[portIndex], 9600);
                try
                {
                    port.Open();
                    this.OpenPort = port;
                    this.OpenPort.ErrorReceived += (s, e) => this.SerialError?.Invoke(e.ToString());
                    this.SerialReader.Start();
                    this.Status = ConnectionStatus.Connected;
                    this.SerialStatus?.Invoke("Connected to " + port.GetPortName());
                }
                catch (Exception ex)
                {
                    this.Status = ConnectionStatus.Error;
                    this.SerialError?.Invoke(ex.Message);
                }
            }
        }
        private void ReloadPorts()
        {
            this.AvailablePorts = PlatformConnectionManager.GetPortNames();
            this.SelectedPort = this.OpenPort == null ? -1 : this.AvailablePorts.IndexOf(this.OpenPort.GetPortName());
        }
        private void Update([CallerMemberName] string propertyName = null) { this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    }
}