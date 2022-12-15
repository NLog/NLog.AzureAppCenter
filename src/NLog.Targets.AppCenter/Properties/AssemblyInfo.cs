using System.Runtime.CompilerServices;
using System.Security;
using NLog.Targets.AppCenter;

[assembly: Preserve]    // Automatic --linkskip=NLog.Targets.AppCenter
[assembly: AllowPartiallyTrustedCallers]
[assembly: SecurityRules(SecurityRuleSet.Level1)]
[assembly: InternalsVisibleTo("NLog.Targets.AppCenter.Tests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100ef8eab4fbdeb511eeb475e1659fe53f00ec1c1340700f1aa347bf3438455d71993b28b1efbed44c8d97a989e0cb6f01bcb5e78f0b055d311546f63de0a969e04cf04450f43834db9f909e566545a67e42822036860075a1576e90e1c43d43e023a24c22a427f85592ae56cac26f13b7ec2625cbc01f9490d60f16cfbb1bc34d9")]