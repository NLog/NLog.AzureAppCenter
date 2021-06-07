// 
// Copyright (c) 2004-2019 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
// 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions 
// are met:
// 
// * Redistributions of source code must retain the above copyright notice, 
//   this list of conditions and the following disclaimer. 
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution. 
// 
// * Neither the name of Jaroslaw Kowalski nor the names of its 
//   contributors may be used to endorse or promote products derived from this
//   software without specific prior written permission. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
// THE POSSIBILITY OF SUCH DAMAGE.
// 

using System;
using System.Collections.Generic;
using NLog.Common;
using NLog.Layouts;

namespace NLog.Targets
{
    /// <summary>
    /// NLog Target for Microsoft AppCenter 
    /// </summary>
    [Target("AppCenter")]
    public class AppCenterTarget : TargetWithContext
    {
        /// <summary>
        /// Get or set the appsecret for starting AppCenter if needed (optional)
        /// </summary>
        public Layout AppSecret { get; set; }

        /// <summary>
        /// Get or set the application UserId to register in AppCenter (optional)
        /// </summary>
        public Layout UserId { get; set; }

        /// <summary>
        /// Get or set whether to activate AppCenter-Crashes and report Exceptions as crashes
        /// </summary>
        public bool ReportExceptionAsCrash { get; set; }

