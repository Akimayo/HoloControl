using System.Text.RegularExpressions;

namespace HoloControl.ViewModels
{
    internal struct HistoryItem
    {
        internal enum ItemType
        {
            Info,
            Error,
            Command,
            Response
        }
        private static readonly string[] ItemTypeIcons = { "cmd_usb.png", "cmd_warning_diamond.png", "cmd_paper_plane_right.png", "cmd_check_square.png", "cmd_x_square.png", "cmd_play_circle.png", "cmd_pause_circle.png", "cmd_stop_circle.png" };
        private static readonly string[] ItemFonts = { "B612", "B612 Mono" };
        public static readonly Regex InvisibleStripper = new(@"[^\x20-\x7e\x80\x82-\x8c\x8e\x91-\x9c\x9e-\xff]");
        public static string Replacer(Match s) => ((int)s.Value[0] & 1) > 0 ? "⬜" : "▫️";

        public string Icon { get; private set; }
        public string Message { get; private set; }
        public string Hex { get; private set; }
        public DateTime Timestamp { get; }
        public string Font { get; }

        public HistoryItem(ItemType type, string message)
        {
            this.Timestamp = DateTime.Now;
            this.Message = message;
            this.Hex = null;
            switch (type)
            {
                case ItemType.Info:
                    this.Icon = ItemTypeIcons[0];
                    this.Font = ItemFonts[0];
                    break;
                case ItemType.Error:
                    this.Icon = ItemTypeIcons[1];
                    this.Font = ItemFonts[0];
                    break;
                case ItemType.Command:
                    this.Icon = ItemTypeIcons[2];
                    this.Font = ItemFonts[1];
                    this.Message = InvisibleStripper.Replace(message, Replacer);
                    break;
                case ItemType.Response:
                    this.Font = ItemFonts[1];
                    this.ParseMessage(message);
                    break;
            }
        }
        public HistoryItem(byte[] command)
        {
            this.Timestamp = DateTime.Now;
            string ascii = System.Text.Encoding.ASCII.GetString(command);
            this.Message = InvisibleStripper.Replace(ascii, Replacer);
            this.Hex = "0x" + Convert.ToHexString(command);
            this.Icon = ItemTypeIcons[2];
            this.Font = ItemFonts[1];
        }
        public HistoryItem(string message)
        {
            this.Timestamp = DateTime.Now;
            this.Font = ItemFonts[1];
            this.ParseMessage(message);
        }
        private void ParseMessage(string message)
        {
            message = message.Trim();
            this.Message = message.Substring(3);
            switch (message[0])
            {
                case '✅': this.Icon = ItemTypeIcons[3]; break;
                case '❌':
                    this.Icon = ItemTypeIcons[4]; break;
                case '▶': this.Icon = ItemTypeIcons[5]; break;
                case '⏸': this.Icon = ItemTypeIcons[6]; break;
                case '⏹':
                    this.Icon = ItemTypeIcons[7]; break;
                default: this.Icon = ItemTypeIcons[1]; this.Message = message; break;
            }
        }
    }
}
