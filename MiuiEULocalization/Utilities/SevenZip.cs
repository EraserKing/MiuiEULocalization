using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace MiuiEULocalization.Utilities
{
    public static class SevenZip
    {
        public static void Extract(string archiveFile, string extractFile, string workingDirectory)
        {
            var process = Process.Start(new ProcessStartInfo()
            {
                FileName = Path.Combine("Resources", "7z"),
                Arguments = string.Join(" ", new string[]
                {
                    "x",
                    archiveFile,
                    extractFile,
                    $"-o{workingDirectory}",
                    "-aoa"
                }),
                RedirectStandardOutput = true
            });
            process.WaitForExit();
        }

        public static bool FolderExist(string archiveFile, string contentPath)
        {
            var process = Process.Start(new ProcessStartInfo()
            {
                FileName = Path.Combine("Resources", "7z"),
                Arguments = string.Join(" ", new string[]
                {
                    "l",
                    archiveFile,
                    contentPath
                }),
                RedirectStandardOutput = true
            });
            return process.StandardOutput.ReadToEnd().Contains("1 folders");
        }
    }
}
