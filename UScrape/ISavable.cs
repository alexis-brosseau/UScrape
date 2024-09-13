using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UScrape
{
    public enum SaveFormat
    {
        JSON,
        SQL
    }

    public interface ISavable
    {
        public string ToJSON();
        public string ToSQL(string table);
    }
}
