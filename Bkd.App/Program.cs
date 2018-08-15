using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using Bkd.App.Models;
using Bkd.App.Services;
using Ninject;
using Ninject.Extensions.Conventions;
using Bkd.App.Utilities;

namespace Bkd.App
{
    internal class Program
    {
        private static string destination;

        private static void Main( string[] args )
        {
            var kernel = new StandardKernel();

            kernel.Bind( x =>
                         {
                             x.FromThisAssembly()
                              .SelectAllClasses()
                              .BindDefaultInterface();
                         } );

            destination = ConfigurationManager.AppSettings["Location"];
            CreateDestinationFolder();

            var schemaService = kernel.Get<SchemaService>();
            ProcessTables( schemaService );
        }

        private static void CreateDestinationFolder()
        {
            if( !Directory.Exists( destination ) )
            {
                Directory.CreateDirectory( destination );
            }
        }

        private static void CreateDatabaseFolder( string databaseName )
        {
            var path = Path.Combine( destination, databaseName );
            if( !Directory.Exists( path ) )
            {
                Directory.CreateDirectory( path );
            }
        }


        private static void ProcessTables( ISchemaService schemaService )
        {
            var tables = schemaService.GetTablesBy("Avid");

            foreach ( var table in tables )
            {
                CreateDatabaseFolder( table.DatabaseName );
                var columns = schemaService.GetColumnsBy( "Avid", table.TableSchema, table.TableName );
                CreateClassFile( table, columns );
            }
        }

        private static void CreateClassFile( TableDefinition table, List<ColumnDefinition> columns )
        {
            var namespaceValue = ConfigurationManager.AppSettings["Namespace"];
            var classBody= new StringBuilder();

            var orderedColumns = columns.OrderByDescending( c => c.Constraints != null && c.Constraints.Exists( col => col.Type == ConstraintType.PrimaryKey ) );

            foreach( var column in orderedColumns )
            {
                var cSharpType = SqlTypeConverter.ConvertSqlTypeToCSharpType(column.DataType);
                classBody.AppendLine( $"        public {cSharpType} {column.ColumnName} {{ get; set; }}" );
            }


            var classContent = $"using System;{Environment.NewLine}{Environment.NewLine}namespace {namespaceValue}{Environment.NewLine}{{{Environment.NewLine}    public class {table.TableName}{Environment.NewLine}    {{{Environment.NewLine}{classBody}    }}{Environment.NewLine}}}";

            var path = Path.Combine(destination, table.DatabaseName);
            File.WriteAllText( Path.Combine(path, $"{table.TableName}.cs" ), classContent );
          
        }
    }
}