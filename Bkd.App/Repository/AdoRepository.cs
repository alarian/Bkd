using System;
using System.Data.SqlClient;
using Bkd.App.Extensions;
using Bkd.App.Models;

namespace Bkd.App.Repository
{
    public abstract class AdoRepository
    {
        private readonly ISqlConnectionManager connectionManager;

        protected AdoRepository( ISqlConnectionManager connectionManager )
        {
            this.connectionManager = connectionManager;
        }

        protected void ExecuteNonQuery( Func<SqlDataCommand> commandFunc )
        {
            var command = commandFunc();

            using( var sqlCommand = command.ToSqlCommand( connectionManager ) )
            {
                if( string.IsNullOrWhiteSpace( sqlCommand.CommandText ) ) return;

                sqlCommand.ExecuteNonQuery();
            }
        }

        protected void ExecuteQuery( Func<SqlDataCommand> commandFunc, Action<SqlDataReader> action )
        {
            var command = commandFunc();

            using( var sqlCommand = command.ToSqlCommand( connectionManager ) )
            {
                sqlCommand.CommandTimeout = 360;

                using( var reader = sqlCommand.ExecuteReader() )
                {
                    do
                    {
                        action( reader );
                    }
                    while( reader.NextResult() );
                }
            }
        }
    }
}