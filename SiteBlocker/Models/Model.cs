using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using SiteBlocker.Properties;

namespace SiteBlocker.Models
{
    /// <inheritdoc />
    /// <summary>
    /// The main View Model.
    /// </summary>
    public class Model : INotifyPropertyChanged
    {
        private readonly StringBuilder log = new StringBuilder();
        private string newUri;
        private bool startBtnStatus = true;
        private bool stopBtnStatus;

        /// <summary>
        /// See <see cref="INotifyPropertyChanged" />
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets uRI to add.
        /// </summary>
        public string NewUri
        {
            get => newUri;
            set
            {
                if (value == newUri)
                {
                    return;
                }

                newUri = value;
                OnPropertyChanged();
            }
        }

        public bool StartBtnStatus
        {
            get => startBtnStatus;
            set
            {
                if (value == startBtnStatus)
                {
                    return;
                }

                startBtnStatus = value;
                OnPropertyChanged();
            }
        }

        public bool StopBtnStatus
        {
            get => stopBtnStatus;
            set
            {
                if (value == stopBtnStatus)
                {
                    return;
                }

                stopBtnStatus = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets initial Crawling URIs.
        /// </summary>
        public ObservableCollection<InputItem> Input { get; } = new ObservableCollection<InputItem>();
        public ObservableCollection<BrowserItem> BrowserList { get; } = new ObservableCollection<BrowserItem>();

        /// <summary>
        /// Gets uRIs found by searching.
        /// </summary>
        public ObservableCollection<string> SearchResult { get; } = new ObservableCollection<string>();

        /// <summary>
        /// Gets or sets cancellation Token Source to stop crawler.
        /// </summary>
        public CancellationTokenSource CrawlerCancellation { get; set; }

        /// <summary>
        /// Gets log content.
        /// </summary>
        public string Log => log.ToString();

        /// <summary>
        /// Add line into log.
        /// </summary>
        /// <param name="line">Line to add.</param>
        public void AddLogLine(string line)
        {
            var culture = new CultureInfo("en-GB");
            log.AppendLine($"{DateTime.Now.ToString(culture)} : {line}");
            OnPropertyChanged(nameof(Log));
        }

        /// <summary>
        /// Reset Log content.
        /// </summary>
        public void ResetLog()
        {
            log.Clear();
        }

        /// <summary>
        /// See <see cref="INotifyPropertyChanged" />
        /// </summary>
        /// <param name="propertyName">Name of property to notify.</param>
        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}