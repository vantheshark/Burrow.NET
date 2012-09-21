using System;

namespace Burrow
{
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
