using MiuiEULocalization.Processors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace MiuiEULocalization.Utilities
{
    public class Baksmali
    {
        public string BaksmaliPath { get; private set; }

        public Baksmali(string resourceFolder = null)
        {
            if (!string.IsNullOrEmpty(resourceFolder) && Directory.Exists(resourceFolder))
            {
                BaksmaliPath = Directory.EnumerateFiles(resourceFolder).FirstOrDefault(x => Path.GetFileName(x).StartsWith("baksmali") && Path.GetFileName(x).EndsWith(".jar"));
            }
            if (BaksmaliPath == null)
            {
                throw new FileNotFoundException("Unable to find baksmali JAR file.");
            }
        }

        public void DisassembleFrom(string dexPath)
        {
            var process = Process.Start(new ProcessStartInfo()
            {
                FileName = "java",
                Arguments = string.Join(" ", new string[] {
                    "-jar",
                    BaksmaliPath,
                    "d",
                    dexPath,
                    "--output",
                    Path.Combine(Path.GetDirectoryName(dexPath), $"{Path.GetFileNameWithoutExtension(dexPath)}_smali")
                })
            });
            process.WaitForExit();
            ConsoleWrapper.WriteInfo("DEX", true, $"disassembled from DEX {dexPath}");
        }

        public void Disassemble(ApkProcessor apkProcessor)
        {
            var process = Process.Start(new ProcessStartInfo()
            {
                FileName = "java",
                Arguments = string.Join(" ", new string[] {
                    "-jar",
                    BaksmaliPath,
                    "d",
                    apkProcessor.ApkPath,
                    "--output",
                    Path.Combine(apkProcessor.ApkPath, "smali")
                })
            });
            process.WaitForExit();
            ConsoleWrapper.WriteInfo("DEX", true, $"disassembled from APK {apkProcessor.ApkPath}");
        }
    }
}
