using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;
using SiteBlocker.Constant;
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
        private static readonly List<string> browsers = Const.DefaultBrowsers;
        private static readonly List<string> defaultSites = Const.DefaultSites;

        private const string DefaultHost = Const.DefaultHost;
        private readonly string _originalContent;
        DispatcherTimer _timer;
        private Process[] processes;

        private Model Model => DataContext as Model;

        public MainWindow()
        {
            InitializeComponent();
            using (var sr = new StreamReader(Const.HostsPath))
                _originalContent = sr.ReadToEndAsync().Result;
            Debug.WriteLine(Const.READ_HOSTS_FILE_MSG);

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
            var modelNewUri = $"{Const.DefaultUrlPrefix}{Model.NewUri}".Trim();
            if (Uri.TryCreate(modelNewUri, UriKind.Absolute, out _))
            {
                Model.Input.Add(new InputItem(modelNewUri));
                Model.NewUri = string.Empty;
            }
            else
            {
                MessageBox.Show(Const.NOT_A_URL_ERROR_MSG);
            }
        }

        private void RemoveBtn_Click(object sender, RoutedEventArgs e)
        {
            var urlItem = ((FrameworkElement)sender).DataContext as InputItem;
            Model.Input.Remove(urlItem);
        }

        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ReadyToBlock())
            {

                var dialogResult = MessageBox.Show(Const.CLOSE_ALL_BROWSERS_MSG, "Warning", MessageBoxButton.OKCancel);
                if (dialogResult == MessageBoxResult.OK)
                {
                    foreach (var browser in browsers)
                    {
                        //first, close all browsers
                        while (StillHasProcess(browser))
                        {
                            processes = GetAllProcessesByName(browser);

                            foreach (var p in processes)
                            {
                                Debug.WriteLine($"{Const.FOUND_PROCESS_LOG_MSG} {p.ProcessName}");
                                p.CloseMainWindow();
                            }
                        }
                    }

                    Model.StartBtnStatus = false;
                    Model.StopBtnStatus = true;

                    BlockSite();

                }
                else if (dialogResult == MessageBoxResult.Cancel)
                {
                    //do nothing
                }
            }
        }
        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            AddUrl();
        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            Model.Input.Clear();
        }

        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            RestoreHostsFile();
            MessageBox.Show(Const.OK_TO_ACCESS_MSG);

            _timer?.Stop();

            TimeLeft.Text = "00:00:00";

            Model.StartBtnStatus = true;
        }

        private bool StillHasProcess(string processName) => Process.GetProcesses().Any(p => p.ProcessName == processName);

        private Process[] GetAllProcessesByName(string processName) => Process.GetProcessesByName(processName);

        private void RestoreHostsFile()
        {
            using (var sw = new StreamWriter(Const.HostsPath, false))
                sw.WriteAsync(_originalContent);
            Debug.WriteLine(Const.RESTORE_HOSTS_FILE_MSG);
        }

        private void BlockSite()
        {
            //then do the magic
            var time = TimeSpan.FromSeconds(int.Parse(TimeSet.Text));

            try
            {

                using (var sw = new StreamWriter(Const.HostsPath, true))
                {
                    foreach (var url in Model.Input)
                    {
                        sw.WriteAsync(Environment.NewLine + DefaultHost + url.Uri);
                    }
                }

                MessageBox.Show(Const.BLOCK_SITES_MSG);
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
                    using (var sw = new StreamWriter(Const.HostsPath, false))
                    {
                        sw.WriteAsync(_originalContent);
                    }

                    Model.StartBtnStatus = true;
                    Model.StopBtnStatus = false;

                    MessageBox.Show(Const.OK_TO_ACCESS_MSG);
                }
                time = time.Add(TimeSpan.FromSeconds(-1));
            }, Application.Current.Dispatcher);

            _timer.Start();

        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool ReadyToBlock()
        {
            if (Model.Input.Count == 0)
            {
                MessageBox.Show(Const.INPUT_URL_WARNING_MSG);
                return false;
            }

            var validateResult = ValidateTime(TimeSet.Text);
            if (validateResult.IsOK == false)
            {
                MessageBox.Show(validateResult.Message);
                return false;
            }

            return true;
        }

        private ValidateResult ValidateTime(string time)
        {
            try
            {
                int.TryParse(time, out var intTime);
                if (intTime > 0)
                    return new ValidateResult(true, "", intTime);
                else
                    return new ValidateResult(false, Const.INPUT_TIME_WARNING_MSG, 0);
            }
            catch (FormatException e)
            {
                Console.WriteLine(e);
                throw;
            }

        }
    }

}
