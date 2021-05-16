using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiuiEULocalization.Models
{
    public class ConfigurationExtractCollection : ConfigurationFileBase, IEnumerable<ConfigurationExtract>
    {
        private List<ConfigurationExtract> collection = new List<ConfigurationExtract>();

        public ConfigurationExtractCollection(string path) : base(path)
        {
            string[][] contents = Read();

            collection.AddRange(contents.Select(
                x => new ConfigurationExtract
                {
                    Source = x[0],
                    Image = x[1],
                    Path = x[2]
                }
                ));
        }

        public IEnumerator<ConfigurationExtract> GetEnumerator()
        {
            return ((IEnumerable<ConfigurationExtract>)collection).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)collection).GetEnumerator();
        }
    }
}
