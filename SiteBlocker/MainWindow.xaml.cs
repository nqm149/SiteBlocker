using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;
using SiteBlocker.Models;
using SiteBlocker.Properties;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace SiteBlocker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        private const string HostsPath = "C:\\Windows\\System32\\drivers\\etc\\hosts";
        private static readonly List<string> browsers = new List<string> { "chrome", "MicrosoftEdge", "firefox" };
        private static readonly List<string> defaultSites = new List<string> { "www.facebook.com", "www.youtube.com" };
        private const string DefaultUrlPrefix = "http://";

        private const string DefaultHost = "127.0.0.1 ";
        private readonly string _originalContent;
        DispatcherTimer _timer;
        private Process[] processes;

        private Model Model => DataContext as Model;

        public MainWindow()
        {
            InitializeComponent();
            using (var sr = new StreamReader(HostsPath))
                _originalContent = sr.ReadToEndAsync().Result;
            Debug.WriteLine("Read hosts file's original content.");

            // adding default sites
            foreach (var site in defaultSites)
                Model.Input.Add(new InputItem(site));
        }

        public void CloseWindow(object sender, CancelEventArgs cancelEventArgs)
        {
            //return original content to hosts file
            RestoreHostsFile();
        }
        private void AddUrl()
        {
            var modelNewUri = $"{DefaultUrlPrefix}{NewUri}".Trim();
            if (Uri.TryCreate(modelNewUri, UriKind.Absolute, out _))
            {
                Model.Input.Add(new InputItem(modelNewUri));
                Model.NewUri = string.Empty;
            }
            else
            {
                MessageBox.Show("Not a URI.");
            }
        }

        private void RemoveBtn_Click(object sender, RoutedEventArgs e)
        {
            var urlItem = ((FrameworkElement)sender).DataContext as InputItem;
            Model.Input.Remove(urlItem);
        }

        private void StartBtn(object sender, RoutedEventArgs e)
        {
            foreach (var browser in browsers)
            {
                if (StillHasProcess(browser))
                {
                    var dialogResult = MessageBox.Show("This will close all your opening browsers and tabs and block Facebook !",
                            "Warning", MessageBoxButton.OKCancel);

                    if (dialogResult == MessageBoxResult.OK)
                    {
                        //first, close all browsers
                        while (StillHasProcess(browser))
                        {
                            processes = GetAllProcessesByName(browser);

                            foreach (var p in processes)
                            {
                                Debug.WriteLine($"Found browser related process. Close it now. {p.ProcessName}");
                                p.CloseMainWindow();
                            }
                        }
                    }
                    else if (dialogResult == MessageBoxResult.Cancel)
                    {
                        //do nothing
                    }
                }

            }

            BlockSite();

        }

        private void StopBtn(object sender, RoutedEventArgs e)
        {
            RestoreHostsFile();
            MessageBox.Show("Now you can access Facebook again !");
            _timer.Stop();
            TimeLeft.Text = "00:00:00";
        }

        private bool StillHasProcess(string processName) => Process.GetProcesses().Any(p => p.ProcessName == processName);

        private Process[] GetAllProcessesByName(string processName) => Process.GetProcessesByName(processName);

        private void RestoreHostsFile()
        {
            using (var sw = new StreamWriter(HostsPath, false))
                sw.WriteAsync(_originalContent);
            Debug.WriteLine("Restore hosts file's original content.");
        }
        private void BlockSite()
        {
            //then do the magic
            var time = TimeSpan.FromSeconds(int.Parse(TimeSet.Text));

            try
            {

                using (var sw = new StreamWriter(HostsPath, true))
                {
                    foreach (var url in Model.Input)
                    {
                        sw.WriteAsync(Environment.NewLine + DefaultHost + url.Uri);
                    }
                }

                MessageBox.Show("Blocked Facebook !");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }

            _timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
            {
                TimeLeft.Text = time.ToString("c");
                if (time == TimeSpan.Zero)
                {
                    _timer.Stop();
                    using (var sw = new StreamWriter(HostsPath, false))
                    {
                        sw.WriteAsync(_originalContent);
                    }

                    MessageBox.Show("Now you can access Facebook again !");
                }
                time = time.Add(TimeSpan.FromSeconds(-1));
            }, Application.Current.Dispatcher);

            _timer.Start();

        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            AddUrl();
        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            Model.Input.Clear();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
