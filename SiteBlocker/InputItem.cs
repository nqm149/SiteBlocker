using System.ComponentModel;
using System.Runtime.CompilerServices;
using SiteBlocker.Properties;

namespace SiteBlocker
{
    /// <inheritdoc />
    /// <summary>
    /// Initial crawling URI model item.
    /// </summary>
    public sealed class InputItem : INotifyPropertyChanged
    {
        private string uri;

        /// <summary>
        /// Initializes a new instance of the <see cref="InputItem" /> class.
        /// </summary>
        /// <param name="uri">Crawling URI.</param>
        public InputItem(string uri)
        {
            Uri = uri;
        }

        /// <summary>
        /// See <see cref="INotifyPropertyChanged" />
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets initial crawling URI.
        /// </summary>
        public string Uri
        {
            get => uri;
            set
            {
                if (value == uri)
                {
                    return;
                }

                uri = value;
                OnPropertyChanged();
            }
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}