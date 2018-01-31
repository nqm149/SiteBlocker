using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
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
            Model.AddLogLine(Const.READ_HOSTS_FILE_MSG);
            
            // add default sites
            foreach (var site in defaultSites)
                Model.Input.Add(new InputItem(site));

            // add detected browsers
            DetectBrowsers().ForEach(b => Model.BrowserList.Add(b));
            if (Model.BrowserList.Count == 0)
            {
                Model.BrowserList.Add(new BrowserItem("No browser was detected. Please scan again.", false));
            }
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
                    var okToCloseBrowserList = OKToCloseBrowserList();
                    CloseBrowsers(okToCloseBrowserList);

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

        private void CloseBrowsers(List<BrowserItem> browserItems)
        {
            if (browserItems == null)
                throw new ArgumentNullException($"No browser in detected browsers List");

            foreach (var browser in browserItems)
            {
                //first, close all browsers
                while (HasProcess(browser.BrowserName))
                {
                    processes = GetAllProcessesByName(browser.BrowserName);

                    foreach (var p in processes)
                    {
                        //                        p.CloseMainWindow();
                        //                        p.Close();
                        try
                        {
                            p.Kill();
                            p.WaitForExit();
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine(e);
                            // ignore this exception
                        }
                        
                    }

                    Model.AddLogLine($"{Const.FOUND_PROCESS_LOG_MSG} {browser.BrowserName}");
                    Debug.WriteLine($"{Const.FOUND_PROCESS_LOG_MSG} {browser.BrowserName}");
                }
            }
        }

        private List<BrowserItem> DetectBrowsers()
        {
            var detectedBrowsers = new List<BrowserItem>();
            foreach (var browser in browsers)
            {
                //first, close all browsers
                if (HasProcess(browser))
                {
                    var newDectectedBrowser = new BrowserItem(browser, true);
                    if (detectedBrowsers.Contains(newDectectedBrowser) == false)
                    {
                        detectedBrowsers.Add(newDectectedBrowser);
                        Model.AddLogLine($"{browser} has has been detected.");
                    }
                }
            }

            return detectedBrowsers;
        }

        private List<BrowserItem> OKToCloseBrowserList() =>
            Model.BrowserList.Where(b => b.BrowserCheckbox == true).ToList();

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            AddUrl();
        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            Model.NewUri = string.Empty;
        }

        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            RestoreHostsFile();
            MessageBox.Show(Const.OK_TO_ACCESS_MSG);

            _timer?.Stop();

            TimeLeft.Text = "00:00:00";
            TimeLeft.Foreground = Brushes.Black;

            Model.StartBtnStatus = true;
            Model.StopBtnStatus = false;
        }

        private bool HasProcess(string processName) => Process.GetProcesses().Any(p => p.ProcessName == processName);

        private Process[] GetAllProcessesByName(string processName) => Process.GetProcessesByName(processName);

        private void RestoreHostsFile()
        {
            using (var sw = new StreamWriter(Const.HostsPath, false))
                sw.WriteAsync(_originalContent);
            Model.AddLogLine(Const.RESTORE_HOSTS_FILE_MSG);
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

                MessageBox.Show($"Sites will be blocked for {TimeSet.Text} seconds ! Click \"Stop\" to access again. ");
                Model.AddLogLine($"Sites will be blocked for {TimeSet.Text} seconds ! Click \"Stop\" to access again. ");
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
                    TimeLeft.Foreground = Brushes.Black;
                    using (var sw = new StreamWriter(Const.HostsPath, false))
                    {
                        sw.WriteAsync(_originalContent);
                    }

                    Model.StartBtnStatus = true;
                    Model.StopBtnStatus = false;

                    MessageBox.Show(Const.OK_TO_ACCESS_MSG);
                    Model.AddLogLine(Const.OK_TO_ACCESS_MSG); 
                }
                time = time.Add(TimeSpan.FromSeconds(-1));
            }, Application.Current.Dispatcher);

            _timer.Start();
            TimeLeft.Foreground = Brushes.Red;
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

        private void TextBlock_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            LogScrollViewer.ScrollToBottom();
        }

        private void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            Model.BrowserList.Clear();
            DetectBrowsers().ForEach(b => Model.BrowserList.Add(b));
            if (Model.BrowserList.Count == 0)
                Model.BrowserList.Add(new BrowserItem("No browser was detected. Please scan again.", false));
        }
    }

}
