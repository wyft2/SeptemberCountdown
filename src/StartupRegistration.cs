using System;
using System.Reflection;
using Microsoft.Win32;

namespace SeptemberCountdown
{
    internal static class StartupRegistration
    {
        private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string ValueName = "SeptemberCountdown";

        internal static bool SetEnabled(bool enabled)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true)
                    ?? Registry.CurrentUser.CreateSubKey(RunKeyPath))
                {
                    if (key == null)
                    {
                        return false;
                    }

                    if (enabled)
                    {
                        string executable = Assembly.GetExecutingAssembly().Location;
                        if (string.IsNullOrEmpty(executable))
                        {
                            return false;
                        }
                        key.SetValue(ValueName, "\"" + executable + "\"", RegistryValueKind.String);
                    }
                    else
                    {
                        if (key.GetValue(ValueName) != null)
                        {
                            key.DeleteValue(ValueName, false);
                        }
                    }
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        internal static string GetRegisteredCommand()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunKeyPath, false))
                {
                    if (key == null)
                    {
                        return null;
                    }
                    return key.GetValue(ValueName) as string;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
