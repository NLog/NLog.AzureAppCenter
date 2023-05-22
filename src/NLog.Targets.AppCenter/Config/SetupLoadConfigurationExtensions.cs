using NLog.Config;
using NLog.Layouts;
using NLog.Targets;

namespace NLog
{
    /// <summary>
    /// Extension methods to setup NLog <see cref="LoggingConfiguration"/>
    /// </summary>
    public static class SetupLoadConfigurationExtensions
    {
        /// <summary>
        /// Write to AppCenter NLog Target
        /// </summary>
        /// <param name="configBuilder"></param>
        /// <param name="layout">Override the default Layout for output</param>
        /// <param name="appSecret">appsecret for starting AppCenter if needed</param>
        /// <param name="reportExceptionAsCrash">activate AppCenter-Crashes and report Exceptions as crashes</param>
        public static ISetupConfigurationTargetBuilder WriteToAppCenter(this ISetupConfigurationTargetBuilder configBuilder, Layout layout = null, Layout appSecret = null, bool reportExceptionAsCrash = false)
        {
            var logTarget = new AppCenterTarget();
            if (layout != null)
                logTarget.Layout = layout;
            if (appSecret != null)
                logTarget.AppSecret = appSecret;
            if (reportExceptionAsCrash)
                logTarget.ReportExceptionAsCrash = reportExceptionAsCrash;
            return configBuilder.WriteTo(logTarget);
        }
    }
}
