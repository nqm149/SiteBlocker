
namespace CrawlerUI
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using JetBrains.Annotations;

    /// <inheritdoc />
    /// <summary>
    /// Initial crawling URI model item.
    /// </summary>
    public sealed class InputItem : INotifyPropertyChanged
    {
        private int depth;
        private int hostDepth;
        private string uri;

        /// <summary>
        /// Initializes a new instance of the <see cref="InputItem" /> class.
        /// </summary>
        /// <param name="uri">Crawling URI.</param>
        /// <param name="depth">Crawling depth.</param>
        /// <param name="hostDepth">Crawling host depth.</param>
        public InputItem(string uri, int depth, int hostDepth)
        {
            Uri = uri;
            Depth = depth;
            HostDepth = hostDepth;
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

        /// <summary>
        /// Gets or sets crawling depth for this URI. Currently not used.
        /// </summary>
        public int Depth
        {
            get => depth;
            set
            {
                if (value == depth)
                {
                    return;
                }

                depth = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets crawling host depth. Currently not used.
        /// </summary>
        public int HostDepth
        {
            get => hostDepth;
            set
            {
                if (value == hostDepth)
                {
                    return;
                }

                hostDepth = value;
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