# NLog.Targets.AppCenter
NLog Target for [Microsoft Visual Studio App Center with Azure](https://azure.microsoft.com/services/app-center/)

[![Version](https://badge.fury.io/nu/NLog.Targets.AppCenter.svg)](https://www.nuget.org/packages/NLog.Targets.AppCenter)
[![AppVeyor](https://img.shields.io/appveyor/ci/nlog/nlog-azureappcenter/master.svg)](https://ci.appveyor.com/project/nlog/nlog-azureappcenter/branch/master)

### How to setup NLog in MAUI

1) Install the NLog packages

   - `Install-Package NLog.Targets.AppCenter` 
   - `Install-Package NLog.Extensions.Logging` 
    
   or in your csproj:

    ```xml
    <PackageReference Include="NLog.Targets.AppCenter" Version="5.*" />
    <PackageReference Include="NLog.Extensions.Logging" Version="5.*" />
    ```

2) Add NLog to the MauiApp

   Update `MauiProgram.cs` to include NLog as Logging Provider: 
   ```csharp
   var builder = MauiApp.CreateBuilder();

   // Add NLog for Logging
   builder.Logging.ClearProviders();
   builder.Logging.AddNLog();
   ```

   If getting compiler errors with unknown methods, then update `using`-section:
   ```csharp
   using Microsoft.Extensions.Logging;
   using NLog;
   using NLog.Extensions.Logging;
   ```

3) Load NLog configuration for logging

   Add the `NLog.config` into the Application-project as assembly-resource (`Build Action` = `embedded resource`), and load like this:
   ```csharp
   NLog.LogManager.Setup().RegisterAppCenter().LoadConfigurationFromAssemblyResource(typeof(App).Assembly);
   ```
   Alternative setup NLog configuration using [fluent-API](https://github.com/NLog/NLog/wiki/Fluent-Configuration-API):
   ```csharp
   var logger = NLog.LogManager.Setup().RegisterAppCenter()
                    .LoadConfiguration(c => c.ForLogger(NLog.LogLevel.Debug).WriteToAppCenter())
                    .GetCurrentClassLogger();
   ```

### Configuration options for AppCenter NLog Target

- **AppSecret** - Appsecret for starting AppCenter if needed (optional)
- **UserId** - Application UserId to register in AppCenter (optional)
- **LogUrl** - Base URL (scheme + authority + port only) to the AppCenter-backend (optional)
- **CountryCode** - Two-letter ISO country code to send to the AppCenter-backend (optional)
- **ReportExceptionAsCrash** - Report all exceptions as crashes to AppCenter (default=false)
- **IncludeEventProperties** - Include LogEvent properties in AppCenter properties (default=true)
- **IncludeScopeProperties** - Include MappedDiagnosticsLogicalContext (MLDC) that can be provided with MEL BeginScope (default=false)

```xml
<nlog>
<extensions>
    <add assembly="NLog.Targets.AppCenter"/>
</extensions>
<targets>
    <target name="appcenter" xsi:type="appcenter" layout="${message}" reportExceptionAsCrash="true">
	<contextproperty name="logger" layout="${logger}" />
	<contextproperty name="loglevel" layout="${level}" />
	<contextproperty name="threadid" layout="${threadid}" />
    </target>
</targets>
<rules>
    <logger minLevel="Info" writeTo="appcenter" />
</rules>
</nlog>
```