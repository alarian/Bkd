using System;
using System.Data;
using System.Data.SqlClient;

namespace Bkd.App.Repository
{
    public interface ISqlConnectionManager
    {
        SqlConnection GetConnection( string connectionName );
    }

    public class SqlConnectionManager : IDisposable, ISqlConnectionManager
    {
        private static readonly object Lock = new object();
        private readonly Connections connections;

        private SqlConnection Connection { get; set; }

        public SqlConnectionManager( Connections connections )
        {
            this.connections = connections;
        }

        public void Dispose()
        {
            lock( Lock )
            {
                Connection?.Close();
                Connection?.Dispose();
            }
        }

        public SqlConnection GetConnection( string connectionName )
        {
            lock( Lock )
            {
                if( Connection != null )
                {
                    if( Connection.State == ConnectionState.Open )
                        return Connection;

                    Connection.Open();
                    return Connection;
                }

                Connection = new SqlConnection( connections[connectionName] );
                Connection.Open();

                return Connection;
            }
        }
    }
}