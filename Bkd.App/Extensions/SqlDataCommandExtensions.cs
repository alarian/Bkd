using System.Data.SqlClient;
using System.Linq;
using Bkd.App.Models;
using Bkd.App.Repository;

namespace Bkd.App.Extensions
{
    public static class SqlDataCommandExtensions
    {
        public static SqlCommand ToSqlCommand( this SqlDataCommand sqlDataCommand, ISqlConnectionManager connectionManager )
        {
            var sqlCommand = new SqlCommand( sqlDataCommand.CommandText );

            if( sqlDataCommand.Parameters != null && sqlDataCommand.Parameters.Count > 0 )
                sqlCommand.Parameters.AddRange( sqlDataCommand.Parameters.Select( p => new SqlParameter( p.Key, p.Value ) ).ToArray() );

            if( !string.IsNullOrEmpty( sqlDataCommand.ConnectionName ) )
                sqlCommand.Connection = connectionManager.GetConnection( sqlDataCommand.ConnectionName );

            return sqlCommand;
        }
    }
}