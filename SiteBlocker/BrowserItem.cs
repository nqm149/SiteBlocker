using System.ComponentModel;
using System.Runtime.CompilerServices;
using SiteBlocker.Properties;

namespace SiteBlocker
{
    /// <inheritdoc />
    /// <summary>
    /// Browsers to close
    /// </summary>
    public sealed class BrowserItem : INotifyPropertyChanged
    {
        private string _browserName;
        private bool browserCheckbox;

        /// <summary>
        /// Initializes a new instance of the <see cref="BrowserItem" /> class.
        /// </summary>
        /// <param name="browserName"></param>
        /// <param name="browserCheckbox"></param>
        public BrowserItem(string browserName, bool browserCheckbox)
        {
            BrowserName = browserName;
            BrowserCheckbox = browserCheckbox;
        }

        /// <summary>
        /// See <see cref="INotifyPropertyChanged" />
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public string BrowserName
        {
            get => _browserName;
            set
            {
                if (value == _browserName)
                {
                    return;
                }

                _browserName = value;
                OnPropertyChanged();
            }
        }

        public bool BrowserCheckbox

        {
            get => browserCheckbox;
            set
            {
                if (value == browserCheckbox)
                {
                    return;
                }

                browserCheckbox = value;
                OnPropertyChanged();
            }
        }

        public override bool Equals(object obj)
        {
            var browserItem = obj as BrowserItem;

            return browserItem?._browserName == _browserName;
        }

        public override int GetHashCode()
        {
            return _browserName.GetHashCode();
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}