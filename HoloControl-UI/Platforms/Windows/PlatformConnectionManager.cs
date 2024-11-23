using System.IO.Ports;
using System.Text;

namespace HoloControl.Models
{
    public partial class PlatformConnectionManager
    {
        private readonly SerialPort _port;
        public static partial PlatformConnectionManager Create(string portName, int baudRate) => new(portName, baudRate);

        private PlatformConnectionManager(string port, int baudRate)
        {
            this._port = new SerialPort(port, baudRate, Parity.None, 8, StopBits.One) { RtsEnable = true, DtrEnable = true, Encoding = Encoding.UTF8 };
            this._port.ErrorReceived += this.ErrorReceived;
        }
        public partial bool HasBytesToRead() => this._port.BytesToRead > 0;

        public static partial IList<string> GetPortNames() => SerialPort.GetPortNames().ToList();

        public partial void Close() => this._port.Close();

        public partial void Open() => this._port.Open();

        public partial string ReadExisting() => this._port.ReadExisting();

        public partial void Write(byte[] bytes, int offset, int length) => this._port.Write(bytes, offset, length);
        public partial string GetPortName() => this._port.PortName;
        public partial bool IsOpen() => this._port.IsOpen;
    }
}
