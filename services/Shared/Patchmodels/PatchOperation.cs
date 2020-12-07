using System.ComponentModel.DataAnnotations;

namespace Koasta.Shared.PatchModels
{
    public class PatchOperation<T>
    {
        [Required]
        public OperationKind Operation { get; set; }
        public T Value { get; set; }
    }
}
