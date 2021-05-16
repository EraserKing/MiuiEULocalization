using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiuiEULocalization.Utilities
{
    class Aapt
    {
        public static void Remove(string apkFile, string fileToDelete, string workingDirectory)
        {
            var process = Process.Start(new ProcessStartInfo()
            {
                FileName = Path.Combine("Resources", "aapt"),
                Arguments = string.Join(" ", new string[]
                {
                    "r",
                    apkFile,
                    fileToDelete,
                }),
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true
            });
            process.WaitForExit();
        }

        public static void Add(string apkFile, string fileToAdd, string workingDirectory)
        {
            var process = Process.Start(new ProcessStartInfo()
            {
                FileName = Path.Combine("Resources", "aapt"),
                Arguments = string.Join(" ", new string[]
                {
                    "a",
                    apkFile,
                    fileToAdd,
                }),
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true
            });
            process.WaitForExit();
        }
    }
}
