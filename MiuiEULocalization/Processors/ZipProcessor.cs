using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace MiuiEULocalization.Processors
{
    public class ZipProcessor
    {
        public string Name { get; set; }

        public string ExtractFolder { get; set; }

        public ZipProcessor(string name, string extractFolder = null)
        {
            Name = name;
            ExtractFolder = extractFolder;
        }

        public void Extract()
        {
            string extractFolder = ExtractFolder ?? Path.GetFileNameWithoutExtension(Name);
            Directory.CreateDirectory(extractFolder);

            foreach (string imageName in new string[] { "system", "vendor" })
            {
                if (!File.Exists(Path.Combine(extractFolder, imageName + ".img")))
                {
                    using (ZipArchive za = new ZipArchive(new FileStream(Name, FileMode.Open)))
                    {
                        foreach (var entry in za.Entries)
                        {
                            if (entry.Name.StartsWith($"{imageName}."))
                            {
                                entry.ExtractToFile(Path.Combine(extractFolder, entry.Name), true);
                            }
                        }
                    }

                    foreach (var brFilePath in Directory.EnumerateFiles(extractFolder, "*.br"))
                    {
                        using (var brStream = new BrotliStream(new FileStream(brFilePath, FileMode.Open), CompressionMode.Decompress))
                        {
                            using (var uncompressedBrStream = new FileStream(brFilePath.Replace(".br", ""), FileMode.Create))
                            {
                                brStream.CopyTo(uncompressedBrStream);
                            }
                        }
                        File.Delete(brFilePath);
                    }

                    var process = Process.Start(new ProcessStartInfo()
                    {
                        FileName = "py",
                        Arguments = string.Join(" ", new string[] {
                            "-2",
                            Path.Combine("Resources", "sdat2img.py"),
                            Path.Combine(extractFolder, $"{imageName}.transfer.list"),
                            Path.Combine(extractFolder, $"{imageName}.new.dat"),
                            Path.Combine(extractFolder, $"{imageName}.img")
                        }),
                    });
                    process.WaitForExit();

                    File.Delete(Path.Combine(extractFolder, $"{imageName}.transfer.list"));
                    File.Delete(Path.Combine(extractFolder, $"{imageName}.new.dat"));
                    File.Delete(Path.Combine(extractFolder, $"{imageName}.patch.dat"));
                }
            }
        }
    }
}
