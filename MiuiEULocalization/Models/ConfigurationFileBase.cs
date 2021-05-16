using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MiuiEULocalization.Models
{
    public class ConfigurationFileBase
    {
        public string Path { get; private set; }

        public ConfigurationFileBase(string path)
        {
            Path = path;
        }

        public string[][] Read()
        {
            return File.ReadAllLines(Path).Skip(1).Where(x => !x.StartsWith("#")).Select(x => x.Split(',')).ToArray();
        }
    }
}
