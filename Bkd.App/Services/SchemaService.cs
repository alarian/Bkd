using System.Collections.Generic;
using Bkd.App.Models;
using Bkd.App.Repository;

namespace Bkd.App.Services
{
    public interface ISchemaService
    {
        List<ColumnDefinition> GetColumnsBy( string connectionName, string schema = null, string table = null, string column = null );
        List<TableDefinition> GetTablesBy( string connectionName, string schema = null, string table = null );
    }

    public class SchemaService : ISchemaService
    {
        private readonly IColumnDefinitionRepository _columnDefinitionRepository;
        private readonly ITableDefinitionRepository _tableDefinitionRepository;

        public SchemaService( IColumnDefinitionRepository columnDefinitionRepository, ITableDefinitionRepository tableDefinitionRepository )
        {
            _columnDefinitionRepository = columnDefinitionRepository;
            _tableDefinitionRepository = tableDefinitionRepository;
        }

        public List<ColumnDefinition> GetColumnsBy( string connectionName, string schema = null, string table = null, string column = null )
        {
            if( IsSchemaTableColumnPassed( schema, table, column ) )
                return new List<ColumnDefinition> {_columnDefinitionRepository.GetBy( connectionName, schema, table, column )};

            if( IsSchemaTablePassed( schema, table ) )
                return _columnDefinitionRepository.GetBy( connectionName, schema, table );

            if( IsSchemaPassed( schema ) )
                return _columnDefinitionRepository.GetBy( connectionName, schema );

            return _columnDefinitionRepository.Get( connectionName );
        }

        public List<TableDefinition> GetTablesBy( string connectionName, string schema = null, string table = null )
        {
            if( IsSchemaTablePassed( schema, table ) )
                return new List<TableDefinition> {_tableDefinitionRepository.GetBy( connectionName, schema, table )};

            if( IsSchemaPassed( schema ) )
                return _tableDefinitionRepository.GetBy( connectionName, schema );

            return _tableDefinitionRepository.Get( connectionName );
        }

        private bool IsSchemaPassed( string schema )
        {
            return !string.IsNullOrWhiteSpace( schema );
        }

        private bool IsSchemaTableColumnPassed( string schema, string table, string column )
        {
            return
                !string.IsNullOrWhiteSpace( schema ) &&
                !string.IsNullOrWhiteSpace( table ) &&
                !string.IsNullOrWhiteSpace( column );
        }

        private bool IsSchemaTablePassed( string schema, string table )
        {
            return
                !string.IsNullOrWhiteSpace( schema ) &&
                !string.IsNullOrWhiteSpace( table );
        }
    }
}