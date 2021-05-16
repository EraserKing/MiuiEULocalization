using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MiuiEULocalization.Processors
{
    public class ImageProcessor
    {
        public string ImagePath { get; set; }

        public string WorkingFolder { get; set; }

        public ImageProcessor(string imagePath, string workingFolder)
        {
            ImagePath = imagePath;
            WorkingFolder = workingFolder;
        }

        public void Extract(string originalPath)
        {
            Utilities.SevenZip.Extract(ImagePath, originalPath, WorkingFolder);
        }

        public string GetArch() => new string[] { "x86", "arm", "x86_64", "arm64" }.LastOrDefault(x => Utilities.SevenZip.FolderExist(ImagePath, $@"system\framework\{x}"));

        public string GetApi()
        {
            Extract(@"system\build.prop");
            string api = File.ReadAllLines(Path.Combine(Settings.WorkingDirectory, "system", "build.prop")).FirstOrDefault(x => x.StartsWith("ro.build.version.sdk"))?.Split('=')?[1] ?? "25";
            File.Delete(Path.Combine(Settings.WorkingDirectory, "system", "build.prop"));
            return api;
        }
    }
}
