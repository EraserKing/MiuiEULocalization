using MiuiEULocalization.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MiuiEULocalization.Processors
{
    public class ApkProcessor
    {
        public string ApkPath { get; set; }

        public string ApkContentPath => Path.Combine(Path.GetDirectoryName(ApkPath), "content");

        public string ApkDirectory => Path.GetDirectoryName(ApkPath);

        public string AppName => Path.GetFileNameWithoutExtension(ApkPath);

        public ApkProcessor(string apkPath)
        {
            ApkPath = apkPath;
        }

        public void Extract()
        {
            Utilities.SevenZip.Extract(ApkPath, "*", ApkContentPath);
        }

        public void Disassemble()
        {
            if (!Directory.Exists(ApkContentPath))
            {
                Extract();
            }

            Baksmali baksmali = new Baksmali(Settings.ResourceFolder);
            foreach (string dexFilePath in Directory.EnumerateFiles(ApkContentPath, "*.dex"))
            {
                baksmali.DisassembleFrom(dexFilePath);
            }
        }

        public void Assemble(string api)
        {
            if (!Directory.Exists(ApkContentPath))
            {
                throw new InvalidOperationException("Content not extracted yet");
            }

            Smali s = new Smali(Settings.ResourceFolder);
            foreach (string smaliPath in Directory.EnumerateDirectories(ApkContentPath, "*_smali"))
            {
                s.AssembleFrom(smaliPath, api);
            }
        }

        public SmaliProcessor CreateSmaliProcessor(string smaliFilePath, string dexName = "classes")
        {
            if (string.IsNullOrEmpty(dexName))
            {
                dexName = "classes";
            }
            return new SmaliProcessor(Path.Combine(Path.GetDirectoryName(ApkPath), "content", dexName + "_smali", smaliFilePath));
        }

        public IEnumerable<SmaliProcessor> CreateSmaliProcessors(string smaliFilePathBase = null, string dexName = "classes")
        {
            if (string.IsNullOrEmpty(dexName))
            {
                dexName = "classes";
            }

            if (smaliFilePathBase == null)
            {
                return Directory.EnumerateFiles(Path.Combine(Path.GetDirectoryName(ApkPath), "content", dexName + "_smali"), "*.smali", SearchOption.AllDirectories).Select(x => new SmaliProcessor(x));
            }
            else
            {
                return Directory.EnumerateFiles(Path.Combine(Path.GetDirectoryName(ApkPath), "content", dexName + "_smali", smaliFilePathBase), "*.smali", SearchOption.AllDirectories).Select(x => new SmaliProcessor(x));
            }

        }

        public bool MoveArch(string originalArchFolder, string newArchFolder)
        {
            if (!Directory.Exists(ApkContentPath))
            {
                Extract();
            }

            if (originalArchFolder == "_CURRENT_")
            {
                originalArchFolder = Settings.Arch switch
                {
                    "arm64" => "arm64-v8a",
                    "arm" => "armeabi-v7a",
                    "x86_64" => "x86_64",
                    "x86" => "x86",
                    _ => throw new ArgumentOutOfRangeException(Settings.Arch)
                };
            }

            if (Directory.Exists(Path.Combine(ApkContentPath, "lib", originalArchFolder)))
            {
                Directory.Move(Path.Combine(ApkContentPath, "lib", originalArchFolder), Path.Combine(ApkContentPath, "lib", newArchFolder));
                ConsoleWrapper.WriteInfo("APK", true, $"Renamed arch from {originalArchFolder} to {newArchFolder}");
                return true;
            }
            else
            {
                ConsoleWrapper.WriteInfo("APK", false, $"Unable to rename arch from {originalArchFolder} to {newArchFolder}");
                return false;
            }
        }

        public bool ExtractLib()
        {
            string sourceLibPath = Path.Combine(ApkContentPath, "lib");
            if (Directory.Exists(sourceLibPath))
            {
                foreach (var sourceDirectoryPath in Directory.GetDirectories(sourceLibPath))
                {
                    string sourceLibName = Path.GetFileName(sourceDirectoryPath);
                    if (sourceLibName == "arm64-v8a")
                    {
                        ConsoleWrapper.WriteInfo("APK", true, "Extracting arch arm64-v8a");
                        MoveFilesInDirectoryIntoDirectory(sourceDirectoryPath, Path.Combine(ApkContentPath, "lib", "arm64"));
                    }
                    else if (sourceLibName == "armeabi-v7a")
                    {
                        ConsoleWrapper.WriteInfo("APK", true, "Extracting arch armeabi-v7a");
                        MoveFilesInDirectoryIntoDirectory(sourceDirectoryPath, Path.Combine(ApkContentPath, "lib", "arm"));
                    }
                    else
                    {
                        ConsoleWrapper.WriteInfo("APK", true, $"Extracting arch {sourceLibName}");
                        MoveFilesInDirectoryIntoDirectory(sourceDirectoryPath, Path.Combine(ApkContentPath, "lib", sourceLibName));
                    }
                }
            }
            return true;
        }

        private void MoveFilesInDirectoryIntoDirectory(string sourceDirectory, string targetDirectory)
        {
            Directory.CreateDirectory(targetDirectory);

            foreach (string filePath in Directory.GetFiles(sourceDirectory))
            {
                File.Move(filePath, Path.Combine(targetDirectory, Path.GetFileName(filePath)));
            }
        }

        public bool AddDexClasses()
        {
            bool isEverProcessed = false;

            foreach (var dexFile in Directory.GetFiles(ApkDirectory, "*.dex"))
            {
                File.Delete(dexFile);
            }

            if (Directory.GetFiles(ApkContentPath, "*_x.dex").Length > 0) // Not cleaned up
            {
                foreach (var dexFile in Directory.GetFiles(ApkContentPath, "*_x.dex"))
                {
                    File.Copy(dexFile, Path.Combine(ApkDirectory, Path.GetFileName(dexFile).Replace("_x.dex", ".dex")));
                }
            }

            else // Cleaned up
            {
                foreach (var dexFile in Directory.GetFiles(ApkContentPath, "*.dex"))
                {
                    File.Copy(dexFile, Path.Combine(ApkDirectory, Path.GetFileName(dexFile)));
                }
            }

            foreach (var dexFile in Directory.GetFiles(ApkDirectory, "*.dex"))
            {
                Aapt.Remove(Path.GetFileName(ApkPath), Path.GetFileName(dexFile), ApkDirectory);
                Aapt.Add(Path.GetFileName(ApkPath), Path.GetFileName(dexFile), ApkDirectory);
                ConsoleWrapper.WriteInfo("AAPT", true, $"Readd DEX file {Path.GetFileName(dexFile)}");
                isEverProcessed = true;
                File.Delete(dexFile);
            }

            return isEverProcessed;
        }
    }
}
