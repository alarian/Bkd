using System.Collections.Generic;

namespace Bkd.App.Models
{
    public class SqlDataCommand
    {
        public string CommandText { get; set; }

        public string ConnectionName { get; set; }

        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();

        public SqlDataCommand( string cmdText, string connectionName )
        {
            ConnectionName = connectionName;
            CommandText = cmdText;
        }
    }
}