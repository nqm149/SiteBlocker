using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using WpfApp1.Annotations;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private const string HostsPath = "C:\\Windows\\System32\\drivers\\etc\\hosts";
        private const string DefaultHost = "127.0.0.1 ";
        private string _originalContent;
        DispatcherTimer _timer;
        private Process[] processes;

        public MainWindow()
        {
            InitializeComponent();
            using (var sr = new StreamReader(HostsPath))
            {
                _originalContent = sr.ReadToEndAsync().Result;
            }

            _startBtnStatus = true;
            _stopBtnStatus = false;

        }

        public void CloseWindow(object sender, CancelEventArgs cancelEventArgs)
        {
            //return original content to hosts file
            RestoreHostsFile();
        }

        private void StartBtn(object sender, RoutedEventArgs e)
        {
            _stopBtnStatus = true;
            if (StillHasProcess("chrome"))
            {
                MessageBoxResult dialogResult = MessageBox.Show("This will close all your browsers and block Facebook !", "Warning",
                    MessageBoxButton.OKCancel);

                if (dialogResult == MessageBoxResult.OK)
                {
                    //first, close all browsers
                    while (StillHasProcess("chrome"))
                    {
                        processes = GetAllProcessesByName("chrome");

                        foreach (var p in processes)
                        {
                            p.CloseMainWindow();
                        }
                    }

                    DoTheMagic();
                }
                else if (dialogResult == MessageBoxResult.Cancel)
                {
                    //do nothing
                }
            }
        }

        private void StopBtn(object sender, RoutedEventArgs e)
        {
            RestoreHostsFile();
            MessageBox.Show("Now you can access Facebook again !");

            _startBtnStatus = true;
        }

        private bool StillHasProcess(string processName) => Process.GetProcesses().Any(p => p.ProcessName == processName);

        private Process[] GetAllProcessesByName(string processName) => Process.GetProcessesByName(processName);

        private void RestoreHostsFile()
        {
            using (var sw = new StreamWriter(HostsPath, false))
            {
                sw.WriteAsync(_originalContent);
            }
        }
        private void DoTheMagic()
        {
            //then do the magic
            var time = TimeSpan.FromSeconds(int.Parse(TimeSet.Text));

            try
            {

                using (var sw = new StreamWriter(HostsPath, true))
                {
                    foreach (UrlItem url in UrlList.Items)
                    {
                        sw.WriteAsync(Environment.NewLine + DefaultHost + url.Text);
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
            var newUrl = new UrlItem { Text = UrlInput.Text};
            UrlList.Items.Add(newUrl);
        }

        private class UrlItem
        {
            public string Text { get; set; }

            public override string ToString()
            {
                return Text;
            }
        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            UrlList.Items.Clear();
        }

        public bool _startBtnStatus;
        public bool _stopBtnStatus;

        public bool StartBtnStatus
        {
            get => _startBtnStatus;
            set
            {
                _startBtnStatus = value;
                OnPropertyChanged(nameof(StartBtnStatus));
            }
        }

        public bool StopBtnStatus
        {
            get => _stopBtnStatus;
            set
            {
                _stopBtnStatus = value;
                OnPropertyChanged(nameof(StopBtnStatus));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
