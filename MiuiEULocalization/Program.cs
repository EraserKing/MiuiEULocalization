using MiuiEULocalization.Models;
using MiuiEULocalization.Processors;
using MiuiEULocalization.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;

namespace MiuiEULocalization
{
    public class Program
    {
        static void Main(string[] args)
        {
            new ZipProcessor("miui_MIMIX2S_V12.0.3.0.QDGCNXM_cd55623678_10.0.zip", Settings.ChinaSourceDirectory).Extract();
            new ZipProcessor("xiaomi.eu_multi_MIMix2S_V12.0.3.0.QDGCNXM_v12-10.zip", Settings.GlobalSourceDirectory).Extract();

            ImageProcessor systemImage = new ImageProcessor(Path.Combine("c", Settings.SystemImageName), Settings.WorkingDirectory);

            Settings.Api = systemImage.GetApi();
            Settings.Arch = systemImage.GetArch();

            Dictionary<string, string> packageToPath = new Dictionary<string, string>();

            ConfigurationExtractCollection extracts = new ConfigurationExtractCollection(Path.Combine("Configuration", "extract.txt"));
            foreach (var extract in extracts)
            {
                new ImageProcessor(Path.Combine(extract.Source, extract.Image), Settings.WorkingDirectory).Extract(extract.Path);
                packageToPath.Add(extract.Path.Split('\\').Last(), extract.Path);
            }

            ConfigurationActionsCollection actions = new ConfigurationActionsCollection(Path.Combine("Configuration", "actions.txt"));
            foreach (var packageName in actions.GetPackages())
            {
                ApkProcessor apkProcessor = new ApkProcessor(Directory.GetFiles(Path.Combine(Settings.WorkingDirectory, packageToPath[packageName]), "*.apk").First());
                apkProcessor.Disassemble();

                try
                {
                    foreach (var action in actions.GetByPackageName(packageName))
                    {
                        if (action.Action == "MoveArch")
                        {
                            apkProcessor.MoveArch(action.Parameter.Split('>')[0], action.Parameter.Split('>')[1]);
                            continue;
                        }

                        else if (action.Action == "Decompile")
                        {
                            apkProcessor.Disassemble();
                        }

                        else if (action.Path.EndsWith(".smali"))
                        {
                            bool _ = action.Action switch
                            {
                                "PatchMethod" => apkProcessor.CreateSmaliProcessor(action.Path, action.Parameter).PatchMethod(action.Parameter),
                                "RemoveLine" => apkProcessor.CreateSmaliProcessor(action.Path, action.Parameter).RemoveLine(action.Parameter),
                                "UpdateGlobalFlag" => apkProcessor.CreateSmaliProcessor(action.Path, action.Parameter).UpdateGlobalFlag(),
                                "UpdateInternationalFlag" => apkProcessor.CreateSmaliProcessor(action.Path, action.Parameter).UpdateInternationFlag(),
                                "UpdateDeviceMod" => apkProcessor.CreateSmaliProcessor(action.Path, action.Parameter).UpdateDeviceMod(),
                                "UpdateDeviceRegion" => apkProcessor.CreateSmaliProcessor(action.Path, action.Parameter).UpdateDeviceRegion(),
                                _ => throw new ArgumentException($"Unrecognized action {action.Action}")
                            };
                        }

                        else
                        {
                            IEnumerable<bool> _ = action.Action switch
                            {
                                "PatchMethod" => throw new ArgumentException($"Unable to patch method for directory {action.Path}"),
                                "RemoveLine" => throw new ArgumentException($"Unable to remove line for directory {action.Path}"),
                                "UpdateGlobalFlag" => apkProcessor.CreateSmaliProcessors(action.Path, action.Parameter).Select(x => x.UpdateGlobalFlag(true)).ToArray(),
                                "UpdateInternationalFlag" => apkProcessor.CreateSmaliProcessors(action.Path, action.Parameter).Select(x => x.UpdateInternationFlag(true)).ToArray(),
                                "UpdateDeviceMod" => apkProcessor.CreateSmaliProcessors(action.Path, action.Parameter).Select(x => x.UpdateDeviceMod(true)).ToArray(),
                                "UpdateDeviceRegion" => apkProcessor.CreateSmaliProcessors(action.Path, action.Parameter).Select(x => x.UpdateDeviceRegion(true)).ToArray(),
                                _ => throw new ArgumentException($"Unrecognized action {action.Action}")
                            };
                        }
                    }
                }
                catch (ArgumentException ex)
                {
                    ConsoleWrapper.WriteError("GENERAL", ex.Message);
                }

                apkProcessor.Assemble(Settings.Api);
                // apkProcessor.ExtractLib();
                apkProcessor.AddDexClasses();

                if (Settings.Cleanup)
                {
                    foreach (var tempSmaliDirectory in Directory.GetDirectories(apkProcessor.ApkContentPath, "*_smali"))
                    {
                        Directory.Delete(tempSmaliDirectory, true);
                    }

                    foreach (var tempDexFile in Directory.GetFiles(apkProcessor.ApkContentPath, "*_x.dex"))
                    {
                        File.Move(tempDexFile, tempDexFile.Replace("_x.dex", ".dex"), true);
                    }
                }
            }
        }
    }
}
