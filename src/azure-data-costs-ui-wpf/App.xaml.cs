using DbMeta.Ui.Wpf.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using NLog.Extensions.Logging;
using NLog;

namespace DataEstateOverview
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Config Config;
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            LogUnhandledException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");

            DispatcherUnhandledException += (s, e) =>
            {
                LogUnhandledException(e.Exception, "Application.Current.DispatcherUnhandledException");
                e.Handled = true;
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                LogUnhandledException(e.Exception, "TaskScheduler.UnobservedTaskException");
                e.SetObserved();
            };

            Config = new ConfigurationBuilder()
           .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
           .AddJsonFile("appsettings.json")
           .Build()
           .Get<Config>();


   //         var config = new ConfigurationBuilder()
   //.SetBasePath(System.IO.Directory.GetCurrentDirectory())
   //.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
   //.Build();

            //LogManager.Configuration = new NLogLoggingConfiguration(config.GetSection("NLog"));

            Debug.WriteLine("Nlog Setup");

            //NLog.Common.InternalLogger.LogToConsole = true;

            // enable internal logging to a file
            NLog.Common.InternalLogger.LogFile = "c:\\nlog-internal.txt"; // On Linux one can use "/home/nlog-internal.txt"

            // enable internal logging to a custom TextWriter
            //NLog.Common.InternalLogger.LogWriter = new StringWriter(); //e.g. TextWriter writer = File.CreateText("C:\\perl.txt")

            // set internal log level
            NLog.Common.InternalLogger.LogLevel = NLog.LogLevel.Trace;

        }

        private void ConfigureServices(IServiceCollection services)
        {
            //services.AddSingleton(Configuration);
        }

        private void LogUnhandledException(Exception exception, string source)
        {
            string message = $"Unhandled exception ({source})";
            try
            {
                System.Reflection.AssemblyName assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName();
                message = string.Format("Unhandled exception in {0} v{1}", assemblyName.Name, assemblyName.Version);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception in LogUnhandledException");
            }
            finally
            {
                _logger.Error(exception, message);
            }
        }
        public static void SaveConfig()
        {
            try
            {
                var jsonWriteOptions = new JsonSerializerOptions()
                {
                    WriteIndented = true
                };
                jsonWriteOptions.Converters.Add(new JsonStringEnumConverter());

                var newJson = JsonSerializer.Serialize(Config, jsonWriteOptions);

                var appSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
                File.WriteAllText(appSettingsPath, newJson);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}
