using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Companion.Models
{
    public class BesteldProduct
    {
        [Key]
        public int Id { get; set; } = default!;
        public int Aantal { get; set; }
        public string Opmerking { get; set; } = default!;
        public int ProductId { get; set; }
        public Product Product { get; set; } = default!;
    }
}
