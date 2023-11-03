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

        public string Icon { get; private set; }
        public string Message { get; private set; }
        public DateTime Timestamp { get; }
        public string Font { get; }

        public HistoryItem(ItemType type, string message)
        {
            this.Timestamp = DateTime.Now;
            this.Message = message;
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
                    break;
                case ItemType.Response:
                    this.Font = ItemFonts[1];
                    this.ParseMessage(message);
                    break;
            }
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
