using System.Collections.Generic;

namespace Koasta.Shared.Types
{
    public class PaginatedResult<T>
    {
        public int Count { get; set; }
        public List<T> Data { get; set; }
    }
}
