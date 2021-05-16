using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiuiEULocalization.Models
{
    public class ConfigurationActionsCollection : ConfigurationFileBase, IEnumerable<ConfigurationActions>
    {
        private List<ConfigurationActions> collection = new List<ConfigurationActions>();

        public ConfigurationActionsCollection(string path) : base(path)
        {
            string[][] contents = Read();

            collection.AddRange(contents.Select(
                x => new ConfigurationActions
                {
                    Package = x[0],
                    Action = x[1],
                    Path = x[2],
                    Parameter = x[3]
                }
                ));
        }

        public IEnumerable<string> GetPackages() => collection.GroupBy(x => x.Package).Select(x => x.FirstOrDefault().Package);

        public IEnumerable<ConfigurationActions> GetByPackageName(string packageName) => collection.Where(x => x.Package == packageName);

        public IEnumerator<ConfigurationActions> GetEnumerator()
        {
            return ((IEnumerable<ConfigurationActions>)collection).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)collection).GetEnumerator();
        }
    }
}
