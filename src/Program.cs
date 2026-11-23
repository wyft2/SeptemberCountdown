using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace SeptemberCountdown
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            try
            {
                AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

                bool createdNew;
                using (Mutex mutex = new Mutex(true, "SeptemberCountdown.SingleInstance", out createdNew))
                {
                    if (!createdNew)
                    {
                        if (!NativeMethods.ActivateExistingWindow())
                        {
                            MessageBox.Show("九月倒计时已经在运行。", "九月倒计时",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        return;
                    }

                    bool reset = false;
                    bool openSettings = false;
                    bool noAcrylic = false;
                    if (args != null)
                    {
                        for (int i = 0; i < args.Length; i++)
                        {
                            string arg = args[i].ToLowerInvariant();
                            if (arg == "--reset")
                            {
                                reset = true;
                            }
                            else if (arg == "--settings")
                            {
                                openSettings = true;
                            }
                            else if (arg == "--no-acrylic")
                            {
                                noAcrylic = true;
                            }
                        }
                    }

                    if (reset)
                    {
                        AppSettings.DeleteSaved();
                        StartupRegistration.SetEnabled(false);
                    }

                    try
                    {
                        System.Windows.Forms.Application.EnableVisualStyles();
                    }
                    catch
                    {
                    }

                    Application app = new Application();
                    app.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                    app.DispatcherUnhandledException += OnDispatcherUnhandledException;
                    MainWindow window = new MainWindow(openSettings, noAcrylic);
                    app.MainWindow = window;
                    window.Show();
                    app.Run();

                    GC.KeepAlive(mutex);
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
                MessageBox.Show("九月倒计时启动失败：\n\n" + ex.Message +
                    "\n\n错误日志已写入：\n" + CrashLogPath,
                    "九月倒计时", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static string CrashLogPath
        {
            get
            {
                try
                {
                    Directory.CreateDirectory(AppSettings.SettingsDirectory);
                    return Path.Combine(AppSettings.SettingsDirectory, "crash.log");
                }
                catch
                {
                    return Path.Combine(Path.GetTempPath(), "SeptemberCountdown-crash.log");
                }
            }
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            if (ex != null)
            {
                LogException(ex);
            }
        }

        private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            LogException(e.Exception);
            MessageBox.Show("九月倒计时遇到错误：\n\n" + e.Exception.Message +
                "\n\n错误日志已写入：\n" + CrashLogPath,
                "九月倒计时", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private static void LogException(Exception ex)
        {
            try
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                builder.AppendLine(ex.ToString());
                builder.AppendLine();
                File.AppendAllText(CrashLogPath, builder.ToString(), Encoding.UTF8);
            }
            catch
            {
            }
        }
    }
}
