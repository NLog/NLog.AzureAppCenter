#if !__APPLE__ && !__ANDROID__

using System.Diagnostics;
using NLog.Layouts;

namespace NLog.Targets.Xamarin
{
    /// <summary>
    /// Output target for Android.Util.Log
    /// </summary>
    [Target("DebugXamarin")]
    public class XamarinDebuggerTarget : TargetWithLayoutHeaderAndFooter
    {
        /// <summary>
        /// The category of the message
        /// </summary>
        public Layout Category { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="XamarinDebuggerTarget"/> class.
        /// </summary>
        public XamarinDebuggerTarget()
        {
            Layout = "${level}|${message:withException=true:exceptionSeparator=|}";
            Category = "${logger}";
        }

        /// <inheritdoc/>
        protected override void InitializeTarget()
        {
            base.InitializeTarget();

            if (Header != null && Debugger.IsLogging())
            {
                var logEvent = LogEventInfo.CreateNullEvent();
                logEvent.Level = LogLevel.Info;
                logEvent.LoggerName = "Starting";
                DebugWriteLine(Header, logEvent);
            }
        }

        /// <inheritdoc/>
        protected override void CloseTarget()
        {
            if (Footer != null && Debugger.IsLogging())
            {
                var logEvent = LogEventInfo.CreateNullEvent();
                logEvent.Level = LogLevel.Info;
                logEvent.LoggerName = "Closing";
                DebugWriteLine(Footer, logEvent);
            }

            base.CloseTarget();
        }

        /// <inheritdoc/>
        protected override void Write(LogEventInfo logEvent)
        {
            if (Debugger.IsLogging())
            {
                DebugWriteLine(Layout, logEvent);
            }
        }

        private void DebugWriteLine(Layout layout, LogEventInfo logEvent)
        {
            var logMessage = RenderLogEvent(layout, logEvent) ?? string.Empty;
            var logCategory = RenderLogEvent(Category, logEvent);
            if (string.IsNullOrEmpty(logCategory))
                logCategory = null;

            Debugger.Log(logEvent.Level.Ordinal, logCategory, logMessage + System.Environment.NewLine);
        }
    }
}

#endif