using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bkd.App.Models
{
    public class ColumnDefinition
    {
        public string TableSchema { get; set; }
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public string DataType { get; set; }
        public long? CharacterMaxLength { get; set; }
        public bool? IsNullable { get; set; }
        public List<Constraint> Constraints { get; set; }
        public bool? IsComputed { get; set; }
        public bool? IsIdentity { get; set; }
        public bool? IsGenerated { get; set; }
        public bool? HasDefault { get; set; }
    }
}
