using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace MiuiEULocalization.Utilities
{
    public class Smali
    {
        public string SmaliPath { get; private set; }

        public Smali(string folder = null)
        {
            if (!string.IsNullOrEmpty(folder) && Directory.Exists(folder))
            {
                SmaliPath = Directory.EnumerateFiles(folder).FirstOrDefault(x => Path.GetFileName(x).StartsWith("smali") && Path.GetFileName(x).EndsWith(".jar"));
            }
            if (SmaliPath == null)
            {
                throw new FileNotFoundException("Unable to find smali JAR file.");
            }
        }

        public void AssembleFrom(string smaliPath, string api)
        {
            var process = Process.Start(new ProcessStartInfo()
            {
                FileName = "java",
                Arguments = string.Join(" ", new string[] {
                    "-jar",
                    SmaliPath,
                    "a",
                    smaliPath,
                    "-a",
                    api,
                    "-o",
                    Path.Combine(Path.GetDirectoryName(smaliPath), $"{Path.GetFileNameWithoutExtension(smaliPath).Replace("_smali", "_x.dex")}")
                })
            });
            process.WaitForExit();
            ConsoleWrapper.WriteInfo("DEX", true, $"assembled from SMALI {smaliPath}");
        }
    }
}
