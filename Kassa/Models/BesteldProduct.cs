using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Kassa.Models
{
    public class BesteldProduct
    {
        public int Aantal { get; set; }
        [JsonIgnore]
        public Bestelling? Bestelling { get; set; } = default!;

        public int BestellingId { get; set; }
        public string? Naam { get; set; }

        public int Id { get; set; }

        public string Opmerking { get; set; } = default!;
        
        public Product? Product { get; set; } = default!;

        public int ProductId { get; set; }
    }
}
