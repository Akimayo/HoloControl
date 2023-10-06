using System.Collections;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace HoloControl.Models.Form
{
    internal class TimeKeeper : IDictionary<string, string>, INotifyPropertyChanged
    {
        const string WAIT = "Wait", RED = "Red", GREEN = "Green", BLUE = "Blue", EXTERNAL = "External", FINISHING = "Finishing";
        public string this[string key]
        {
            get => key switch
            {
                WAIT => this.TimeToHex(0),
                RED => this.TimeToHex(1),
                GREEN => this.TimeToHex(2),
                BLUE => this.TimeToHex(3),
                EXTERNAL => this.TimeToHex(4),
                FINISHING => this.TimeToHex(5),
                _ => ""
            }; set { }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private readonly float[] state = new float[6] { 0, 0, 0, 0, 0, 0 };
        private readonly string[] instr = new string[6] { "17", "12", "07", "02", "08", "06" };

        private string TimeToHex(byte index) => instr[index] + Convert.ToString((int)(this.state[index] * 1000), 16).PadLeft(6, '0');

        public float WaitTime { get => this.state[0]; set { this.state[0] = value; this.Update(WAIT); } }
        public string WaitString => "0x" + TimeToHex(0);
        public float RedTime { get => this.state[1]; set { this.state[1] = value; this.Update(RED); } }
        public string RedString => "0x" + TimeToHex(1);
        public float GreenTime { get => this.state[2]; set { this.state[2] = value; this.Update(GREEN); } }
        public string GreenString => "0x" + TimeToHex(2);
        public float BlueTime { get => this.state[3]; set { this.state[3] = value; this.Update(BLUE); } }
        public string BlueString => "0x" + TimeToHex(3);
        public float ExternalTime { get => this.state[4]; set { this.state[4] = value; this.Update(EXTERNAL); } }
        public string ExternalString => "0x" + TimeToHex(4);
        public float FinishingTime { get => this.state[5]; set { this.state[5] = value; this.Update(FINISHING); } }
        public string FinishingString => "0x" + TimeToHex(5);

        public void Reset()
        {
            for (int i = 0; i < 6; i++) this.state[i] = 0;
        }

        private void Update(string property)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property + "Time"));
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property + "String"));
        }

        #region Unused
        public ICollection<string> Keys => throw new NotImplementedException();

        public ICollection<string> Values => throw new NotImplementedException();

        public int Count => throw new NotImplementedException();

        public bool IsReadOnly => throw new NotImplementedException();


        public void Add(string key, string value)
        {
            throw new NotImplementedException();
        }

        public void Add(KeyValuePair<string, string> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<string, string> item)
        {
            throw new NotImplementedException();
        }

        public bool ContainsKey(string key)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public bool Remove(string key)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, string> item)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out string value)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
