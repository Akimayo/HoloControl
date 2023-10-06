using Android.App;
using Android.Content;
using Android.Hardware.Usb;
using System.Text;

namespace HoloControl.Models
{
    public partial class PlatformConnectionManager
    {
        private static IDictionary<string, UsbDevice> GetDevices()
        {
            Activity act = Platform.CurrentActivity;
            UsbManager manager = (UsbManager)act.GetSystemService(Context.UsbService);
            return manager.DeviceList;
        }

        public static partial PlatformConnectionManager Create(string portName, int baudRate) => new(portName);
        public static partial IList<string> GetPortNames() => GetDevices().Keys.ToList();


        private readonly UsbDevice _selectedDevice;
        private readonly UsbManager _usbManager;
        private readonly UsbInterface _usbInterface;
        private readonly UsbEndpoint _read, _write;
        private PlatformConnectionManager(string port)
        {
            Activity act = Platform.CurrentActivity;
            UsbManager manager = (UsbManager)act.GetSystemService(Context.UsbService);
            IDictionary<string, UsbDevice> devices = manager.DeviceList;
            UsbDevice device = devices[port];
            const string ACTION_USB_PERMISSION = "rzepak";
            UsbInterface ifc = device.GetInterface(1);
            this._read = ifc.GetEndpoint(0);
            this._write = ifc.GetEndpoint(1);
            PendingIntent intent = PendingIntent.GetBroadcast(Platform.CurrentActivity, 0, new Intent(ACTION_USB_PERMISSION), 0);
            if (manager.HasPermission(device) == false) manager.RequestPermission(device, intent);
            this._selectedDevice = device;
            this._usbManager = manager;
            this._usbInterface = ifc;
        }

        private UsbDeviceConnection _connection;
        public partial void Open()
        {
            UsbDeviceConnection connection = this._usbManager.OpenDevice(this._selectedDevice);
            if (connection != null && connection.ClaimInterface(this._usbInterface, true))
                this._connection = connection;
            else throw new Exception("Connection to device couldn't be established or interface claim was rejected.");
        }

        public partial void Close()
        {
            this._connection.ReleaseInterface(this._usbInterface);
            this._connection.Close();
        }

        public partial void Write(byte[] bytes, int offset, int length) => this._connection.BulkTransfer(this._write, bytes, length, 500);

        private readonly byte[] buffer = new byte[500];
        private int transferred;
        public partial string ReadExisting()
        {
            StringBuilder result = new();
            if (this.transferred > 0)
            {
                result.Append(Encoding.Default.GetString(this.buffer, 0, this.transferred));
            }
            while ((this.transferred = this._connection.BulkTransfer(this._read, this.buffer, 500, 500)) > 0)
                result.Append(Encoding.Default.GetString(this.buffer, 0, this.transferred));
            this.transferred = 0;
            return result.ToString();
        }

        public partial bool HasBytesToRead()
        {
            int transferred = this._connection.BulkTransfer(this._read, this.buffer, 500, 10);
            if (transferred > 0)
            {
                this.transferred = transferred;
                return true;
            }
            else return false;
        }
        public partial string GetPortName() => this._selectedDevice.DeviceName;
    }
}
