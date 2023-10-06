using System.IO.Ports;

namespace HoloControl.Models
{
    public partial class PlatformConnectionManager
    {
        public event SerialErrorReceivedEventHandler ErrorReceived;

        public static partial IList<string> GetPortNames();
        public static partial PlatformConnectionManager Create(string portName, int baudRate);
        public partial bool HasBytesToRead();
        public partial string ReadExisting();
        public partial void Write(byte[] bytes, int offset, int length);
        public partial void Open();
        public partial void Close();
        public partial string GetPortName();
    }
}
