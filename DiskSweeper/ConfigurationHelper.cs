using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DiskSweeper
{
    public static class ConfigurationHelper
    {
        public static string GetConfiguration(string settingName, string defaultValue = null)
        {
            return ConfigurationManager.AppSettings.AllKeys.Contains(settingName)
                ? ConfigurationManager.AppSettings[settingName]
                : defaultValue;
        }

        public static long GetConfigurationInt64(string settingName, long defaultValue)
        {
            return ConfigurationManager.AppSettings.AllKeys.Contains(settingName)
                && long.TryParse(ConfigurationManager.AppSettings[settingName], out long result)
                ? result
                : defaultValue;
        }

        public static int GetConfigurationInt32(string settingName, int defaultValue)
        {
            return ConfigurationManager.AppSettings.AllKeys.Contains(settingName)
                && int.TryParse(ConfigurationManager.AppSettings[settingName], out int result)
                ? result
                : defaultValue;
        }

        public static double GetConfigurationDouble(string settingName, double defaultValue)
        {
            return ConfigurationManager.AppSettings.AllKeys.Contains(settingName)
                && double.TryParse(ConfigurationManager.AppSettings[settingName], out double result)
                ? result
                : defaultValue;
        }

        public static TimeSpan GetConfigurationTimeSpan(string settingName, TimeSpan defaultValue)
        {
            return ConfigurationManager.AppSettings.AllKeys.Contains(settingName)
                && TimeSpan.TryParse(ConfigurationManager.AppSettings[settingName], out TimeSpan result)
                ? result
                : defaultValue;
        }

        public static bool GetConfigurationBoolean(string settingName, bool defaultValue)
        {
            return ConfigurationManager.AppSettings.AllKeys.Contains(settingName)
                && bool.TryParse(ConfigurationManager.AppSettings[settingName], out bool result)
                ? result
                : defaultValue;
        }

        public static Guid GetConfigurationGuid(string settingName, Guid defaultValue)
        {
            return ConfigurationManager.AppSettings.AllKeys.Contains(settingName)
                && Guid.TryParse(ConfigurationManager.AppSettings[settingName], out Guid result)
                ? result
                : defaultValue;
        }

        public static Color GetConfigurationColor(string settingName, Color defaultValue)
        {
            return ConfigurationManager.AppSettings.AllKeys.Contains(settingName)
                ? (Color)ColorConverter.ConvertFromString(ConfigurationManager.AppSettings[settingName])
                : defaultValue;
        }

        public static T GetConfigurationEnum<T>(string settingName, T defaultValue) where T: struct
        {
            return ConfigurationManager.AppSettings.AllKeys.Contains(settingName)
                && Enum.TryParse(ConfigurationManager.AppSettings[settingName], out T result)
                ? result
                : defaultValue;
        }
    }
}
