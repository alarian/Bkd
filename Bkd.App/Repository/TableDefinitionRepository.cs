using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Bkd.App.Models;

namespace Bkd.App.Repository
{
    public interface ITableDefinitionRepository
    {
        List<TableDefinition> Get( string connectionName );
        List<TableDefinition> GetBy( string connectionName, string schema );
        TableDefinition GetBy( string connectionName, string schema, string table );
    }

    public class TableDefinitionRepository : AdoRepository, ITableDefinitionRepository
    {
        private const string OrderBy = @"
            ORDER BY 
	             t.TABLE_SCHEMA ASC
                ,t.TABLE_NAME ASC
        ";

        private const string SqlQuery = @"
            SELECT 
                 t.TABLE_CATALOG
	            ,t.TABLE_SCHEMA
	            ,t.TABLE_NAME 
            FROM INFORMATION_SCHEMA.TABLES t
        ";

        public TableDefinitionRepository( ISqlConnectionManager connectionManager ) : base( connectionManager )
        {
        }

        public List<TableDefinition> Get( string connectionName )
        {
            var where = @"WHERE t.TABLE_TYPE = 'BASE TABLE' AND t.TABLE_NAME <> 'sysdiagrams'";
            return GetTableDefinitions( new SqlDataCommand( $"{SqlQuery} {where} {OrderBy}", connectionName ) );
        }

        public List<TableDefinition> GetBy( string connectionName, string schema )
        {
            var where = @"WHERE t.TABLE_TYPE = 'BASE TABLE' AND t.TABLE_SCHEMA = @schema AND t.TABLE_NAME <> 'sysdiagrams'";
            var command = new SqlDataCommand( $"{SqlQuery} {where} {OrderBy}", connectionName ) {Parameters = {["@schema"] = schema}};

            return GetTableDefinitions( command );
        }

        public TableDefinition GetBy( string connectionName, string schema, string table )
        {
            var where = @"WHERE t.TABLE_TYPE = 'BASE TABLE' AND t.TABLE_SCHEMA = @schema AND t.TABLE_NAME = @table AND t.TABLE_NAME <> 'sysdiagrams'";
            var command = new SqlDataCommand( $"{SqlQuery} {where} {OrderBy}", connectionName )
                          {
                              Parameters =
                              {
                                  ["@schema"] = schema,
                                  ["@table"] = table
                              }
                          };

            return GetTableDefinitions( command ).Single();
        }

        private List<TableDefinition> GetTableDefinitions( SqlDataCommand dataCommand )
        {
            var tableDefinitions = new List<TableDefinition>();

            void GetTableDefinitions( SqlDataReader dataReader )
            {
                while( dataReader.Read() )
                {
                    var tableDefinition = new TableDefinition
                                          {
                                              DatabaseName = dataReader.GetString( 0 ),
                                              TableSchema = dataReader.GetString( 1 ),
                                              TableName = dataReader.GetString( 2 )
                                          };
                    tableDefinitions.Add( tableDefinition );
                }
            }

            ExecuteQuery( () => dataCommand, GetTableDefinitions );

            return tableDefinitions;
        }
    }
}