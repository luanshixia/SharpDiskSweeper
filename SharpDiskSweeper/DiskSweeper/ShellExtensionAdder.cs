using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiskSweeper
{
    public class ShellExtensionAdder
    {
        //private readonly Dictionary<string, string> RegValuesToAdd = new Dictionary<string, string>
        //{
        //    { @"HKEY_CLASSES_ROOT\Directory\shell\SharpDiskSweeper", "@=Open with SharpDiskSweeper&Icon=" },
        //    { @"HKEY_CLASSES_ROOT\Directory\shell\SharpDiskSweeper\command", "@=\"\" \"%1\"" }
        //};

        //public static Dictionary<string, string> GetRegValuesToAdd(string shellObject, string appName, string caption, string command, string icon) => new Dictionary<string, string>
        //{
        //    { $@"HKEY_CLASSES_ROOT\{shellObject}\shell\{appName}", $"@={caption}&Icon={icon}" },
        //    { $@"HKEY_CLASSES_ROOT\{shellObject}\shell\{appName}\command", $"@={command}" }
        //};

        public static (string, string, string, string)[] GetRegValuesToAdd(string shellObject, string appName, string caption, string command, string icon = null) => new[]
        {
            ($@"HKEY_CLASSES_ROOT\{shellObject}\shell\{appName}", "", caption, "REG_EXPAND_SZ"),
            ($@"HKEY_CLASSES_ROOT\{shellObject}\shell\{appName}", "Icon", icon, "REG_EXPAND_SZ"),
            ($@"HKEY_CLASSES_ROOT\{shellObject}\shell\{appName}\command", "", command, "REG_EXPAND_SZ")
        }.Where(entry => entry.Item3 != null).ToArray();

        public static void AddRegValues((string, string, string, string)[] regValues)
        {
            foreach (var regValue in regValues)
            {
                AddReg(regValue.Item1, regValue.Item2, regValue.Item3, regValue.Item4);
            }
        }

        public static void AddReg(string key, string valueName, string data, string dataType = "REG_SZ", string separator = null, bool force = true)
        {
            Process.Start("reg", $"add {key}" 
                + (!string.IsNullOrWhiteSpace(valueName) ? $" /v {valueName}" : "") 
                + $" /t {dataType}" 
                + (separator != null ? $" /s {separator}" : "") 
                + $" /d {data}" 
                + (force ? " /f" : ""));
        }
    }
}
