using System.Runtime.CompilerServices;
using System.Security;
using NLog.Targets.AppCenter;

[assembly: Preserve]    // Automatic --linkskip=NLog.Targets.AppCenter
[assembly: AllowPartiallyTrustedCallers]
[assembly: SecurityRules(SecurityRuleSet.Level1)]
[assembly: InternalsVisibleTo("NLog.Targets.AppCenter.Tests")]