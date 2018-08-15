using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Bkd.App.Models;

namespace Bkd.App.Repository
{
    public interface IColumnDefinitionRepository
    {
        List<ColumnDefinition> Get( string connectionName );
        List<ColumnDefinition> GetBy( string connectionName, string schema );
        List<ColumnDefinition> GetBy( string connectionName, string schema, string table );
        ColumnDefinition GetBy( string connectionName, string schema, string table, string column );
    }

    public class ColumnDefinitionRepository : AdoRepository, IColumnDefinitionRepository
    {
        private const string OrderBy = @"
                        ORDER BY
	                        c.TABLE_SCHEMA ASC,
	                        c.TABLE_NAME ASC,
	                        c.ORDINAL_POSITION ASC
";

        private const string SqlQuery = @"
            SELECT
	             c.TABLE_SCHEMA
	            ,c.TABLE_NAME
	            ,c.COLUMN_NAME
	            ,c.DATA_TYPE
	            ,c.CHARACTER_MAXIMUM_LENGTH
	            ,CAST((CASE WHEN c.IS_NULLABLE = 'YES'THEN 1 ELSE 0 END) AS BIT) AS IS_NULLABLE
	            ,tc.CONSTRAINT_TYPE
	            ,kcu.CONSTRAINT_NAME
	            ,CAST(COALESCE(cc.is_computed, 0) AS BIT) 'IsComputed'
                ,sc.is_identity
                ,CAST(COALESCE(sc.generated_always_type , 0) AS BIT) 'IsGenerated'
	            ,CAST((CASE WHEN c.COLUMN_DEFAULT IS NULL THEN 0 ELSE 1 END) AS BIT) AS HAS_DEFAULT
            FROM INFORMATION_SCHEMA.COLUMNS c
            LEFT JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu 
	            ON 
			            c.TABLE_SCHEMA = kcu.TABLE_SCHEMA
		            AND	c.TABLE_NAME = kcu.TABLE_NAME
		            AND	c.COLUMN_NAME = kcu.COLUMN_NAME
            LEFT JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
	            ON
			            tc.CONSTRAINT_NAME = kcu.CONSTRAINT_NAME
            LEFT JOIN sys.computed_columns cc
	            ON
			            OBJECT_SCHEMA_NAME(cc.object_id) = c.TABLE_SCHEMA
		            AND	OBJECT_NAME(cc.object_id) = c.TABLE_NAME
		            AND	cc.[name] = c.COLUMN_NAME
            LEFT JOIN sys.columns sc
	            ON		
                        OBJECT_SCHEMA_NAME(sc.object_id) = c.TABLE_SCHEMA
		            AND	OBJECT_NAME(sc.object_id) = c.TABLE_NAME
		            AND	sc.[name] = c.COLUMN_NAME
            WHERE c.TABLE_NAME != 'sysdiagrams'
        ";

        public ColumnDefinitionRepository( ISqlConnectionManager connectionManager ) : base( connectionManager )
        {
        }

        public List<ColumnDefinition> Get( string connectionName )
        {
            return GetColumnDefinitions( new SqlDataCommand( $"{SqlQuery} {OrderBy}", connectionName ) );
        }

        public List<ColumnDefinition> GetBy( string connectionName, string schema, string table )
        {
            var where = @"AND c.TABLE_SCHEMA = @schema AND c.TABLE_NAME = @table";
            var command = new SqlDataCommand( $"{SqlQuery} {where} {OrderBy}", connectionName )
                          {
                              Parameters =
                              {
                                  ["@schema"] = schema,
                                  ["@table"] = table
                              }
                          };

            return GetColumnDefinitions( command );
        }

        public List<ColumnDefinition> GetBy( string connectionName, string schema )
        {
            var where = @"AND c.TABLE_SCHEMA = @schema";
            var command = new SqlDataCommand( $"{SqlQuery} {where} {OrderBy}", connectionName ) {Parameters = {["@schema"] = schema}};

            return GetColumnDefinitions( command );
        }

        public ColumnDefinition GetBy( string connectionName, string schema, string table, string column )
        {
            var where = @"AND c.TABLE_SCHEMA = @schema AND c.TABLE_NAME = @table AND c.COLUMN_NAME = @column";
            var command = new SqlDataCommand( $"{SqlQuery} {where} {OrderBy}", connectionName )
                          {
                              Parameters =
                              {
                                  ["@schema"] = schema,
                                  ["@table"] = table,
                                  ["@column"] = column
                              }
                          };

            //TODO: if column two key types, multiple rows returned!!!
            return GetColumnDefinitions( command ).FirstOrDefault();
        }

        private List<ColumnDefinition> GetColumnDefinitions( SqlDataCommand dataCommand )
        {
            var columnDefinitions = new List<ColumnDefinition>();

            void GetColumnDefinitions( SqlDataReader dataReader )
            {
                while( dataReader.Read() )
                {
                    var tableSchema = dataReader.GetString( 0 );
                    var tableName = dataReader.GetString( 1 );
                    var columnName = dataReader.GetString( 2 );
                    var dataType = dataReader.GetString( 3 );
                    var constraint = GetConstraint( dataReader );

                    var existingColumnDefinition = columnDefinitions.FirstOrDefault( cd =>
                                                                                         cd.TableSchema == tableSchema &&
                                                                                         cd.TableName == tableName &&
                                                                                         cd.ColumnName == columnName &&
                                                                                         cd.DataType == dataType );

                    if( existingColumnDefinition != null )
                    {
                        existingColumnDefinition.Constraints.Add( constraint );
                        continue;
                    }

                    var columnDefinition = new ColumnDefinition
                                           {
                                               TableSchema = tableSchema,
                                               TableName = tableName,
                                               ColumnName = columnName,
                                               DataType = dataType,
                                               CharacterMaxLength = !dataReader.IsDBNull( 4 ) ? dataReader.GetInt32( 4 ) : (long?) null,
                                               IsNullable = dataReader.GetBoolean( 5 ),
                                               Constraints = constraint == null ? null : new List<Constraint> {constraint},
                                               IsComputed = dataReader.GetBoolean( 8 ),
                                               IsIdentity = dataReader.GetBoolean( 9 ),
                                               IsGenerated = dataReader.GetBoolean( 10 ),
                                               HasDefault = dataReader.GetBoolean( 11 )
                                           };
                    columnDefinitions.Add( columnDefinition );
                }
            }

            ExecuteQuery( () => dataCommand, GetColumnDefinitions );

            return columnDefinitions;
        }

        private Constraint GetConstraint( SqlDataReader dataReader )
        {
            Constraint constraint = null;
            if( !dataReader.IsDBNull( 6 ) )
            {
                ConstraintType constraintType;
                var constraintTypeString = dataReader.GetString( 6 );
                if( !string.IsNullOrWhiteSpace( constraintTypeString ) )
                {
                    constraintType = (ConstraintType) Enum.Parse( typeof(ConstraintType), constraintTypeString.Replace( " ", "" ), true );

                    constraint = new Constraint
                                 {
                                     Type = constraintType,
                                     Name = !dataReader.IsDBNull( 7 ) ? dataReader.GetString( 7 ) : null
                                 };
                }
            }

            return constraint;
        }
    }
}