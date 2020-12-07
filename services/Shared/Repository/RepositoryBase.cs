using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;

namespace Koasta.Shared.Database
{
    public abstract class RepositoryBase<T>
    {
        protected RepositoryBase()
        {
            var type = typeof(T);
            SqlMapper.SetTypeMap(type, new CustomPropertyTypeMap(type, (_, columnName) => Array
                .Find(type.GetProperties(), prop => prop.GetCustomAttributes(false).OfType<ColumnAttribute>()
                .Any(attr => attr.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase)))));
        }
    }
}
