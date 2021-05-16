using MiuiEULocalization.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MiuiEULocalization.Processors
{
    public class SmaliProcessor
    {
        public string SmaliFilePath { get; private set; }

        public SmaliProcessor(string smaliFilePath)
        {
            SmaliFilePath = smaliFilePath;
        }

        public bool PatchMethod(string methodToSet)
        {
            if (!File.Exists(SmaliFilePath))
            {
                ConsoleWrapper.WriteInfo("PM", false, $"Smali file {SmaliFilePath} is not found.");
                return false;
            }

            string methodName = string.Empty;
            bool overwriting = false;
            string overvalue = "1";

            bool isMethodEverFound = false;

            string[] smaliFileContent = File.ReadAllLines(SmaliFilePath);
            List<string> newSmaliFileContent = new List<string>(smaliFileContent.Length);

            for (int i = 0; i < smaliFileContent.Length; i++)
            {
                Match match = Regex.Match(smaliFileContent[i], @"\.method\s+(?:(?:public|private)\s+)?(?:static\s+)?([^\(]+)\(");
                if (match.Success)
                {
                    methodName = match.Groups[1].Value;
                    if (methodToSet == methodName)
                    {
                        isMethodEverFound = true;
                        overwriting = true;
                        overvalue = "1";
                    }
                    else if ("-" + methodToSet == methodName)
                    {
                        isMethodEverFound = true;
                        overwriting = true;
                        overvalue = "0";
                    }
                    else if ("--" + methodToSet == methodName)
                    {
                        isMethodEverFound = true;
                        overwriting = true;
                        overvalue = "-1";
                    }

                    newSmaliFileContent.Add(smaliFileContent[i]);
                }
                else if (smaliFileContent[i].Contains(".end method"))
                {
                    if (overwriting)
                    {
                        overwriting = false;
                        if (overvalue == "-1")
                        {
                            newSmaliFileContent.Add(string.Join(Environment.NewLine, ".locals 0", "return-void", smaliFileContent[i]));
                            ConsoleWrapper.WriteInfo("PM", true, $"Patched method: {methodName} => void");
                        }
                        else if (overvalue == "0")
                        {
                            newSmaliFileContent.Add(string.Join(Environment.NewLine, ".locals 1", "const/4 v0, 0", "return v0", smaliFileContent[i]));
                            ConsoleWrapper.WriteInfo("PM", true, $"Patched method: {methodName} => false");
                        }
                        else if (overvalue == "1")
                        {
                            newSmaliFileContent.Add(string.Join(Environment.NewLine, ".locals 1", "const/4 v0, 1", "return v0", smaliFileContent[i]));
                            ConsoleWrapper.WriteInfo("PM", true, $"Patched method: {methodName} => true");
                        }
                    }
                    else
                    {
                        newSmaliFileContent.Add(smaliFileContent[i]);
                    }
                }
                else if (!overwriting)
                {
                    newSmaliFileContent.Add(smaliFileContent[i]);
                }
            }

            if (isMethodEverFound)
            {
                ConsoleWrapper.WriteInfo("PM", true, $"Method {methodToSet} in {SmaliFilePath} is updated.");
            }
            else
            {
                ConsoleWrapper.WriteInfo("PM", false, $"Method {methodToSet} in {SmaliFilePath} is not updated.");
            }

            File.WriteAllLines(SmaliFilePath, newSmaliFileContent);
            return isMethodEverFound;
        }

        public bool RemoveLine(string symbol)
        {
            if (!File.Exists(SmaliFilePath))
            {
                ConsoleWrapper.WriteInfo("RL", false, $"Smali file {SmaliFilePath} is not found.");
                return false;
            }

            string[] fileContentByLines = File.ReadAllLines(SmaliFilePath);
            if (fileContentByLines.Any(x => x.Contains(symbol)))
            {
                File.WriteAllLines(SmaliFilePath, fileContentByLines.Where(x => !x.Contains(symbol)));
                ConsoleWrapper.WriteInfo("RL", true, $"Removed line {symbol} from {SmaliFilePath}");
                return true;
            }
            else
            {
                ConsoleWrapper.WriteInfo("RL", false, $"Not removed line {symbol} from {SmaliFilePath}");
                return false;
            }
        }

        public bool UpdateDeviceMod(bool suppressMessage = false)
        {
            if (!File.Exists(SmaliFilePath))
            {
                ConsoleWrapper.WriteInfo("UDM", false, $"Smali file {SmaliFilePath} is not found.");
                return false;
            }

            string fileContent = File.ReadAllText(SmaliFilePath);

            if (fileContent.Contains("\"_global\""))
            {
                File.WriteAllText(SmaliFilePath, fileContent.Replace("\"_global\"", "\"_NON_EXISTING_DEVICE_MOD\""));
                ConsoleWrapper.WriteInfo("RL", true, $"Patched smali (device_mod): {SmaliFilePath}");
                return true;
            }
            else
            {
                if (!suppressMessage)
                {
                    ConsoleWrapper.WriteInfo("RL", false, $"Not patched smali (device_mod): {SmaliFilePath}");
                }
                return false;
            }
        }

        public bool UpdateDeviceRegion(bool suppressMessage = false)
        {
            if (!File.Exists(SmaliFilePath))
            {
                ConsoleWrapper.WriteInfo("UDR", false, $"Smali file {SmaliFilePath} is not found.");
                return false;
            }

            string fileContent = File.ReadAllText(SmaliFilePath);

            if (fileContent.Contains("\"CN\"") && fileContent.Contains("\"GB\""))
            {
                File.WriteAllText(SmaliFilePath, fileContent.Replace("\"GB\"", "\"ZZ\"").Replace("\"CN\"", "\"GB\""));
                ConsoleWrapper.WriteInfo("UDR", true, $"Patched smali (device_region): {SmaliFilePath}");
                return true;
            }
            else
            {
                if (!suppressMessage)
                {
                    ConsoleWrapper.WriteInfo("UDR", false, $"Not patched smali (device_region): {SmaliFilePath}");
                }
                return false;
            }
        }

        public bool UpdateInternationFlag(bool suppressMessage = false)
        {
            if (!File.Exists(SmaliFilePath))
            {
                ConsoleWrapper.WriteInfo("UIF", false, $"Smali file {SmaliFilePath} is not found.");
                return false;
            }

            string[] fileContentByLines = File.ReadAllLines(SmaliFilePath);
            bool updated = false;

            for (int i = 0; i < fileContentByLines.Length; i++)
            {
                var line = fileContentByLines[i];
                if (line.Contains("sget-boolean") && line.Contains("/Build;->IS_INTERNATIONAL_BUILD"))
                {
                    Match m = Regex.Match(line, @"sget-boolean ([a-z]\d+), L([a-zA-Z/]+)/Build;->IS_INTERNATIONAL_BUILD:Z");
                    fileContentByLines[i] = $"const/4 {m.Groups[1].Value}, 0x0";
                    updated = true;
                }
            }

            if (updated)
            {
                File.WriteAllLines(SmaliFilePath, fileContentByLines);
                ConsoleWrapper.WriteInfo("UIF", true, $"Patched smali (i18n): {SmaliFilePath}");
                return true;
            }
            else
            {
                if (!suppressMessage)
                {
                    ConsoleWrapper.WriteInfo("UIF", false, $"Not patched smali (i18n): {SmaliFilePath}");
                }
                return false;
            }
        }

        public bool UpdateGlobalFlag(bool suppressMessage = false)
        {
            if (!File.Exists(SmaliFilePath))
            {
                ConsoleWrapper.WriteInfo("UGF", false, $"Smali file {SmaliFilePath} is not found.");
                return false;
            }

            string[] fileContentByLines = File.ReadAllLines(SmaliFilePath);
            bool updated = false;

            for (int i = 0; i < fileContentByLines.Length; i++)
            {
                var line = fileContentByLines[i];
                if (line.Contains("sget-boolean") && line.Contains("/Build;->IS_GLOBAL_BUILD"))
                {
                    Match m = Regex.Match(line, @"sget-boolean ([a-z]\d+), L([a-zA-Z/]+)/Build;->IS_GLOBAL_BUILD:Z");
                    fileContentByLines[i] = $"const/4 {m.Groups[1].Value}, 0x0";
                    updated = true;
                }
            }

            if (updated)
            {
                File.WriteAllLines(SmaliFilePath, fileContentByLines);
                ConsoleWrapper.WriteInfo("UIF", true, $"Patched smali (global): {SmaliFilePath}");
                return true;
            }
            else
            {
                if (!suppressMessage)
                {
                    ConsoleWrapper.WriteInfo("UIF", false, $"Not patched smali (global): {SmaliFilePath}");
                }
                return false;
            }
        }
    }
}
