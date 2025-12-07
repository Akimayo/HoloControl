using System.Collections;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace HoloControl.Models.Form
{
    internal class ColorKeeper : IDictionary<string, string>, INotifyPropertyChanged
    {
        internal const string RGB = "RGB", EXTERNAL = "External", FINISHING = "Finishing";
        public string this[string key]
        {
            get => key switch
            {
                RGB => this.GetRGBString(),
                EXTERNAL => this.GetExternalString(),
                FINISHING => this.GetFinishingString(),
                _ => ""
            }; set { }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private int state = 0;
        private void Set(int mask, bool value)
        {
            if (value) this.state |= mask;
            else this.state &= ~mask;
        }

        public bool Red { get => (this.state & 1) > 0; set { this.Set(1, value); this.Update(); this.Update(nameof(this.RGBString)); } }
        public bool Green { get => (this.state & 2) > 0; set { this.Set(2, value); this.Update(); this.Update(nameof(this.RGBString)); } }
        public bool Blue { get => (this.state & 4) > 0; set { this.Set(4, value); this.Update(); this.Update(nameof(this.RGBString)); } }
        public bool External { get => (this.state & 8) > 0; set { this.Set(8, value); this.Update(); this.Update(nameof(this.ExternalString)); } }
        public bool Finishing { get => (this.state & 16) > 0; set { this.Set(16, value); this.Update(); this.Update(nameof(this.FinishingString)); } }

        public string GetRGBString() => $"030{(this.Red ? 1 : 0)}0{(this.Green ? 1 : 0)}0{(this.Blue ? 1 : 0)}";
        public string GetExternalString() => "0900000" + (this.External ? 1 : 0);
        public string GetFinishingString() => "0500000" + (this.Finishing ? 1 : 0);

        public string RGBString => "0x" + this.GetRGBString();
        public string ExternalString => "0x" + this.GetExternalString();
        public string FinishingString => "0x" + this.GetFinishingString();

        private void Update([CallerMemberName] string propertyName = null) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

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
