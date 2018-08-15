namespace Bkd.App.Utilities
{
    public static class SqlTypeConverter
    {
        public static string ConvertSqlTypeToCSharpType( string sqlType )
        {
            switch( sqlType )
            {
                case "bit":
                    return "bool";

                case "tinyint":
                case "smallint":
                case "int":
                case "bigint":
                    return "int";

                case "decimal":
                case "numeric":
                case "smallmoney":
                case "money":
                    return "decimal";

                case "float":
                case "real":
                    return "float";

                case "char":
                case "varchar":
                case "text":
                case "nchar":
                case "nvarchar":
                case "ntext":
                    return "string";

                case "uniqueidentifier":
                    return "Guid";

                case "binary":
                case "varbinary":
                    return "byte []";

                case "datetime":
                case "datetimeoffset":
                case "datetime2":
                    return "DateTime";

                default:
                    return $"{sqlType}";
            }
        }
    }
}