using System;

namespace Burrow
{
    /// <summary>
    /// this interface borrows a subset of log4net.ILog, properly implement an adaptor for your favorite logigng library
    /// </summary>
    public interface IRabbitWatcher
    {
        bool IsDebugEnable { get; set; }
        void DebugFormat(string format, params object[] args);
        void InfoFormat(string format, params object[] args);
        void WarnFormat(string format, params object[] args);
        void ErrorFormat(string format, params object[] args);
        void Error(Exception exception);
    }
}
