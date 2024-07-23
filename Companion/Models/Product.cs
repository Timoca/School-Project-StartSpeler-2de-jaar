using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Companion.Models
{
    public class Product
    {
        public int Id { get; set; } = default!;
        public string Naam { get; set; } = default!;
        public string Notities { get; set; } = default!;
        public float Prijs { get; set; } = default!;
        public int StockAantal { get; set; } = default!;
        public string Type { get; set; } = default!;
    }
}
