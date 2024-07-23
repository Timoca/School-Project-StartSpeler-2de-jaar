using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Companion.Models
{
    public class Drank
    {
        public int Id { get; set; }
        public string Naam { get; set; }
        public string Notities { get; set; }
        public decimal Prijs { get; set; }
        public int StockAantal { get; set; }
        public string Type { get; set; }
    }
}
