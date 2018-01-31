using System.Collections.Generic;

namespace SiteBlocker.Constant
{
    internal class Const
    {
        // Path
        public const string HostsPath = "C:\\Windows\\System32\\drivers\\etc\\hosts";
        public static readonly List<string> DefaultBrowsers = new List<string> { "chrome", "MicrosoftEdge", "firefox", "opera", "Safari" };
        public static readonly List<string> DefaultSites = new List<string> { "www.facebook.com", "www.youtube.com" };
        public const string DefaultHost = "127.0.0.1 ";
        public const string DefaultUrlPrefix = "http://";

        // Alert Message
        public const string READ_HOSTS_FILE_MSG = "Read hosts file's original content.";
        public const string RESTORE_HOSTS_FILE_MSG = "Restore hosts file's original content.";
        public const string BLOCK_SITES_MSG = "Sites blocked ! Click \"Stop\" to access again. ";
        public const string CLOSE_ALL_BROWSERS_MSG = "This will close your browsers and tabs to start blocking sites. !";
        public const string FOUND_PROCESS_LOG_MSG = "Found browser related process. Close it now : ";
        public const string OK_TO_ACCESS_MSG = "Now you can access all sites again !";
        public const string NOT_A_URL_ERROR_MSG = "Not a valid URL.";
        public const string INPUT_URL_WARNING_MSG = "Please enter an URL.";
        public const string INPUT_TIME_WARNING_MSG = "Please enter a valid number for blocking time.";
    }
}