        /// <summary>
        /// Get or set the path to a directory to zip and attach to AppCenter-Crashes
        /// No more than 10 files will be attached
        /// Files bigger than 1mb after (compressed) will be ignored
        /// </summary>
        public Layout PathToCrashAttachmentDirectory { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppCenterTarget" /> class.
        /// </summary>
        public AppCenterTarget()
        {
            Layout = "${message}";          // MaxEventNameLength = 256 (Automatically truncated by Analytics)
            IncludeEventProperties = true;  // maximum item count = 20 (Automatically truncated by Analytics)
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppCenterTarget" /> class.
        /// </summary>
        public AppCenterTarget(string name)
            : this()
        {
            Name = name;
        }

        /// <inheritdoc />
        protected override void InitializeTarget()
        {
            base.InitializeTarget();

            try
            {
                if (!Microsoft.AppCenter.AppCenter.Configured)
                {
                    var appSecret = RenderLogEvent(AppSecret, LogEventInfo.CreateNullEvent());
                    if (!string.IsNullOrEmpty(appSecret))
                    {
                        InternalLogger.Debug("AppCenter(Name={0}): Starting AppCenter", Name);
                        var types = ReportExceptionAsCrash ? new[] { typeof(Microsoft.AppCenter.Analytics.Analytics), typeof(Microsoft.AppCenter.Crashes.Crashes) } : new[] { typeof(Microsoft.AppCenter.Analytics.Analytics) };
                        Microsoft.AppCenter.AppCenter.Start(appSecret, types);
                    }
                }
            }
            catch (Exception ex)
            {
                InternalLogger.Error(ex, "AppCenter(Name={0}): Failed to start AppCenter", Name);
                throw;
            }

            try
            {
                if (!Microsoft.AppCenter.Analytics.Analytics.IsEnabledAsync().ConfigureAwait(false).GetAwaiter().GetResult())
                {
                    Microsoft.AppCenter.Analytics.Analytics.SetEnabledAsync(true).ConfigureAwait(false).GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                InternalLogger.Error(ex, "AppCenter(Name={0}): Failed to enable AppCenter.Analytics", Name);
                throw;
            }

            try
            {
                if (ReportExceptionAsCrash && !Microsoft.AppCenter.Crashes.Crashes.IsEnabledAsync().ConfigureAwait(false).GetAwaiter().GetResult())
                {
                    Microsoft.AppCenter.Crashes.Crashes.SetEnabledAsync(true).ConfigureAwait(false).GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                InternalLogger.Error(ex, "AppCenter(Name={0}): Failed to enable AppCenter.Crashes", Name);
                throw;
            }

            var userId = RenderLogEvent(UserId, LogEventInfo.CreateNullEvent());
            if (!string.IsNullOrEmpty(userId))
            {
                Microsoft.AppCenter.AppCenter.SetUserId(userId);
            }
        }

        /// <inheritdoc />
        protected override void Write(LogEventInfo logEvent)
        {
            var eventName = RenderLogEvent(Layout, logEvent);
            var properties = BuildProperties(logEvent);
            var attachmentLogs = BuildAttachmentLogs(logEvent);
            TrackEvent(eventName, logEvent.Exception, properties, attachmentLogs);
        }

        /// <remarks>
        ///     The name parameter can not be null or empty, Maximum allowed length = 256.
        ///     The properties parameter maximum item count = 20.
        ///     The properties keys/names can not be null or empty, maximum allowed key length = 125.
        ///     The properties values can not be null, maximum allowed value length = 125.
        /// </remarks>
        private IDictionary<string, string> BuildProperties(LogEventInfo logEvent)
        {
            if (ShouldIncludeProperties(logEvent))
            {
                var properties = GetAllProperties(logEvent);
                return properties.Count > 0 ? new StringDictionary(properties) : null;
            }
            else if (ContextProperties?.Count > 0)
            {
                Dictionary<string, string> properties = new Dictionary<string, string>(ContextProperties.Count, StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < ContextProperties.Count; ++i)
                {
                    var contextProperty = ContextProperties[i];
                    if (string.IsNullOrEmpty(contextProperty.Name))
                        continue;

                    var contextValue = RenderLogEvent(contextProperty.Layout, logEvent);
                    if (!contextProperty.IncludeEmptyValue && string.IsNullOrEmpty(contextValue))
                        continue;

                    properties[contextProperty.Name] = contextValue;
                }
                return properties.Count > 0 ? properties : null;
            }

            return null;
        }

        private void TrackEvent(string eventName, Exception exception, IDictionary<string, string> properties = null, Microsoft.AppCenter.Crashes.ErrorAttachmentLog[] attachmentLogs = null)
        {
            if (string.IsNullOrWhiteSpace(eventName))
            {
                // Avoid event being discarded when name is null or empty
                if (exception != null)
                    eventName = exception.GetType().ToString();
                else if (properties?.Count > 0)
                    eventName = nameof(AppCenterTarget);
            }

            if (ReportExceptionAsCrash && exception != null)
            {
                properties = properties ?? new Dictionary<string, string>(1);
                if (properties.Count < 20)
                    properties["EventName"] = eventName;

                Microsoft.AppCenter.Crashes.Crashes.TrackError(exception, properties, attachmentLogs);
            }

            Microsoft.AppCenter.Analytics.Analytics.TrackEvent(eventName, properties);
        }

        /// <remarks>
        ///     The maximun file size (not compressed) is 10mb
        ///     The maximum attached file (compressed) size is 1mb.
        ///     The maximum attached file count is 10.
        /// </remarks>
        private Microsoft.AppCenter.Crashes.ErrorAttachmentLog[] BuildAttachmentLogs(LogEventInfo logEvent)
        {
            var path = RenderLogEvent(PathToCrashAttachmentDirectory, LogEventInfo.CreateNullEvent());
            if (!string.IsNullOrEmpty(path))
            {
                var errorAttachements = new List<Microsoft.AppCenter.Crashes.ErrorAttachmentLog>();
                try
                {
                    var directoryToCompress = new System.IO.DirectoryInfo(path);
                    if (directoryToCompress.Exists)
                    {
                        foreach (System.IO.FileInfo fileToCompress in directoryToCompress.GetFiles())
                        {
                            // 10mb file size limit
                            if (fileToCompress.Length > 1024 * 1024 * 10)
                                continue;

                            if (errorAttachements.Count >= 10)
                                break;

                            if ((System.IO.File.GetAttributes(fileToCompress.FullName) & System.IO.FileAttributes.Hidden) != System.IO.FileAttributes.Hidden)
                            {
                                using (System.IO.FileStream originalFileStream = fileToCompress.OpenRead())
                                {
                                    using (System.IO.MemoryStream compressedFileStream = new System.IO.MemoryStream())
                                    {
                                        using (System.IO.Compression.GZipStream compressionStream = new System.IO.Compression.GZipStream(compressedFileStream, System.IO.Compression.CompressionMode.Compress))
                                        {
                                            originalFileStream.CopyTo(compressionStream);

                                            // 1mb compressed file size limit
                                            if (compressedFileStream.Length > 1024 * 1024)
                                                continue;

                                            var errorAttachement = Microsoft.AppCenter.Crashes.ErrorAttachmentLog.AttachmentWithBinary(
                                                                        compressedFileStream.ToArray(),
                                                                        fileToCompress.Name + ".gz",
                                                                        "application/x-zip-compressed");
                                            errorAttachements.Add(errorAttachement);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    InternalLogger.Error(ex, "BuildAttachmentLogs(Path={0}): Failed to attach directory content", path);
                    throw;
                }
                return errorAttachements.ToArray();
            }
            else
                return Array.Empty<Microsoft.AppCenter.Crashes.ErrorAttachmentLog>();
        }
    }
}
